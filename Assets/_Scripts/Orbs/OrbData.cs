// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-09 by Omid Ameri
// Last Modified: 2026-06-16 by Omid Ameri

// Course: P6

using UnityEngine;

[CreateAssetMenu(fileName = "Orb_Mode", menuName = "P6/Orbs/Mode Orb")]
public class OrbData : ScriptableObject
{
    [Header("Mode")]
    [SerializeField] private LLMMode mode;
    [SerializeField] private string orbName;

    [Header("Visuals")]
    [SerializeField] private Color orbColor = Color.white;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject indicatorPrefab;

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip hoverAudio;
    [SerializeField] private AudioClip selectAudio;
    [SerializeField] private AudioClip processingAudio;
    [SerializeField] private AudioClip resultAudio;
    [SerializeField] private AudioClip cancelAudio;

    public LLMMode Mode => mode;
    public string OrbName => orbName;
    public Color OrbColor => orbColor;
    public Sprite Icon => icon;
    public GameObject IndicatorPrefab => indicatorPrefab;

    public AudioClip HoverAudio => hoverAudio;
    public AudioClip SelectAudio => selectAudio;
    public AudioClip ProcessingAudio => processingAudio;
    public AudioClip ResultAudio => resultAudio;
    public AudioClip CancelAudio => cancelAudio;
}