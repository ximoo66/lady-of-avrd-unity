//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script is used for onboarding with multiple steps, controls UI

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class OnboardingManager : MonoBehaviour
{
    

    public GameObject[] steps;

    private int currentIndex = 0;

    public float SpawnDistance = 0.5f;
    public float SpawnHeightOffset = 1f;
    
    [Header("Follow Behavior")]
    public float positionDampTime = 0.5f;  
    public float rotationDampSpeed = 2f;    
    public float moveThreshold = 0.5f;     
    public float angleThreshold = 30f; 
    private Transform cam;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private bool needsReposition = false;
    public event Action OnStartOnboardingFinished;
    public event Action OnOnboardingFinished;
    void Awake()
    
    {
        ShowStep(0);
        SpawnAtPosition();
    }



    public void SpawnAtPosition()
    {
        cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        targetPosition = cam.position + flatForward * SpawnDistance + Vector3.up * SpawnHeightOffset;
        transform.position = targetPosition;
        Vector3 directionToCam = transform.position - cam.position;
        directionToCam.y = 0f;
        transform.rotation = Quaternion.LookRotation(directionToCam);
    }

    public void SkipOnboarding()
    {
        CloseUI();
    }

    void Update()
    {
        if (cam == null)
        {
            SpawnAtPosition();
            return;
        }
        Vector3 flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 desiredPosition = cam.position + flatForward * SpawnDistance + Vector3.up * SpawnHeightOffset;

        float distanceFromTarget = Vector3.Distance(transform.position, desiredPosition);
        float angleFromTarget = Vector3.Angle(transform.forward, -flatForward);

        if (distanceFromTarget > moveThreshold || angleFromTarget > angleThreshold)
        {
            needsReposition = true;
        }

        if (needsReposition)
        {
            targetPosition = desiredPosition;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionDampTime);

            Vector3 directionToCam = transform.position - cam.position;
            directionToCam.y = 0f;

            if (directionToCam.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(directionToCam);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationDampSpeed);
            }

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                needsReposition = false;
            }
        }
    }

    public void ShowStep(int index)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].SetActive(i == index);
        }

        currentIndex = index;
        
    }

    public void GoNext()
    {
        if (currentIndex < steps.Length - 1)
            ShowStep(currentIndex + 1);
    }

    public void GoBack()
    {
        if (currentIndex > 0)
            ShowStep(currentIndex - 1);
    }

    public void CloseUI()
    {
        OnOnboardingFinished?.Invoke();
        this.gameObject.SetActive(false);
    }

    public void StartOnboardingFinished()
    {
        OnStartOnboardingFinished?.Invoke();
        CloseUI();
    }
}