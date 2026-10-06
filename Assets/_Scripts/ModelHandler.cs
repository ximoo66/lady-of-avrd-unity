// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-07 by Linus Ziesel
// Last Modified: 2026-07-20 by Linus Ziesel

﻿using System;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Materials;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ModelHandler : MonoBehaviour
{
    [Tooltip("A prefab which must contain a child GameObject with the '3DGenSpawnPoint' tag.")]
    public GameObject modelNodePrefab;

    [Tooltip("The distance from the user's camera where the node itself will be spawned.")]
    public float distanceToUserWhenSpawned = 1.0f;

    public Material basicMatForGlb;

    private const string SpawnPointTag = "3DGenSpawnPoint";

    public MindMapManager mindMapManager;
    private void Start()
    {
        mindMapManager = FindFirstObjectByType<MindMapManager>();
        if (!mindMapManager)
        {
            Debug.LogError("MindMapManager not found in scene.");
        }
    }

    public void SpawnModelNodeLoading(string prompt)
    {
        if (modelNodePrefab == null)
        {
            Debug.LogError("ModelNodePrefab is not assigned in the ModelHandler inspector!");
            return;
        }

        // 1. Instantiate the container node and place it in front of the camera.
        Vector3 nodePosition = Camera.main.transform.position + Camera.main.transform.forward * distanceToUserWhenSpawned;
        GameObject modelContainer = Instantiate(modelNodePrefab, nodePosition, Quaternion.identity);
        
        TMP_Text promptDisplay = FindChildWithTag(modelContainer.transform, "PromptText").GetComponent<TMP_Text>();
        promptDisplay.text = prompt;
             

        Action<string> onModelReceived = null;
        onModelReceived = contentB64 =>
        {
            SpawnModel(contentB64, modelContainer);
            AIManagerMain.OnModelReceived -= onModelReceived; // unsubscribe after use
        };
        AIManagerMain.OnModelReceived += onModelReceived; 
    }

        
    private async void SpawnModel(string contentB64, GameObject modelNode)
    {
        // Two alternatives to play the loading animation
        modelNode.GetComponentInChildren<ImgSeqPlayer>().enabled = false;
        modelNode.GetComponentInChildren<ImageSpinner>().enabled = false;

        Transform spawnPoint = FindChildWithTag(modelNode.transform, SpawnPointTag);

        if (spawnPoint == null)
        {
            Debug.LogError(
                $"The prefab '{modelNodePrefab.name}' does not contain a child with the tag '{SpawnPointTag}'. The model could not be spawned. Cleaning up container.",
                modelNode);
            return;
        }

        Debug.Log("Spawning model into the designated spawn point within the node...");

        // 3. Spawn the actual GLB model as a child of the found spawn point.
        GameObject spawnedModel = await SpawnGlbFromB64(contentB64, spawnPoint);

        var meshRenderers = spawnedModel.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length > 0)
        {
            // Apply materials and calculate initial bounds
            var combinedBounds = meshRenderers[0].bounds;
            meshRenderers[0].material = basicMatForGlb;
            for (var i = 1; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = basicMatForGlb;
                combinedBounds.Encapsulate(meshRenderers[i].bounds);
            }

            // --- SCALING ---
            float maxDimension = Mathf.Max(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
            if (maxDimension > 0)
            {
                float scaleFactor = 1f / maxDimension;
                spawnedModel.transform.localScale *= scaleFactor;
            }

            // --- POSITIONING ---
            // Recalculate bounds after scaling to get the new center
            var scaledBounds = meshRenderers[0].bounds;
            for (var i = 1; i < meshRenderers.Length; i++)
            {
                scaledBounds.Encapsulate(meshRenderers[i].bounds);
            }

            // 1. Center the model on the spawn point
            var centerOffset = scaledBounds.center - spawnPoint.position;
            spawnedModel.transform.position -= centerOffset;

            // 2. Move the model "behind" the spawn point (the handle) from the user's perspective.
            // Since the model is scaled to roughly 1x1x1, moving it back by half its max size (0.5 units)
            // should be sufficient to place its front face at or behind the handle.
            spawnedModel.transform.position += Camera.main.transform.forward * 0.5f;
        }
        else
        {
            var rootRenderer = spawnedModel.GetComponent<MeshRenderer>();
            if (rootRenderer != null)
            {
                rootRenderer.material = basicMatForGlb;
            }
        }

        mindMapManager.ShowModel(modelNode);
    }

    
    
    public static async Task<GameObject> SpawnGlbFromB64(string contentB64, Transform parent)
    {
        byte[] modelBytes = Convert.FromBase64String(contentB64);

        var gltf = new GltfImport(null, null);
        
        bool success = await gltf.LoadGltfBinary(modelBytes);

        if (success)
        {
            success = await gltf.InstantiateMainSceneAsync(parent);

            if (success)
            {
                Debug.Log("Successfully loaded and instantiated GLB model with URP materials from the current pipeline asset.");
                return parent.GetChild(parent.childCount - 1).gameObject;
            }
            else
            {
                Debug.LogError("Failed to instantiate GLB scene.");
                return null;
            }
        }
        else
        {
            Debug.LogError("Failed to load GLB data.");
            return null;
        }
    }
    
    
    
    /// <summary>
    /// Recursively searches for a child with a specific tag.
    /// </summary>
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