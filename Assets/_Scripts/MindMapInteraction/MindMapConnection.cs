//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script is used for connecting nodes together. it calculates correct location and rotation of the cyll,inder that is used as connection

using UnityEngine;
public class MindMapConnection : MonoBehaviour
{
    [Header("Prefab References")]
    public Transform cylinder;

    [Header("Runtime")]
    public MindMapNode nodeA;
    public MindMapNode nodeB;

    [Header("Visuals")]
    public float thickness = 0.05f;

    public void Connect(MindMapNode a, MindMapNode b)
    {
        nodeA = a;
        nodeB = b;
        UpdateGeometry();
    }

    private void LateUpdate()
    {
        if (nodeA != null && nodeB != null)
            UpdateGeometry();
    }

    private void UpdateGeometry()
    {
        Transform ptA = nodeA.GetBestConnectorToward(nodeB.Position);
        Transform ptB = nodeB.GetBestConnectorToward(nodeA.Position);

        Vector3 from = ptA.position;
        Vector3 to   = ptB.position;
        Vector3 dir  = to - from;

        if (dir.sqrMagnitude < 0.0001f) return;
        
        transform.position = (from + to) * 0.5f;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
        float dist = dir.magnitude;
        if (cylinder != null)
            cylinder.localScale = new Vector3(thickness, dist * 0.5f, thickness);
        else
            transform.localScale = new Vector3(thickness, dist * 0.5f, thickness);
    }
}