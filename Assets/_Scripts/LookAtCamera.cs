// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-17 by Noah Wendt
// Last Modified: 2026-06-17 by Noah Wendt

using Sirenix.OdinInspector;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public bool flip = true;
    public bool yAxisOnly = false;
    
    void Update()
    {
        
        transform.LookAt(Camera.main.transform);
        
        if (yAxisOnly)
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            transform.rotation = Quaternion.Euler(euler);
        }
        
        if (flip)
        {
            transform.Rotate(0, 180, 0);
        }
    }
}