// Authors: Linus Ziesel, Ekaterina Siling
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-06-21 by Ekaterina Siling

﻿using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the display and interactions of the error user interface when an issue occurs.
/// </summary>
public class ErrorUI : MonoBehaviour
{
    public GameObject errorGO;
    public Button continueConvBtn;

    
    void Start()
    {
        //continueConvBtn = GameObject.FindGameObjectWithTag("ContinueConvBtn").GetComponent<Button>();
        //Debug.Assert(continueConvBtn, "continueConvBtn not found");
        
        errorGO = GameObject.FindGameObjectWithTag("ErrorUIGO");
        Debug.Assert(errorGO, "ErrorUIGO not found");
        errorGO.SetActive(false);
        
        AIManagerMain.OnError += () => 
        {
            errorGO.SetActive(true);
        };
        
        continueConvBtn.onClick.AddListener(() =>
        {
            errorGO.SetActive(false);
        });
    }

}