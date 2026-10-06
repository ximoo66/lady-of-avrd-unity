// Authors: Linus Ziesel, Ekaterina Siling, Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-07-15 by Noah Wendt

﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.UI;
using WebSocket = NativeWebSocket.WebSocket;
using WebSocketState = NativeWebSocket.WebSocketState;
using System.Linq;

/// <summary>
/// Manages websocket communication, microphone recording, and audio playback to facilitate real-time conversations with the AI backend.
/// \n 
/// Also receives the tool calls. In fact, it receives and manages everything that is sent by the AI Backend.
/// </summary>
///
public enum LLMMode
{
    ConversationMode,
    ImageMode,
    ModelMode,
    MindmapMode,
}

[RequireComponent(typeof(AudioSource))]
public class AIManagerMain : MonoBehaviour
{
    private const byte HEADER_JSON = 0x01;
    private const byte HEADER_AUDIO = 0x02;

    private string serverUrl = LocalConfig.serverUrl;
    private Dictionary<string, string> webSocketHeaders = new();
    private int frequency = 16000;
    private int maxRecordingTime = 10;

    public WebSocket websocket;
    private AudioSource audioSource;
    private AudioClip recordingClip;
    private Coroutine resetIndicatorCoroutine;
    private UTF8Encoding encoding = new();

    private string micName;
    private bool isRecording = false;
    private bool isAIsTurn;
    private bool conversationStarted;
    private MindMapToolCalls mindMapToolCalls;

    public static event Action<string> OnToolCallReceived;
    public static event Action<string, Dictionary<string, string>> OnToolCallWithArgsReceived;

    public static event Action OnStartListening;
    public static event Action OnStopListening;
    public static event Action OnStartSpeaking;
    public static event Action OnError;
    public static event Action OnNoSpeechDetected;

    public static event Action<string> OnTranscriptionFinished;

    public static event Action<Dictionary<string, string>> OnServerStartedGen;
    public static event Action<string> OnImgReceived;
    public static event Action<string> OnModelReceived;

    public static event Action<LLMMode> OnModeChanged;

    private ConcurrentQueue<float> audioBuffer = new ConcurrentQueue<float>();

    double inputRate;
    double systemRate;
    float stepSize;

    byte[] MAGIC_LLM1 = Encoding.ASCII.GetBytes("LLM1"); // convo mode
    byte[] MAGIC_LLM2 = Encoding.ASCII.GetBytes("LLM2"); // mindmap mode
    byte[] MAGIC_LLM3 = Encoding.ASCII.GetBytes("LLM3"); // image/generative mode
    byte[] MAGIC_LLM4 = Encoding.ASCII.GetBytes("LLM4"); // model/spare mode

    [HideInInspector]
    public byte[] MAGIC_TXT = Encoding.ASCII.GetBytes("TXT1"); // transcription

    public const int MAGIC_LEN = 4;

    public LLMMode currentMode = LLMMode.ConversationMode; //defaulting mode to convo

    private void Start()
    {
        inputRate = 24000.0;
        systemRate = AudioSettings.outputSampleRate;
        stepSize = (float)(inputRate / systemRate);

        audioSource = GetComponent<AudioSource>();

        mindMapToolCalls = FindObjectOfType<MindMapToolCalls>();

        var encodedHeaders = Convert.ToBase64String(
            System.Text.ASCIIEncoding.ASCII.GetBytes(
                $"{LocalConfig.serverBasicAuthUsername}:{LocalConfig.serverBasicAuthPassword}"
            )
        );

        webSocketHeaders["Authorization"] = $"Basic {encodedHeaders}";

#if UNITY_EDITOR
        serverUrl = LocalConfig.editorServerUrl;

        string preferredMic = "Headset (Cisco’s AirPods Pro - Find My)";
        micName = Microphone.devices.FirstOrDefault(m => m.Contains(preferredMic));

        if (string.IsNullOrEmpty(micName))
            micName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        Debug.Log($"Connected with micro: {micName}");
#else
        micName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        Debug.Log($"Connected with micro: {micName}");
#endif

        SetupSocket();
        //   StartingConversation();
    }

    public void SwitchMode(LLMMode mode)
    {
        if (mode == currentMode)
        {
            Debug.Log("Switching to the same mode");
        }
        else
        {
            currentMode = mode;

            Debug.Log("Switched to mode: " + mode.ToString());

            OnModeChanged?.Invoke(currentMode);
        }
    }

    private async void SetupSocket()
    {
        websocket = new WebSocket(serverUrl, webSocketHeaders);

        websocket.OnOpen += () =>
        {
            Debug.Log("WS Connected");
        };

        websocket.OnError += (e) =>
            Debug.LogError("WS Error: " + e);

        websocket.OnClose += (e) =>
            Debug.Log("WS Closed");

        websocket.OnMessage += (bytes) =>
        {
            if (bytes.Length == 0)
                return;

            byte header = bytes[0];

            switch (header)
            {
                case HEADER_JSON:
                    ProcessJsonMessage(bytes);
                    break;

                case HEADER_AUDIO:
                    // Pass offset 1 to skip header
                    ProcessAudioChunk(
                        bytes,
                        1,
                        bytes.Length - 1
                    );
                    break;

                default:
                    Debug.LogWarning(
                        $"Unknown header byte: {header}"
                    );
                    break;
            }
        };

        await websocket.Connect();
    }

