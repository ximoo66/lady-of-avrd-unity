// Authors: Ekaterina Siling, Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-31 by Ekaterina Siling
// Last Modified: 2026-07-20 by Linus Ziesel

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script is used for logging all the existing nodes, it manges creation and deletion of nodes. An object with this script should be in the scene for the system to work

public class MindMapManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject lineConnectorPrefab;
    public GameObject imageNodePrefab;
    public GameObject modelNodePrefab;

    [Header("Layout")]
    public float   orbitRadius        = 1.5f;
    public Vector3 rootSpawnPosition  = Vector3.zero;
    public float distanceToUserWhenSpawned = .7f;


    [Header("Visuals")]
    public float connectionThickness = 0.05f;

    private readonly Dictionary<string, MindMapNode> _nodes = new();
    private readonly List<MindMapLineConnection> _connections = new();
    
    public AIManagerMain aiManagerMain;

    [Header("Node Source")]
    //public GameObject nodeSource;
    public float sourceSpawnDistance = 0.5f;
    public float sourceSpawnHeightOffset = 0f;

    public MindMapNodeSource _activeSource = null;

   
    private void OnEnable()
    {
        AIManagerMain.OnTranscriptionFinished += SetNodeLabel;
        //AIManagerMain.OnModeChanged += ToggleNodeSource;
        _activeSource = FindAnyObjectByType<MindMapNodeSource>();

    }

    private void ToggleNodeSource(LLMMode obj)
    {
        if (obj == LLMMode.MindmapMode)
        {
            SpawnNodeSource();
        }
        else
        {
            RemoveNodeSource();
        }
    }

    private MindMapNode EnsureNode(string label, MindMapNode parentForNew = null)
    {
        if (_nodes.TryGetValue(label, out var existing))
            return existing;

        // If we know a parent, use its label; otherwise null (root)
        var parentLabel = parentForNew != null ? parentForNew.label : null;
        return CreateBubble(label, parentLabel);
    }
    public void SpawnNodeSource()
    {
        if (_activeSource != null) return; // already one active

        Transform cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 spawnPos = cam.position
                           + flatForward * sourceSpawnDistance
                           + Vector3.up  * sourceSpawnHeightOffset;

        //var go = Instantiate(nodeSource, spawnPos, Quaternion.identity);
        //_activeSource = go.GetComponent<MindMapNodeSource>();
        _activeSource.mindMapManager = this;
        Debug.Log("[MindMapManager] NodeSource spawned.");
    }

    public void RemoveNodeSource()
    {
        if (_activeSource == null) return;
        Destroy(_activeSource.gameObject);
        _activeSource = null;
        Debug.Log("[MindMapManager] NodeSource removed.");
    }
    public IEnumerable<MindMapLineConnection> GetConnections()
    {
        return _connections.Where(c => c != null);
    }
    
    public MindMapNode CreateBubble(string label, string parentLabel = null, Vector3 position = default)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            Debug.LogWarning("[MindMapManager] CreateBubble called with empty label.");
            return null;
        }

        if (_nodes.TryGetValue(label, out var existing))
            return existing;

        MindMapNode parent = null;
        if (!string.IsNullOrEmpty(parentLabel))
            _nodes.TryGetValue(parentLabel, out parent);
        Vector3 spawnPos;
        if (position == default)
            spawnPos = CalculateSpawnPosition(label, parent);
        else
        {
            spawnPos = position;
        }
        var go = Instantiate(nodePrefab, spawnPos, Quaternion.identity);
        go.name = $"Node_{label}";

        var node = go.GetComponent<MindMapNode>();
        if (node == null)
        {
            Debug.LogError("[MindMapManager] nodePrefab missing MindMapNode!");
            Destroy(go);
            return null;
        }

        node.Initialise(label, parent);
        _nodes[label] = node;
        NotifyGraphState();

        return node;
    }
     
    public void ShowModel(GameObject modelNodeGO, string label = "model")
    {
        if (_nodes.ContainsKey(label))
        {
            int suffix = 1;
            string candidate = $"{label}{suffix}";
            while (_nodes.ContainsKey(candidate))
            {
                suffix++;
                candidate = $"{label}{suffix}";
            }
            label = candidate;
        }
        
        modelNodeGO.name = $"Node_{label}";

        var node = modelNodeGO.GetComponent<MindMapNode>();
        if (node == null)
        {
            Debug.LogError("[MindMapManager] imageNodePrefab missing MindMapNode!");
            Destroy(modelNodeGO);
            return;
        }

        node.Initialise(label, null);
        _nodes[label] = node;
        NotifyGraphState();

        // modelNodeGO.transform.position = Camera.main.transform.position
        //                                  + Camera.main.transform.forward * distanceToUserWhenSpawned;


        Debug.Log($"[MindMapManager] Model node '{label}' displayed.");
    }
    
    public void ShowImage(string contentB64, string label = "image", Transform loadingNodeT = null)
    {
        byte[] imageBytes = Convert.FromBase64String(contentB64);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);

        if (_nodes.ContainsKey(label))
        {
            int suffix = 1;
            string candidate = $"{label}{suffix}";
            while (_nodes.ContainsKey(candidate))
            {
                suffix++;
                candidate = $"{label}{suffix}";
            }
            label = candidate;
        }


        GameObject imageNodeGO;
        if (loadingNodeT == null)
        {
            imageNodeGO = Instantiate(imageNodePrefab);
            imageNodeGO.transform.position = Camera.main.transform.position
                                             + Camera.main.transform.forward * distanceToUserWhenSpawned;
        }
        else
        {
            imageNodeGO = Instantiate(imageNodePrefab, loadingNodeT.position, loadingNodeT.rotation);
        }
        
        imageNodeGO.name = $"Node_{label}";

        var node = imageNodeGO.GetComponent<MindMapNode>();
        if (node == null)
        {
            Debug.LogError("[MindMapManager] imageNodePrefab missing MindMapNode!");
            Destroy(imageNodeGO);
            return;
        }

        node.Initialise(label, null);
        _nodes[label] = node;
        NotifyGraphState();

        var targetImage = imageNodeGO.GetComponent<ImageNode>().img;
        targetImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        Debug.Log($"[MindMapManager] Image node '{label}' displayed.");
    }
    public IEnumerable<string> GetNodeLabels()
    {
        return _nodes.Keys;
    }

    public IEnumerable<string> GetConnectionsAsPairs()
    {
        foreach (var c in _connections)
            yield return $"{c.nodeA.label}+{c.nodeB.label}";
    }
    private void NotifyGraphState()
    {
        AIManagerMain.CurrentMindMapState = SerializeGraphState();
        Debug.Log($"[MindMap State] {AIManagerMain.CurrentMindMapState}");
    }
    public string SerializeGraphState()
    {
        if (_nodes.Count == 0)
            return "MindMap: empty";

        var nodeLabels = string.Join(", ", _nodes.Keys);

        var edges = _connections
            .Where(c => c != null && c.nodeA != null && c.nodeB != null)
            .Select(c => $"{c.nodeA.label}->{c.nodeB.label}");
        var edgeStr = string.Join(", ", edges);

        return $"MindMap nodes: [{nodeLabels}] | Edges: [{edgeStr}]";
    }

    public void SetNodeLabel(string label)
    {
        label = Regex.Replace(label, @"[.,!?]+$", "");
    
        // Capitalize first letter
        if (label.Length > 0)
            label = char.ToUpper(label[0]) + label.Substring(1);
        //check if the same label already exists, if yes then add 1 to end
        if (_nodes.TryGetValue(label, out var existing))
            label = label + "1";
        MindMapNode changedNode = null;
        foreach (var node in _nodes)
        {
            if (node.Value.awaitingTranscribedLabel)
            {
                string oldKey = node.Key;
                node.Value.SetNodeLabel(label);
                _nodes.Remove(oldKey);
                _nodes[label] = node.Value;
                NotifyGraphState();
                break;
            }
        }

    }

    
    public MindMapLineConnection ConnectBubbles(string labelA, string labelB)
    {
        if (string.IsNullOrWhiteSpace(labelA) || string.IsNullOrWhiteSpace(labelB))
        {
            Debug.LogWarning("[MindMapManager] ConnectBubbles called with empty label.");
            return null;
        }

        // Ensure both nodes exist:
        // - if both missing: create A as root, then B as child of A
        // - if only one missing: create it as child of the existing one
        MindMapNode nodeA = EnsureNode(labelA);
        MindMapNode nodeB = EnsureNode(labelB, nodeA);

        // Avoid duplicate connections
        foreach (var existing in _connections)
        {
            if ((existing.nodeA == nodeA && existing.nodeB == nodeB) ||
                (existing.nodeA == nodeB && existing.nodeB == nodeA))
                return existing;
        }

        var go = Instantiate(lineConnectorPrefab);
        go.name = $"Connection_{labelA}_{labelB}";

        var conn = go.GetComponent<MindMapLineConnection>();
        if (conn == null)
        {
            Debug.LogError("[MindMapManager] lineConnectorPrefab missing MindMapLineConnection!");
            Destroy(go);
            return null;
        }

        conn.Connect(nodeA, nodeB);
        _connections.Add(conn);
        NotifyGraphState();
        return conn;
        
    }
    public bool DisconnectBubbles(string labelA, string labelB)
    {
        if (string.IsNullOrWhiteSpace(labelA) || string.IsNullOrWhiteSpace(labelB))
        {
            Debug.LogWarning("[MindMapManager] DisconnectBubbles called with empty label.");
            return false;
        }

        if (!_nodes.TryGetValue(labelA, out var nodeA) || !_nodes.TryGetValue(labelB, out var nodeB))
        {
            Debug.LogWarning($"[MindMapManager] DisconnectBubbles: one or both nodes not found ('{labelA}', '{labelB}').");
            return false;
        }

        for (int i = _connections.Count - 1; i >= 0; i--)
        {
            var existing = _connections[i];
            if (existing == null) continue;

            bool matches =
                (existing.nodeA == nodeA && existing.nodeB == nodeB) ||
                (existing.nodeA == nodeB && existing.nodeB == nodeA);

            if (matches)
            {
                _connections.RemoveAt(i);
                Destroy(existing.gameObject);
                Debug.Log($"<color=yellow>[MindMapManager] Disconnected: <b>{labelA}</b> ↔ <b>{labelB}</b></color>");
                NotifyGraphState();
                return true;
            }
        }

        Debug.LogWarning($"[MindMapManager] DisconnectBubbles: no connection found between '{labelA}' and '{labelB}'.");
        return false;
    }

    public void DeleteBubble(string label)
    {
        if (!_nodes.TryGetValue(label, out var node))
        { Debug.LogWarning($"[MindMapManager] DeleteBubble: '{label}' not found."); return; }

        for (int i = _connections.Count - 1; i >= 0; i--)
        {
            if (_connections[i].nodeA == node || _connections[i].nodeB == node)
            { Destroy(_connections[i].gameObject); _connections.RemoveAt(i); }
        }

        _nodes.Remove(label);
        Destroy(node.gameObject);
        Debug.Log($"<color=orange>[MindMapManager] Deleted: <b>{label}</b></color>");
        NotifyGraphState();

    }

    public void ClearAll()
    {
        foreach (var c in _connections) if (c) Destroy(c.gameObject);
        _connections.Clear();
        foreach (var kv in _nodes) if (kv.Value) Destroy(kv.Value.gameObject);
        _nodes.Clear();
        Debug.Log("<color=red>[MindMapManager] Cleared.</color>");
        NotifyGraphState();

    }

    // ------------------------------------------------------------------ layout

    private Vector3 CalculateSpawnPosition(string label, MindMapNode parent)
    {
        if (parent == null)
        {
            // Spread root nodes along X
            int rootCount = _nodes.Values.Count(n => n.parentNode == null);
            return rootSpawnPosition + new Vector3(rootCount * orbitRadius, 0f, 0f);
        }

        int siblings = _nodes.Values.Count(n => n.parentNode == parent);

        // Fibonacci-ish sphere distribution to avoid overlap
        float goldenAngle = 137.5f * Mathf.Deg2Rad;
        float angle       = siblings * goldenAngle;
        float tilt        = (siblings % 2 == 0) ? 0.3f : -0.3f; // alternate Y

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * orbitRadius,
            tilt * orbitRadius,
            Mathf.Sin(angle) * orbitRadius
        );

        return parent.Position + offset;
    }
}