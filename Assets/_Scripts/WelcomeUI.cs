// Authors: Linus Ziesel, Ekaterina Siling
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-07-01 by Ekaterina Siling

using UnityEngine;
using System;
using JetBrains.Annotations;
using UnityEngine.UI;

/// <summary>
/// Handles the initial welcome interface, allowing the user to begin a conversation with the AI.
/// </summary>
public class WelcomeUI : MonoBehaviour
{

    public Button startConvBtn;
    void Start()
    {
        AIManagerMain aiManagerMain = GameObject.FindFirstObjectByType<AIManagerMain>();
        startConvBtn.onClick.AddListener(() =>
        {
            aiManagerMain.StartingConversation();
        });
    }

}
