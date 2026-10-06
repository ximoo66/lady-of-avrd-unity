// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-24 by Noah Wendt
// Last Modified: 2026-05-24 by Noah Wendt

using UnityEngine;

public class ConnectorLookAtManipulation : MonoBehaviour
{
    
    public Transform rootNode, targetNode;


    [ContextMenu("Set Position to root node")]
    public void SetPositionToRootNode()
    {
        transform.position = rootNode.position;
    }

    [ContextMenu("Set Rotation to look at target node")]
    public void SetRotationToLookAtTargetNode()
    {
        transform.forward = transform.up;
        transform.LookAt(targetNode);
    }

    [ContextMenu("Set Z scale to distance between target and root")]
    public void SetZScaleToDistanceBetweenTargetAndRoot()
    {
        Debug.Log(rootNode.position);
        Debug.Log(targetNode.position);
        transform.localScale = new Vector3(1.0f,1.0f, Vector3.Distance(rootNode.position,targetNode.position)*50);
    }

    [ContextMenu("Do All")]
    public void DoAll()
    {
        SetPositionToRootNode();
        SetRotationToLookAtTargetNode();
        SetZScaleToDistanceBetweenTargetAndRoot();
    }
}
