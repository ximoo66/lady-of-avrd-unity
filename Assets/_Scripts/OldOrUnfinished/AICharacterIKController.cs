// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-05-07 by Linus Ziesel

﻿using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Controls the Inverse Kinematics (IK) of the AI character to naturally direct its gaze or posture based on conversation state.
/// </summary>
public class AICharacterIKController : MonoBehaviour
{
    [SerializeField] private Rig ikRig;
    [SerializeField] private ChainIKConstraint rightHandIKConstraint;

    [Header("Targets")]
    [SerializeField] private Transform earTarget;
    [SerializeField] private Transform chinTarget;

    [Header("Settings")]
    [SerializeField] private float ikWeightSpeed = 4.0f;
    [SerializeField] private float ikPositionSpeed = 5.0f;

    private Transform currentTarget;
    private Transform ikEffector;
    private float targetWeight = 0f;

    private void Start()
    {
        if (ikRig == null) ikRig = GetComponentInChildren<Rig>();

        if (rightHandIKConstraint != null && rightHandIKConstraint.data.target != null)
        {
            rightHandIKConstraint.weight = 1.0f;
            ikEffector = rightHandIKConstraint.data.target;
        }
        else
        {
            Debug.LogError("ChainIKConstraint or its Target is missing. Disabling IK Controller.");
            enabled = false;
            return;
        }

        AIManagerMain.OnStartListening += OnStartListening;
        AIManagerMain.OnStopListening += OnStopListening;
        AIManagerMain.OnStartSpeaking += OnStartSpeaking;
    }

    private void OnDestroy()
    {
        AIManagerMain.OnStartListening -= OnStartListening;
        AIManagerMain.OnStopListening -= OnStopListening;
        AIManagerMain.OnStartSpeaking -= OnStartSpeaking;
    }

    private void Update()
    {
        // Smoothly move the IK effector towards the active target (Ear or Chin)
        if (currentTarget != null)
        {
            ikEffector.position = Vector3.Lerp(ikEffector.position, currentTarget.position, Time.deltaTime * ikPositionSpeed);
            ikEffector.rotation = Quaternion.Slerp(ikEffector.rotation, currentTarget.rotation, Time.deltaTime * ikPositionSpeed);
        }

        // Smoothly blend the Rig weight
        ikRig.weight = Mathf.Lerp(ikRig.weight, targetWeight, Time.deltaTime * ikWeightSpeed);
    }

    [ContextMenu("Test Start Listening")]
    private void OnStartListening()
    {
        currentTarget = earTarget;
        targetWeight = 1.0f;
    }

    [ContextMenu("Test Stop Listening")]
    private void OnStopListening()
    {
        currentTarget = chinTarget;
        targetWeight = 1.0f;
    }

    [ContextMenu("Test Start Speaking")]
    private void OnStartSpeaking()
    {
        // Keep currentTarget as is to avoid snapping while fading out
        targetWeight = 0.0f;
    }
}
