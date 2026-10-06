// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-05-07 by Linus Ziesel

﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays bridging sounds (like a hum) after the user stops speaking to fill silence while the AI processes.
/// </summary>
public class BridgingSounds : MonoBehaviour
{
    
    public AudioClip hum;
    private AudioSource bridgeAudioSource;
    void Start()
    {
        bridgeAudioSource = GameObject.FindGameObjectsWithTag("BridgeAudioSource")[0].GetComponent<AudioSource>();
        if (!bridgeAudioSource) Debug.LogError("no tagged bridge audio source found");
            
        bridgeAudioSource.clip = hum;
        AIManagerMain.OnStopListening += () =>
        {
            Invoke(nameof(Hum), 1.5f); 
        };
    }

    private void Hum()
    {
        bridgeAudioSource.Play();
    }
   
}