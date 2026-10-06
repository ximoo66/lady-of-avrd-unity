// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-07 by Noah Wendt
// Last Modified: 2026-06-07 by Noah Wendt

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class MultiMultiPropertyBlockController : MonoBehaviour
{
    public float propertyValue = 1f;
    public string property = "_Fade";

    public Transform distancePoint;
    public List<MeshRenderer> renderers;
        
    private List<MaterialPropertyBlock> _propertyBlocks;
    
    private void OnEnable()
    {
        Init();
    }

    [Button]
    private void Init()
    {
        _propertyBlocks = new List<MaterialPropertyBlock>();
        
        foreach (var renderer in renderers)
        {
            var propertyBlock = new MaterialPropertyBlock();
            renderer.SetPropertyBlock(propertyBlock);
            _propertyBlocks.Add(propertyBlock);
        }
    }
    
    [Button]
    private void CollectAllChildren()
    {
        renderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        Init();
    }

    private void Update()
    {
        for (var i = 0; i < renderers.Count; i++)
        {
            var block = _propertyBlocks[i];
            var distance = Vector3.Distance(renderers[i].transform.position, distancePoint.position);
            block.SetFloat(property, Mathf.Clamp01(distance / propertyValue));
            renderers[i].SetPropertyBlock(block);
        }
    }
}
