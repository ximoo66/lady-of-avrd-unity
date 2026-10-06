// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-07 by Noah Wendt
// Last Modified: 2026-06-07 by Noah Wendt

using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class MultiPropertyBlockControllerFreeRangeSkinned : MonoBehaviour
{
    public bool disableAutoFind;
    //[FormerlySerializedAs("dissolveValue")] [Range(0f, 1f)]
    public float propertyValue = 1f;
    public string property = "_Fade";

    [Tooltip("List of MeshRenderers to apply the dissolve effect to. If empty, will try to find all MeshRenderers in this GameObject and its children.")]
    public List<Renderer> meshRenderers = new List<Renderer>();

    private MaterialPropertyBlock propertyBlock;
    private bool initialized = false;

    private void OnEnable()
    {
        Init();
        UpdateProperty();
    }

    private void OnDestroy()
    {
        //destroy property block
        if (propertyBlock != null)
        {
            
            propertyBlock.Clear();
            propertyBlock = null;
            
            foreach (var renderer in meshRenderers)
            {
                if (renderer == null) continue;
                renderer.SetPropertyBlock(null);
            }
        }
    }

    public void SetValue(float val)
    {
        propertyValue = val;
    }
    
    [Button]
    private void CollectAllChildren()
    {
        meshRenderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
        Init();
    }

    private void Init()
    {
       // if (initialized) return;

        propertyBlock = new MaterialPropertyBlock();

        // If no meshRenderers are assigned, try to find all in this GameObject and children
        if (meshRenderers.Count == 0)
        {
            if(!disableAutoFind)
                meshRenderers.AddRange(GetComponentsInChildren<Renderer>());
        }

        // Remove any null entries that might exist
        meshRenderers.RemoveAll(item => item == null);
        
        initialized = true;
    }

    private Tween tween;
    
    public Tween FadeInTo(float duration, float to)
    {
        tween?.Kill();
        tween = DOTween.To(() => propertyValue, x => propertyValue = x, to, duration).OnUpdate(UpdateProperty);
        return tween;
    }
    
    public Tween FadeIn(float duration)
    {
        tween?.Kill();
        tween = DOTween.To(() => propertyValue, x => propertyValue = x, 1f, duration).OnUpdate(UpdateProperty);
        return tween;
    }
    
    public Tween FadeOut(float duration)
    {
        tween?.Kill();
        tween= DOTween.To(() => propertyValue, x => propertyValue = x, 0f, duration).OnUpdate(UpdateProperty);
        return tween;
    }

    private void Update()
    {
        UpdateProperty();
    }

    public void UpdateProperty()
    {
        if (!initialized) Init();
        if(!Application.isPlaying) Init();

        foreach (var renderer in meshRenderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(property, propertyValue);
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
