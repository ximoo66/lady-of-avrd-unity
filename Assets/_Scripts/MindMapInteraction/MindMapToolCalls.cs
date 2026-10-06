//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: currently not in use, since LLM doesn't generate mindmaps

using UnityEngine;

public class MindMapToolCalls : MonoBehaviour
{
    private MindMapManager _manager;

    private void Awake()
    {
        _manager = FindFirstObjectByType<MindMapManager>();
        if (_manager == null)
            Debug.LogError("[MindMapToolCalls] No MindMapManager found in scene!", this);
    }
    [Tool("Summarise the current mind map nodes and connections.")]
    public string DescribeMindMap()
    {
        if (_manager == null) return "";

        var nodeLabels = string.Join(", ", _manager.GetNodeLabels());
        var edges      = string.Join(", ", _manager.GetConnectionsAsPairs());

        return $"[MindMap] Nodes: {nodeLabels} | Edges: {edges}";
        Debug.Log($"[MindMap] Nodes: {nodeLabels} | Edges: {edges}");
    }

    [Tool("Create a new mind map bubble. " +
          "Use parentLabel to attach it to an existing bubble. " +
          "Example: label='Horse', parentLabel='Animal'")]
    public void CreateBubble(string label, string parentLabel = null)
    {
        Debug.Log($"[MindMapToolCalls] CreateBubble called: label={label} parent={parentLabel}");
        _manager?.CreateBubble(label, parentLabel);
    }
    [Tool("Draw a connector line between two existing bubbles. " +
          "Example: labelA='Animal', labelB='Horse'")]
    public void ConnectBubbles(string labelA, string labelB)
        => _manager?.ConnectBubbles(labelA, labelB);

    [Tool("Remove a bubble and all its connections. " +
          "Example: label='Horse'")]
    public void DeleteBubble(string label)
        => _manager?.DeleteBubble(label);

    [Tool("Clear the entire mind map — removes all bubbles and connections.")]
    public void ClearMindMap()
        => _manager?.ClearAll();
}