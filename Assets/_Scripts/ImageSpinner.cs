// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-12 by Linus Ziesel
// Last Modified: 2026-07-12 by Linus Ziesel

﻿using UnityEngine;
using UnityEngine.UI;

public class ImageSpinner : MonoBehaviour
{
    [Tooltip("The speed at which the image will rotate on the Z-axis.")]
    public float speed = 100f;

    public Image img;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        img = GetComponent<Image>();
    }

    void Update()
    {
        rectTransform.Rotate(0f, 0f, speed * Time.deltaTime);
    }

    private void OnDisable()
    {
        img.enabled = false;

    }
}
