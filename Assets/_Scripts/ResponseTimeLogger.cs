// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-05-07 by Linus Ziesel

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Measures and logs the latency between the user finishing speaking and the AI starting its response.
/// </summary>
public static class ResponseTimeLogger
{
    private static float? _stopListeningTime;
    private static readonly List<float> _responseTimes = new List<float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        // Clear previous data to handle Domain Reload disabled cases
        _responseTimes.Clear();
        _stopListeningTime = null;

        // Unsubscribe first to prevent duplicate subscriptions
        AIManagerMain.OnStopListening -= HandleStopListening;
        AIManagerMain.OnStartSpeaking -= HandleStartSpeaking;
        Application.quitting -= SaveSessionData;

        AIManagerMain.OnStopListening += HandleStopListening;
        AIManagerMain.OnStartSpeaking += HandleStartSpeaking;
        Application.quitting += SaveSessionData;
        
        Debug.Log("ResponseTimeLogger Initialized");
    }

    private static void HandleStopListening()
    {
        _stopListeningTime = Time.realtimeSinceStartup;
    }

    private static void HandleStartSpeaking()
    {
        if (_stopListeningTime.HasValue)
        {
            float duration = Time.realtimeSinceStartup - _stopListeningTime.Value;
            _responseTimes.Add(duration);
            _stopListeningTime = null; // Reset to ensure we only capture the first chunk's time
            Debug.Log($"[ResponseTimeLogger] Response time: {duration:F3}s");
        }
    }

    private static void SaveSessionData()
    {
        if (_responseTimes.Count == 0) return;

        string directoryPath;
#if UNITY_EDITOR
        // In Editor, save to the project root for easy access
        directoryPath = Path.Combine(Application.dataPath, "../ResponseLogs");
#else
        // On Quest/Android, save to persistent data path
        directoryPath = Path.Combine(Application.persistentDataPath, "ResponseLogs");
#endif

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var sessionData = new
        {
            SessionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            AverageResponseTime = _responseTimes.Average(),
            ResponseTimes = _responseTimes
        };

        string fileName = $"Session_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath = Path.Combine(directoryPath, fileName);

        try
        {
            string json = JsonConvert.SerializeObject(sessionData, Formatting.Indented);
            File.WriteAllText(fullPath, json);
            Debug.Log($"[ResponseTimeLogger] Session saved to: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ResponseTimeLogger] Failed to save session data: {e.Message}");
        }
    }
}
