// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-05-07 by Linus Ziesel

﻿using UnityEngine;
using UnityEngine.Events;


public class ControllerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Which controller to listen to (LTouch or RTouch)")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Tooltip("The button to listen for. 'PrimaryHandTrigger' is the Grip button.")]
    public OVRInput.Button buttonToCheck = OVRInput.Button.PrimaryHandTrigger;

    [Header("Events")]
    public UnityEvent onButtonPressed;
    public UnityEvent onButtonReleased; // Optional: if you need release logic

    void Update()
    {
        // 1. OVRInput.GetDown returns true ONLY on the frame the button is pressed.
        if (OVRInput.GetDown(buttonToCheck, controller))
        {
            // Invoke the function set in the Inspector
            onButtonPressed.Invoke();
        }

        // 2. Optional: Detect when the button is released
        if (OVRInput.GetUp(buttonToCheck, controller))
        {
            onButtonReleased.Invoke();
        }
    }
}