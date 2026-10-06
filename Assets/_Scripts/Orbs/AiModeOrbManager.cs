// Authors: Omid Ameri, Ekaterina Siling, Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-16 by Omid Ameri
// Last Modified: 2026-06-21 by Noah Wendt

// Course: P6

using UnityEngine;
using UnityEngine.Events;

public class AiModeOrbManager : MonoBehaviour
{
    [Header("AI Manager")]
    [SerializeField] private AIManagerMain aiManagerMain;

    [Header("Mode Data")]
    [SerializeField] private OrbData defaultMode;
    [SerializeField] private OrbData[] availableModes;

    [Header("Orb References")]
    [SerializeField] private OrbController activeOrb;
    [SerializeField] private OrbController[] selectorOrbs;

    [Header("Events")]
    [SerializeField] private UnityEvent<LLMMode> onModeChanged;

    private OrbData currentMode;

    public OrbData CurrentMode => currentMode;

    private void Start()
    {
        SetupSelectorOrbs();

        if (defaultMode != null)
        {
            SetActiveMode(defaultMode);
        }

        if (aiManagerMain == null)
        {
            aiManagerMain = FindObjectOfType<AIManagerMain>();
        }
    }

    public void SetActiveMode(OrbData modeData)
    {
        if (modeData == null)
        {
            return;
        }

        currentMode = modeData;

        if (activeOrb != null)
        {
            activeOrb.SetData(currentMode);
            activeOrb.SetState(OrbInteractionState.Selected);
        }

        if (aiManagerMain != null)
        {
            aiManagerMain.SwitchMode(currentMode.Mode);
        }

        onModeChanged?.Invoke(currentMode.Mode);
    }

    private void SetupSelectorOrbs()
    {
        if (availableModes == null || selectorOrbs == null)
        {
            return;
        }

        int count = Mathf.Min(availableModes.Length, selectorOrbs.Length);

        for (int i = 0; i < count; i++)
        {
           // selectorOrbs[i].SetData(availableModes[i]);
        }
    }
}