    private void ProcessJsonMessage(byte[] bytes)
    {
        // Skip the first byte (header)
        string jsonString = encoding.GetString(
            bytes,
            1,
            bytes.Length - 1
        );

        ServerMessage msg =
            JsonConvert.DeserializeObject<ServerMessage>(jsonString);

        if (msg == null)
        {
            return;
        }

        switch (msg.type)
        {
            case "tool":
                Debug.Log(
                    $"<color=green>Tool Call:</color> {msg.content}"
                );

                OnToolCallReceived.Invoke(msg.content);
                break;

            case "tool_with_args":
                Debug.Log(
                    $"<color=cyan>Tool With Args:</color> " +
                    $"{msg.content} | args: {FormatArgs(msg.args)}"
                );

                OnToolCallWithArgsReceived?.Invoke(
                    msg.content,
                    msg.args ?? new Dictionary<string, string>()
                );
                break;

            case "control":
                ResetAIsTurn();
                break;

            case "error":
                if (msg.content == "no_speech")
                {
                    HandleNoSpeechDetected();
                }
                else
                {
                    Debug.Log(
                        $"<color=red>Unhandled Server Error:</color> {msg.content}"
                    );

                    isAIsTurn = false;
                }

                break;

            case "txt1":
                Debug.Log(
                    $"<color=pink>Transcribed:</color> {msg.content}"
                );

                OnTranscriptionFinished?.Invoke(msg.content);
                break;

            case "started_gen":
                Debug.Log(
                    $"<color=yellow>Server started generation with:</color> " +
                    $"{msg.args["prompt"]}"
                );

                OnServerStartedGen.Invoke(msg.args);
                break;

            case "image_result":
                OnImgReceived?.Invoke(msg.content);

                Debug.Log(
                    "[image_result] received, invoking event."
                );
                break;

            case "model_result":
                OnModelReceived?.Invoke(msg.content);

                Debug.Log(
                    "[model_result] received, invoking event."
                );
                break;
        }
    }

    private string FormatArgs(
        Dictionary<string, string> args)
    {
        if (args == null || args.Count == 0)
            return "(none)";

        return string.Join(
            ", ",
            args.Select(kv => $"{kv.Key}={kv.Value}")
        );
    }

    private void HandleNoSpeechDetected()
    {
        Debug.LogWarning(
            "No speech detected. Try again."
        );

        //OnError?.Invoke();
        OnNoSpeechDetected?.Invoke();

        isAIsTurn = false;
    }

    private void ResetAIsTurn()
    {
        isAIsTurn = false;

        Debug.Log(
            $"[AIManagerMain] AI turn finished. " +
            $"Ready for next orb. Current mode: {currentMode}"
        );
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
            websocket.DispatchMessageQueue();
#endif
    }

    [Sirenix.OdinInspector.Button]
    public void StartRec(bool processLabelName = false)
    {
        StartingConversation();

        Debug.Log(
            $"[AIManagerMain] StartRec | " +
            $"Mode: {currentMode} | " +
            $"isAIsTurn: {isAIsTurn} | " +
            $"isRecording: {isRecording} | " +
            $"conversationStarted: {conversationStarted}"
        );

        if (processLabelName)
        {
            if (isRecording)
                return;

            recordingClip = Microphone.Start(
                micName,
                false,
                maxRecordingTime,
                frequency
            );

            if (recordingClip == null)
            {
                Debug.LogError(
                    "Microphone failed to start. RecordingClip is null."
                );

                return;
            }

            isRecording = true;

            Debug.Log("Recording label...");

            return;
        }

        if (isAIsTurn || !conversationStarted)
        {
            Debug.LogWarning(
                $"[AIManagerMain] Recording blocked. " +
                $"isAIsTurn: {isAIsTurn}, " +
                $"conversationStarted: {conversationStarted}"
            );

            return;
        }

        OnStartListening?.Invoke();

        if (isRecording)
            return;

        recordingClip = Microphone.Start(
            micName,
            false,
            maxRecordingTime,
            frequency
        );

        if (recordingClip == null)
        {
            Debug.LogError(
                "Microphone failed to start. RecordingClip is null."
            );

            return;
        }

        isRecording = true;

        Debug.Log(
            $"[AIManagerMain] Recording started in mode: {currentMode}"
        );
    }

