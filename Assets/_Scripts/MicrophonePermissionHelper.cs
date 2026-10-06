// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-05-07 by Linus Ziesel

using UnityEngine;
using UnityEngine.Android; // Required for handling Android permissions

/// <summary>
/// Ensures the application has the necessary microphone permissions required for recording user audio.
/// </summary>
public class MicrophonePermissionHelper : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Start() in MicrophonePermissionHelper");
#if PLATFORM_ANDROID
        // 1. Check if we already have permission
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // 2. If not, request it. This triggers the Quest system dialog.
            Permission.RequestUserPermission(Permission.Microphone);
            Debug.Log("Permission requesting");
        }
        else
        {
            Debug.Log("Microphone permission already granted.");
            // You can initialize your audio logic here
        }
#endif
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.LogError("No Microphone permission!");
        }
    }

    // Optional: Call this to check status before starting a recording
    public bool IsMicrophoneReady()
    {
        Debug.Log("IsMicrophoneReady() in MicrophonePermissionHelper");
#if PLATFORM_ANDROID
        return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
        return true; // Assume yes for Editor testing
#endif
    }
}