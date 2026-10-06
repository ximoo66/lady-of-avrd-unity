//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script allows to display UI on a proximity to a controller

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProximityImgDisplay : MonoBehaviour
{
    public List<GameObject>  imgToDisplay;

    public bool enabled = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        foreach (GameObject img in imgToDisplay)
            img.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVisible(bool visible)
    {
        foreach (GameObject img in imgToDisplay)
            img.SetActive(visible);
        
    }

}