    [Sirenix.OdinInspector.Button]
    public async void StopAndSend(
        bool processLabelName = false)
    {
        if (isAIsTurn || !conversationStarted)
        {
            return;
        }

        isAIsTurn = true;

        OnStopListening?.Invoke();

        if (!isRecording)
        {
            isAIsTurn = false;
            return;
        }

        if (recordingClip == null)
        {
            Debug.LogError(
                "RecordingClip is null but isRecording is true. Resetting."
            );

            isRecording = false;
            isAIsTurn = false;

            return;
        }

        int pos = Microphone.GetPosition(micName);

        Microphone.End(micName);

        isRecording = false;

        float[] samples =
            new float[pos * recordingClip.channels];

        recordingClip.GetData(samples, 0);

        SoundUtils.NormalizeAudio(samples);

        byte[] wavBytes = SoundUtils.EncodeAsWAV(
            samples,
            recordingClip.frequency,
            recordingClip.channels
        );

        byte[] prefix = GetModePrefix(currentMode);

        if (websocket != null &&
            websocket.State == WebSocketState.Open)
        {
            byte[] combined;

            Debug.Log(
                $"Sending {wavBytes.Length} bytes " +
                $"with mode: {currentMode}"
            );

            if (processLabelName)
            {
                combined =
                    new byte[MAGIC_LEN + wavBytes.Length];

                Array.Copy(
                    MAGIC_TXT,
                    0,
                    combined,
                    0,
                    MAGIC_LEN
                );

                Array.Copy(
                    wavBytes,
                    0,
                    combined,
                    MAGIC_LEN,
                    wavBytes.Length
                );

                isAIsTurn = false;
            }
            else
            {
                combined =
                    new byte[MAGIC_LEN + wavBytes.Length];

                Array.Copy(
                    prefix,
                    0,
                    combined,
                    0,
                    MAGIC_LEN
                );

                Array.Copy(
                    wavBytes,
                    0,
                    combined,
                    MAGIC_LEN,
                    wavBytes.Length
                );
            }

            await websocket.Send(combined);

            //SendGraphStateToBackend();
        }
        else
        {
            Debug.LogError(
                "Websocket connection was not open, when sending."
            );

            isAIsTurn = false;
        }
    }

    //case switch for the prefix depending on the mode of LLM
    public byte[] GetModePrefix(LLMMode llmMode)
    {
        switch (llmMode)
        {
            case LLMMode.ConversationMode:
                return MAGIC_LLM1;

            case LLMMode.MindmapMode:
                return MAGIC_LLM2;

            case LLMMode.ImageMode:
                return MAGIC_LLM3;

            case LLMMode.ModelMode:
                return MAGIC_LLM4;

            default:
                Debug.LogError(
                    $"Unknown LLM mode: {llmMode}"
                );

                return null;
        }
    }

    public static string CurrentMindMapState =
        "MindMap: empty";

    private async void SendGraphStateToBackend()
    {
        var payload = new
        {
            type = "graph_state",
            content = CurrentMindMapState
        };

        string json = JsonConvert.SerializeObject(payload);

        await websocket.Send(
            Encoding.UTF8.GetBytes("\x01" + json)
        );
    }

    private void ProcessAudioChunk(
        byte[] pcmBytes,
        int offset,
        int length)
    {
        OnStartSpeaking?.Invoke();

        int sampleCount = length / 2;

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(
                pcmBytes,
                offset + i * 2
            );

            audioBuffer.Enqueue(
                sample / 32768f
            );
        }

        if (!audioSource.isPlaying &&
            audioBuffer.Count > 4000)
        {
            audioSource.clip = AudioClip.Create(
                "Streaming",
                24000,
                1,
                24000,
                true,
                (data) => { }
            );

            audioSource.loop = true;

            audioSource.Play();
        }
    }

    private float fractionalPart = 0f;
    private float currentSample = 0f;
    private float nextSample = 0f;
    private bool hasCurrent = false;
    private bool hasNext = false;

    private void OnAudioFilterRead(
        float[] data,
        int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            if (!hasNext)
            {
                if (audioBuffer.TryDequeue(out float val))
                {
                    nextSample = val;
                    hasNext = true;
                }
                else
                {
                    nextSample = 0f;
                }
            }

            if (!hasCurrent)
            {
                currentSample = nextSample; // Initialize
                hasCurrent = true;
            }

            float outputVal = Mathf.Lerp(
                currentSample,
                nextSample,
                fractionalPart
            );

            for (int c = 0; c < channels; c++)
            {
                data[i + c] = outputVal;
            }

            fractionalPart += stepSize;

            if (fractionalPart >= 1.0f)
            {
                currentSample = nextSample;
                hasCurrent = true; // We have a valid current
                hasNext = false;   // We need to fetch a new next
                fractionalPart -= 1.0f;
            }
        }
    }

    [ContextMenu("Start Conversation")]
    public void StartingConversation()
    {
        if (conversationStarted)
        {
            return;
        }

        conversationStarted = true;

        Debug.Assert(
            websocket != null &&
            websocket.State == WebSocketState.Open,
            "Websocket connection was not open, when sending."
        );

        websocket.SendText("any"); // send any string to start
    }

    public async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}

[Serializable]
public class ServerMessage
{
    public string type;
    public string content;
    public Dictionary<string, string> args;
}
