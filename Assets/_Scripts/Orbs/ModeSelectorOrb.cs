// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-16 by Omid Ameri
// Last Modified: 2026-06-16 by Omid Ameri

// Course: P6

using UnityEngine;

[RequireComponent(typeof(OrbController))]
public class ModeSelectorOrb : MonoBehaviour
{
    [SerializeField] private AiModeOrbManager modeOrbManager;

    private OrbController orbController;

    private void Awake()
    {
        orbController = GetComponent<OrbController>();
    }

    public void SelectMode()
    {
        if (modeOrbManager == null || orbController.OrbData == null)
        {
            return;
        }

        modeOrbManager.SetActiveMode(orbController.OrbData);
    }
}