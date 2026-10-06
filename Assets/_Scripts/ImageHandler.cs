// Authors: Linus Ziesel, Noah Wendt, Ekaterina Siling
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-09 by Linus Ziesel
// Last Modified: 2026-07-20 by Linus Ziesel

﻿using System;
using TMPro;
using UnityEngine;

public class ImageHandler : MonoBehaviour
{
    [Tooltip("A prefab which we instantiate when an image is received. It holds an image component")]
    public GameObject imgNodePrefab;

    public float distanceToUserWhenSpawned = .7f;
    public MindMapManager mindMapManager;

    private void Start()
    {
        // AIManagerMain.OnImgReceived += ShowImage;  
        mindMapManager = FindFirstObjectByType<MindMapManager>();
        if (!mindMapManager)
        {
            Debug.LogError("MindMapManager not found in scene.");
        }
    }
    
    
    public void SpawnImgNodeLoading(string prompt)
    {
        if (imgNodePrefab == null)
        {
            Debug.LogError("ModelNodePrefab is not assigned in the ModelHandler inspector!");
            return;
        }

        // 1. Instantiate the container node and place it in front of the camera.
        Vector3 nodePosition = Camera.main.transform.position + Camera.main.transform.forward * distanceToUserWhenSpawned;
        GameObject imgContainer = Instantiate(imgNodePrefab, nodePosition, Quaternion.identity);
        
        TMP_Text promptDisplay = FindChildWithTag(imgContainer.transform, "PromptText").GetComponent<TMP_Text>();
        promptDisplay.text = prompt;
             

        Action<string> onImgReceived = null;
        onImgReceived = contentB64 =>
        {
            ShowImage(contentB64, imgContainer);
            AIManagerMain.OnImgReceived -= onImgReceived; // unsubscribe after use
        };
        AIManagerMain.OnImgReceived += onImgReceived; 
    }


    public void ShowImage(string contentB64, GameObject imgLoadingNode)
    {
        Transform storeT = imgLoadingNode.transform;
        Destroy(imgLoadingNode);

        MindMapManager mindMapManager = FindFirstObjectByType<MindMapManager>();
        mindMapManager.ShowImage(contentB64, "image", storeT);
    }

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
            Transform result = FindChildWithTag(child, tag);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
