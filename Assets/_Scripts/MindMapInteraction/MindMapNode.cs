//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script is used for changing the color of the node. it sits on the UI element

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MindMapNode : MonoBehaviour
{
    
    [Header("Runtime Data")]
    public string label;
    public MindMapNode parentNode;

    [SerializeField] private TextMeshProUGUI  _tmp;

    [Header("Input")]
    public InputActionReference deleteAction;

    private OrbController _orbController;
    


    private float _holdTimer = 0f;
    public bool _isHolding = false;
    
    private SphereCollider _collider;
    private Transform _surfacePoint; // a dummy transform we reuse

    private HoldAction _holdAction;

    private Transform cam;
    public bool awaitingTranscribedLabel = false;

    private MindMapManager _mindMapManager;
    private MindMapNodeAnimator _animator;

    public bool IsHolding
    {
        get => _holdAction.IsHolding;
        set { if (value) _holdAction.StartHold(); else _holdAction.StopHold(); }
    }
    private void Awake()
    {
        _holdAction = GetComponent<HoldAction>();
        _holdAction.OnHoldCompleted += ExecuteDelete;
        _animator = GetComponent<MindMapNodeAnimator>();

        ResolveReferences();
        
    }
    void Start()
    {
        if (Camera.main != null)
            cam = Camera.main.transform;
    }
    private void OnEnable()
    {
        if (deleteAction != null)
        {
            deleteAction.action.Enable();
            deleteAction.action.performed += OnDeletePressed;
            deleteAction.action.canceled  += OnDeleteReleased;
        }
    }

    private void OnDisable()
    {
        if (deleteAction != null)
        {
            deleteAction.action.performed -= OnDeletePressed;
            deleteAction.action.canceled  -= OnDeleteReleased;
        }
    }

    private void OnDeletePressed(InputAction.CallbackContext ctx)
    {
        // Only respond if a controller is physically touching this node
        if (_isTouched) _holdAction.StartHold();
    }

    private void OnDeleteReleased(InputAction.CallbackContext ctx)
    {
        _holdAction.StopHold();
    }
    private bool _isTouched = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            _isTouched = true;
            _animator?.SetHover(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            _isTouched = false;
            _animator?.SetHover(false);
        }
    }
    public void Initialise(string nodeLabel, MindMapNode parent = null)
    {
        label      = nodeLabel;
        parentNode = parent;
        ResolveReferences();
        if (_tmp != null) _tmp.text = nodeLabel;
    }

    public void SetNodeLabel(string newLabel)
    {
        label = newLabel;        // ← sync the field
        _tmp.text = newLabel;
        awaitingTranscribedLabel = false;
    }
    private void ResolveReferences()
    {
        if (_tmp == null)
            _tmp = GetComponentInChildren<TextMeshProUGUI>();

       
        _collider = GetComponent<SphereCollider>();
        _mindMapManager = FindObjectOfType<MindMapManager>();
        var go = new GameObject("_SurfacePoint");
        go.transform.SetParent(transform);
        _surfacePoint = go.transform;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("GameController") && _orbController != null)
        {
            _orbController.Focus();
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("GameController") && _orbController != null)
        {
            _orbController.Unfocus();
        }
    }
    


    private void ExecuteDelete()
    {
        _isHolding = false;
        _holdTimer = 0f;

        _mindMapManager.DeleteBubble(this.label);
    }
    /// <summary>
    /// Returns whichever of the 6 connector points is closest to worldTarget.
    /// </summary>
    public Transform GetBestConnectorToward(Vector3 worldTarget)
    {

        if (_collider == null) return transform;

        Vector3 dir = (worldTarget - transform.position).normalized;
        // Return a point on the surface in that direction
        _surfacePoint.position = transform.position + dir * _collider.radius * transform.lossyScale.x;
        return _surfacePoint;
    }

    public Vector3 Position => transform.position;

}