// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-07 by Linus Ziesel
// Last Modified: 2026-07-07 by Linus Ziesel

﻿using UnityEngine;
using System;
using System.Linq;
using System.Text;
using NativeWebSocket;
using Sirenix.OdinInspector;
using Newtonsoft.Json;

[RequireComponent(typeof(AIManagerMain))]
public class AIManagerDebug : MonoBehaviour
{
    private AIManagerMain aiManager;

    [Tooltip("The text prompt to send to the server.")]
    public string prompt;

    [Tooltip("If true, the script will attempt to spawn a received GLB model in the scene.")]
    public bool spawnModel = true;

    private void Awake()
    {
        aiManager = GetComponent<AIManagerMain>();
        if (aiManager == null)
        {
            Debug.LogError("AIManagerMain component not found!");
        }
    }

    private void OnEnable()
    {
        if (aiManager != null && aiManager.websocket != null)
        {
            aiManager.websocket.OnMessage += HandleDebugMessage;
        }
    }

    private void OnDisable()
    {
        if (aiManager != null && aiManager.websocket != null)
        {
            aiManager.websocket.OnMessage -= HandleDebugMessage;
        }
    }

    private async void HandleDebugMessage(byte[] bytes)
    {
        if (!spawnModel || bytes.Length == 0 || bytes[0] != 0x01)
        {
            return;
        }

        try
        {
            string jsonString = Encoding.UTF8.GetString(bytes, 1, bytes.Length - 1);
            ServerMessage msg = JsonConvert.DeserializeObject<ServerMessage>(jsonString);

            if (msg != null && msg.type == "model_result")
            {
                Debug.Log("[AIManagerDebug] Intercepted model_result. Attempting to spawn. not implemented");
                // await FileUtils.SpawnGlbFromB64(msg.content, new (0, 0, 0));
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AIManagerDebug] Could not process message for spawning: {e.Message}");
        }
    }

    [Button("Send Prompt")]
    private void SendPrompt()
    {
        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogWarning("Prompt is empty, skipping.");
            return;
        }

        // Start the conversation if it hasn't been started yet
        aiManager.StartingConversation();

        SendMessageWithPrefix(aiManager.currentMode, prompt);
    }

    private async void SendMessageWithPrefix(LLMMode mode, string text)
    {
        if (aiManager.websocket == null)
        {
            Debug.LogError("WebSocket is null. Cannot send message.");
            return;
        }
        
        // Wait until the websocket is connected
        while (aiManager.websocket.State == WebSocketState.Connecting)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        if (aiManager.websocket.State != WebSocketState.Open)
        {
            Debug.LogError($"WebSocket is not open. Current state: {aiManager.websocket.State}");
            return;
        }

        byte[] prefix = aiManager.GetModePrefix(mode);

        if (prefix == null)
        {
            Debug.LogError($"Could not find prefix for mode: {mode}");
            return;
        }
        
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        
        byte[] message = new byte[AIManagerMain.MAGIC_LEN + textBytes.Length];
        Array.Copy(prefix, 0, message, 0, AIManagerMain.MAGIC_LEN);
        Array.Copy(textBytes, 0, message, AIManagerMain.MAGIC_LEN, textBytes.Length);

        string prefixString = Encoding.ASCII.GetString(prefix);
        string payloadAsHex = string.Join(" ", message.Select(b => b.ToString("X2")));

        Debug.Log($"<color=yellow>--- SENDING DEBUG PROMPT ---</color>");
        Debug.Log($"<b>Mode:</b> {mode.ToString()}");
        Debug.Log($"<b>Prefix:</b> {prefixString} (Bytes: {string.Join(" ", prefix.Select(b => b.ToString("X2")))})");
        Debug.Log($"<b>Prompt:</b> {text}");
        Debug.Log($"<b>Full Payload ({message.Length} bytes):</b> {payloadAsHex}");
        
        await aiManager.websocket.Send(message);
        Debug.Log($"<color=green>--- PROMPT SENT ---</color>");
    }
}