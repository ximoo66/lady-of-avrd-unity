// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-07 by Noah Wendt
// Last Modified: 2026-07-16 by Noah Wendt

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ThinkingVoiceLineTrigger : MonoBehaviour
{
    public List<AudioClip> thinkingVoiceLines = new List<AudioClip>();
    public List<AudioClip> noSpeechDetected = new List<AudioClip>();
    public List<AudioClip> modelGenThinking = new List<AudioClip>();
    public AudioSource audioSource;
    public Animator animator;
    private LLMMode _llmMode;
    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError("ThinkingVoiceLineTrigger needs an animator");
        }
        audioSource = GetComponent<AudioSource>();
        AIManagerMain.OnStopListening += StartVL;
        AIManagerMain.OnStartSpeaking += StopVL;
        AIManagerMain.OnNoSpeechDetected += NoSpeechVL;
        AIManagerMain.OnModeChanged += ChangeMode;
    }

    private void ChangeMode(LLMMode obj)
    {
        _llmMode = obj;
    }

    private void NoSpeechVL()
    {
        LadyMoodSwitch.Instance.ChangeColor("cyan");
        audioSource.Stop();
        var vl = noSpeechDetected[Random.Range(0, noSpeechDetected.Count)];
        audioSource.clip = vl;
        animator.SetTrigger("NoSpeech");
        audioSource.Play();
    }

    private string[] _thinkingTriggers =
    {
        "Think1",
        "Think2",
        "Think3",
        "Think4",
        "Think5",
        "Think6",
        "Think7",
        "Think8",
    };
    public void StartVL()
    {
        if (_llmMode == LLMMode.ModelMode)
        {
            var vl = modelGenThinking[Random.Range(0, modelGenThinking.Count)];
            audioSource.clip = vl;
            audioSource.Play();
        }
        LadyMoodSwitch.Instance.ChangeColor("cyan");
        animator.SetTrigger(_thinkingTriggers[Random.Range(0, _thinkingTriggers.Length)]);
       // var vl = thinkingVoiceLines[Random.Range(0, thinkingVoiceLines.Count)];
        
    }

    public void StopVL()
    {
        audioSource.Stop();
    }

    private void OnDestroy()
    {
        AIManagerMain.OnStopListening -= StartVL;
        AIManagerMain.OnStartSpeaking -= StopVL;
    }
}
