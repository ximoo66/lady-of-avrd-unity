//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: currently not in use, was used for having multiple onboarding, which were triggered from another code

using UnityEngine;

public class OnboardingHandler : MonoBehaviour
{
    public GameObject startOnboarding;
    public GameObject imageModeOnboarding;
    public GameObject modelModeOnboarding;
    public GameObject mindmapModeOnboarding;

    private bool imageOnboarded = false;
    private bool modelOnboarded = false;
    private bool mindmapOnboarded = false;

    private GameObject currentOnboarding;
    private OnboardingManager currentManager;
    private LLMMode currentMode;

    void Start()
    {
        currentOnboarding = Instantiate(startOnboarding);
        //AIManagerMain.OnModeChanged += ShowOnboarding;
    }

    void ShowOnboarding(LLMMode mode)
    {
        CloseCurrentOnboarding(); 

        switch (mode)
        {
            case LLMMode.ModelMode:
                if (!modelOnboarded) SpawnOnboarding(modelModeOnboarding, LLMMode.ModelMode);
                break;
            case LLMMode.MindmapMode:
                if (!mindmapOnboarded) SpawnOnboarding(mindmapModeOnboarding, LLMMode.MindmapMode);
                break;
            case LLMMode.ImageMode:
                if (!imageOnboarded) SpawnOnboarding(imageModeOnboarding, LLMMode.ImageMode);
                break;
        }
    }

    private void SpawnOnboarding(GameObject prefab, LLMMode mode)
    {
        currentOnboarding = Instantiate(prefab);
        currentManager = currentOnboarding.GetComponent<OnboardingManager>();
        currentMode = mode;
        currentManager.OnOnboardingFinished += HandleFinished;
    }

    private void HandleFinished()
    {
        switch (currentMode)
        {
            case LLMMode.ModelMode: modelOnboarded = true; break;
            case LLMMode.MindmapMode: mindmapOnboarded = true; break;
            case LLMMode.ImageMode: imageOnboarded = true; break;
        }

        if (modelOnboarded && imageOnboarded && mindmapOnboarded)
            AIManagerMain.OnModeChanged -= ShowOnboarding;
    }

    private void CloseCurrentOnboarding()
    {
        if (currentOnboarding != null)
        {
            if (currentManager != null)
                currentManager.OnOnboardingFinished -= HandleFinished;

            Destroy(currentOnboarding);
            currentOnboarding = null;
            currentManager = null;
        }
    }
}