// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-24 by Noah Wendt
// Last Modified: 2026-05-24 by Noah Wendt

using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CreateConnectorNodes : MonoBehaviour
{
    public Vector3 scale;
    
    public List<GameObject> children  = new();
    private void OnValidate()
    {
        scale = transform.localScale;
        if(transform.childCount < 6)
            CreateChildren();
    }

    [ContextMenu("Create Children")]
    public void CreateChildren()
    {
        if (transform.childCount >= 6)
            return;
        
        var positionOffsets = new Vector3[]
        {
            new (scale.x/2,0f, 0f),
            new (-scale.x/2,0f, 0f),
            new (0f,scale.y/2, 0f),
            new (0f, -scale.y/2,0f),
            new (0f, 0f, scale.z/2),
            new (0f, 0f,-scale.z/2)
        };

        foreach (var offset in positionOffsets)
        {
            var pos = transform.position + offset;
            var go = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere),pos, Quaternion.identity, transform);
            go.transform.localScale = scale/5;
            
            go.name = $"{gameObject.name}: {NameAddition(offset)}";
            children.Add(go);
        }
        DisableChildRenderers();
    }
    
    [ContextMenu("Disable Child Renderers")]
    public void DisableChildRenderers()
    {
        foreach (var child in children)
        {
            child.GetComponent<Renderer>().enabled = false;
        }
    }
    [ContextMenu("Enable Child Renderers")]
    public void EnableChildRenderers()
    {
        foreach (var child in children)
        {
            child.GetComponent<Renderer>().enabled = true; 
        }
    }

    [ContextMenu("Destroy Children")]
    public void DestroyChildren()
    {
        foreach (var child in children)
        {
            DestroyImmediate(child); 
        }
        children.Clear();
    }

    [ContextMenu("Update Names")]
    public void UpdateNames()
    {
        foreach (var child in children)
        {
           // child.name = $"{gameObject.name}: {NameAddition(offset)}";

        }
    }
    
    public string NameAddition(Vector3 offset)
    {
        if (offset.x != 0)
        {
            if (offset.x > 0)
                return "x positive";
            else
                return "x negative";
        }

        if (offset.y != 0)
        {
            if (offset.y > 0)
                return "y positive";
            else
                return "y negative";
        }

        if (offset.z != 0)
        {
            if (offset.z > 0)
                return "z positive";
            else
                return "z negative";
        }

        return "??";
    }
}
