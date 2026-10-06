// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-20 by Omid Ameri
// Last Modified: 2026-06-20 by Omid Ameri

// Course: P6

using UnityEngine;

public class ConversationActiveOrbToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIManagerMain aiManagerMain;
    [SerializeField] private Renderer orbRenderer;
    [SerializeField] private Light orbLight;

    [Header("Shader")]
    [SerializeField] private string colorPropertyName = "_Color";

    [Header("Microphone State Colors")]
    [ColorUsage(true, true)]
    [SerializeField] private Color microphoneOffColor = Color.red;

    [ColorUsage(true, true)]
    [SerializeField] private Color microphoneOnColor = Color.green;

    [SerializeField] private float colorIntensity = 1.5f;

    [Header("Behaviour")]
    [SerializeField] private bool reapplyColorWhileActive = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private MaterialPropertyBlock propertyBlock;
    private bool isRecording;
    private bool warnedMissingProperty;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        AIManagerMain.OnStartListening += HandleStartListening;
        AIManagerMain.OnStopListening += HandleStopListening;
        AIManagerMain.OnError += HandleStopListening;
        AIManagerMain.OnModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        AIManagerMain.OnStartListening -= HandleStartListening;
        AIManagerMain.OnStopListening -= HandleStopListening;
        AIManagerMain.OnError -= HandleStopListening;
        AIManagerMain.OnModeChanged -= HandleModeChanged;
    }

    private void Start()
    {
        RefreshVisualState();
    }

    private void LateUpdate()
    {
        if (!reapplyColorWhileActive)
        {
            return;
        }

        if (aiManagerMain == null)
        {
            return;
        }

        ApplyColor(isRecording ? microphoneOnColor : microphoneOffColor);
    }

    public void ToggleConversationRecording()
    {
        if (aiManagerMain == null)
        {
            Debug.LogWarning("ConversationActiveOrbToggle has no AIManagerMain assigned.");
            return;
        }

        if (isRecording)
        {
            Log($"Stopping recording in mode: {aiManagerMain.currentMode}");
            aiManagerMain.StopAndSend();
        }
        else
        {
            Log($"Starting recording in mode: {aiManagerMain.currentMode}");
            aiManagerMain.StartRec();
        }
    }

    private void HandleStartListening()
    {
        Log($"MIC ON EVENT in mode: {aiManagerMain.currentMode}");

        isRecording = true;
        RefreshVisualState();
    }

    private void HandleStopListening()
    {
        Log("MIC OFF EVENT");

        isRecording = false;
        RefreshVisualState();
    }

    private void HandleModeChanged(LLMMode mode)
    {
        Log($"Mode changed to: {mode}");

        if (isRecording)
        {
            Log("Mode changed while recording. Stopping current recording first.");
            aiManagerMain.StopAndSend();
            return;
        }

        RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        if (aiManagerMain == null)
        {
            return;
        }

        ApplyColor(isRecording ? microphoneOnColor : microphoneOffColor);
    }

    [ContextMenu("Test Color/Mic Off Red")]
    private void TestMicOffRed()
    {
        ApplyColor(microphoneOffColor);
    }

    [ContextMenu("Test Color/Mic On Green")]
    private void TestMicOnGreen()
    {
        ApplyColor(microphoneOnColor);
    }

    private void ApplyColor(Color color)
    {
        if (orbRenderer == null)
        {
            Debug.LogWarning("ConversationActiveOrbToggle has no Orb Renderer assigned.");
            return;
        }

        Color finalColor = color * colorIntensity;

        orbRenderer.GetPropertyBlock(propertyBlock);

        if (orbRenderer.sharedMaterial != null &&
            orbRenderer.sharedMaterial.HasProperty(colorPropertyName))
        {
            propertyBlock.SetColor(colorPropertyName, finalColor);
        }
        else if (!warnedMissingProperty)
        {
            warnedMissingProperty = true;
            Debug.LogWarning(
                $"Material does not have shader property '{colorPropertyName}'. " +
                "Check the Shader Graph property Reference name."
            );
        }

        orbRenderer.SetPropertyBlock(propertyBlock);

        if (orbLight != null)
        {
            orbLight.color = color;
            orbLight.intensity = colorIntensity;
        }
    }

    private void Log(string message)
    {
        if (!showDebugLogs)
        {
            return;
        }

        Debug.Log($"[ConversationActiveOrbToggle] {message}");
    }
}