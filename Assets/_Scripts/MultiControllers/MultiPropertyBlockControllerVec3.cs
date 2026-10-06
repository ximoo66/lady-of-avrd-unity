// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-07 by Noah Wendt
// Last Modified: 2026-06-07 by Noah Wendt

using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[ExecuteAlways]
public class MultiPropertyBlockControllerVec3 : MonoBehaviour
{
    public bool disableAutoFind;
    public bool useWorldSpacePosOfObject;
    public Vector3 propertyValue = Vector3.zero;
    public string property = "_Fade";

    [Tooltip("List of MeshRenderers to apply the dissolve effect to. If empty, will try to find all MeshRenderers in this GameObject and its children.")]
    public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    public List<SkinnedMeshRenderer> SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

    private MaterialPropertyBlock propertyBlock;
    private bool initialized = false;

    public Transform parent;
    
    [Button]
    public void CollectChildren()
    {
        meshRenderers = new List<MeshRenderer>(parent.GetComponentsInChildren<MeshRenderer>());
        SkinnedMeshRenderers = new List<SkinnedMeshRenderer>(parent.GetComponentsInChildren<SkinnedMeshRenderer>());
    }

    private void OnEnable()
    {
        Init();
    }

    public void SetValue(Vector3 val)
    {
        propertyValue = val;
    }

    public void Init()
    {
       // if (initialized) return;

        propertyBlock = new MaterialPropertyBlock();

        // If no meshRenderers are assigned, try to find all in this GameObject and children
        if (meshRenderers.Count == 0)
        {
            if(!disableAutoFind)
            meshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
        }

        // Remove any null entries that might exist
        meshRenderers.RemoveAll(item => item == null);
        
        if (SkinnedMeshRenderers.Count == 0)
        {
            if(!disableAutoFind)
            SkinnedMeshRenderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
        }
        
        SkinnedMeshRenderers.RemoveAll(item => item == null);
        
        initialized = true;
    }
    
    private void Update()
    {
        UpdateProperty();
    }

    public void UpdateProperty()
    {
        if (!initialized) Init();
        if(!Application.isPlaying) Init();

        if (useWorldSpacePosOfObject)
        {
            propertyValue = transform.position;
        }

        foreach (var renderer in meshRenderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(property, propertyValue);
            renderer.SetPropertyBlock(propertyBlock);
        }
        
        foreach (var renderer in SkinnedMeshRenderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(property, propertyValue);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Init();
        UpdateProperty();
    }

    [Button]
    public void Reinitialize()
    {
        initialized = false;
        Init();
        UpdateProperty();
    }
#endif
    
    
}