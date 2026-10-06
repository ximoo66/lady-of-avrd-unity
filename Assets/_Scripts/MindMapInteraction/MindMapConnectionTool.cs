//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script is sits on the controller and is used for connecting and recording into nodes. 

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class MindMapConnectionTool : MonoBehaviour
{
    [Header("References")]
    public Transform controllerTip;

    [Header("Input")]
    public InputActionReference connectAction;
    public InputActionReference recordAction;

    [Header("Preview")]
    public Transform previewLinePrefab;
    public float previewLineThickness = 0.01f;

    private Transform _previewLineInstance;
    [Header("Runtime")]
    public MindMapNode currentTouchedNode;

    private MindMapNode _lastTouchedNode;

    private bool _isRecording = false;
    private MindMapNode _startNode;
    private bool _isConnecting = false;
    private MindMapManager _mindMapManager;
    private AIManagerMain _aiManagerMain;
    private HoldAction _holdAction;

    // ── lifecycle ─────────────────────────────────────────────

    private void Awake()
    {
        _mindMapManager = FindFirstObjectByType<MindMapManager>();
        _aiManagerMain  = FindObjectOfType<AIManagerMain>();
        _holdAction     = GetComponent<HoldAction>();

        _holdAction.OnHoldCompleted += StartRecording;

        if (connectAction != null)
        {
            connectAction.action.Enable();
            connectAction.action.performed += StartConnectNodes;
            connectAction.action.canceled  += EndConnectNodes;
        }

        if (recordAction != null)
        {
            recordAction.action.Enable();
            recordAction.action.performed += StartHolding;
            recordAction.action.canceled  += EndRecording;
        }
    }

    private void OnDestroy()
    {
        _holdAction.OnHoldCompleted -= StartRecording;

        if (connectAction != null)
        {
            connectAction.action.performed -= StartConnectNodes;
            connectAction.action.canceled  -= EndConnectNodes;
            connectAction.action.Disable();
        }

        if (recordAction != null)
        {
            recordAction.action.performed -= StartHolding;
            recordAction.action.canceled  -= EndRecording;
            recordAction.action.Disable();
        }

        // safety net: clean up any leftover preview instance
        if (_previewLineInstance != null)
            Destroy(_previewLineInstance.gameObject);
    }

    // ── update ────────────────────────────────────────────────

    private void Update()
    {
        if (_isConnecting)
            UpdatePreviewLine();
    }
    // ── record ────────────────────────────────────────────────
    

    private MindMapNode _recordingTargetNode; 

    private void StartHolding(InputAction.CallbackContext ctx)
    {
        if (_holdAction.IsHolding || _isRecording) return;
        if (currentTouchedNode == null) return;

        _recordingTargetNode = currentTouchedNode;
        _holdAction.StartHold();
        Debug.Log($"[Record] StartHolding — locked: {_recordingTargetNode.label}");
    }

    private void StartRecording()
    {
        if (_recordingTargetNode == null) return;
    
        _aiManagerMain.StartRec(true);
        _isRecording = true;
        _recordingTargetNode.GetComponent<MindMapNodeAnimator>()?.SetRecording(true);
        Debug.Log($"[ConnectionTool] Recording started for: {_recordingTargetNode.label}");
    }


    private void EndRecording(InputAction.CallbackContext ctx)
    {
        _holdAction.StopHold();
    
        if (!_isRecording)
        {
            _recordingTargetNode = null; 
            return;
        }
        _recordingTargetNode.GetComponent<MindMapNodeAnimator>()?.SetRecording(false);


        _isRecording = false;
        _aiManagerMain.StopAndSend(true);
        _recordingTargetNode.awaitingTranscribedLabel = true;
        Debug.Log($"[ConnectionTool] Recording stopped, label pending on: {_recordingTargetNode.label}");
        _recordingTargetNode = null;
    }
    // ── connect ───────────────────────────────────────────────

    private void StartConnectNodes(InputAction.CallbackContext ctx) => TryBeginConnection();
    private void EndConnectNodes(InputAction.CallbackContext ctx)   => TryFinishConnection();

    private void TryBeginConnection()
    {
        if (currentTouchedNode == null) return;

        _startNode    = currentTouchedNode;
        _isConnecting = true;

        if (previewLinePrefab != null)
        {
            _previewLineInstance = Instantiate(previewLinePrefab);
            UpdatePreviewLine();
        }

        Debug.Log($"[ConnectionTool] Connection started from {_startNode.label}");
    }

    private void TryFinishConnection()
    {
        if (!_isConnecting) return;

        if (_startNode != null && currentTouchedNode != null && currentTouchedNode != _startNode)
        {
            _mindMapManager.ConnectBubbles(_startNode.label, currentTouchedNode.label);
            Debug.Log($"[ConnectionTool] Connected {_startNode.label} → {currentTouchedNode.label}");
        }
        else
        {
            Debug.Log("[ConnectionTool] Connection cancelled.");
        }

        _isConnecting = false;
        _startNode    = null;

        if (_previewLineInstance != null)
        {
            Destroy(_previewLineInstance.gameObject);
            _previewLineInstance = null;
        }
    }

    private void UpdatePreviewLine()
    {
        if (!_isConnecting || _startNode == null || _previewLineInstance == null) return;

        Vector3 from = _startNode.transform.position;
        Vector3 to   = controllerTip != null ? controllerTip.position : transform.position;
        Vector3 dir  = to - from;

        if (dir.sqrMagnitude < 0.0001f) return;

        _previewLineInstance.position   = (from + to) * 0.5f;
        _previewLineInstance.rotation   = Quaternion.FromToRotation(Vector3.up, dir.normalized);
        float dist                      = dir.magnitude;
        _previewLineInstance.localScale = new Vector3(previewLineThickness, dist * 0.5f, previewLineThickness);
    }


    // ── triggers ──────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        var node = other.GetComponentInParent<MindMapNode>();
        if (node != null)
        {
            // DO NOT set _recordingTargetNode here
            currentTouchedNode = node;
            _lastTouchedNode   = node;
        }
        if (other.CompareTag("ImgToggle"))
            other.GetComponent<ProximityImgDisplay>().SetVisible(true);
    }

    private void OnTriggerExit(Collider other)
    {
        var node = other.GetComponentInParent<MindMapNode>();
        if (node != null && currentTouchedNode == node)
        {
            currentTouchedNode = null;
        }
            

        if (other.CompareTag("ImgToggle"))
            other.GetComponent<ProximityImgDisplay>().SetVisible(false);
    }
}