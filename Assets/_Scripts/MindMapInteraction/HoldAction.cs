// Authors: Ekaterina Siling
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-21 by Ekaterina Siling
// Last Modified: 2026-06-21 by Ekaterina Siling

using System;
using UnityEngine;
using UnityEngine.UI;
//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script is used for interactions which require holding a button for a certain duration, then it fires an event that holding is completed. also controlls progress bar


public class HoldAction : MonoBehaviour
{
    [Header("Hold Settings")]
    public float holdDuration = 2f;
    public Image progressBar;
    public GameObject displayObject; // root of the canvas

    public event Action OnHoldCompleted;

    private float _timer = 0f;
    private bool _isHolding = false;

    private void Awake()
    {
        if (progressBar != null) progressBar.fillAmount = 0f;
        if (displayObject != null) displayObject.SetActive(false);
    }

    public void StartHold()
    {
        _timer = 0f; 
        _isHolding = true;
        if (displayObject != null) displayObject.SetActive(true);
    }

    public void StopHold()
    {
        if (!_isHolding) return;
        _isHolding = false;
        _timer = 0f;              
        if (progressBar != null) progressBar.fillAmount = 0f;
        if (displayObject != null) displayObject.SetActive(false);
    }

    public bool IsHolding => _isHolding;

    private void Update()
    {
        if (!_isHolding) return; 

        _timer += Time.deltaTime;
        if (progressBar != null)
            progressBar.fillAmount = _timer / holdDuration;

        if (_timer >= holdDuration)
        {
            _isHolding = false;
            _timer = 0f;
            if (progressBar != null) progressBar.fillAmount = 0f;
            if (displayObject != null) displayObject.SetActive(false);
            OnHoldCompleted?.Invoke();
        }
    }
}