// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-24 by Noah Wendt
// Last Modified: 2026-05-24 by Noah Wendt

using System;
using System.Collections.Generic;
using UnityEngine;

public class ConnectNodesViaSplineSetup : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public List<Transform> splineNodes;

    public float lineSize = .1f;

    private void OnValidate()
    {
        lineRenderer = GetComponent<LineRenderer>();
        for (int i = 0; i < splineNodes.Count; i++)
        {
            lineRenderer.SetPosition(i, splineNodes[i].position);
        }
        lineRenderer.startWidth = lineSize;
        lineRenderer.endWidth = lineSize;
    }
}
