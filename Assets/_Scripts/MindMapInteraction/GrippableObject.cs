//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: since the default grabbable uses both trigger and grip buttons, this script uses only grip button to move nodes
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrippableObject : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference leftGripAction;
    public InputActionReference rightGripAction;

    private Rigidbody _rb;

    private Transform _leftHand    = null;
    private Transform _rightHand   = null;
    private bool      _leftInRange  = false;
    private bool      _rightInRange = false;

    private Transform  _heldBy;
    private Vector3    _localOffset;
    private Quaternion _localRotation;

    private NodeScaler _nodeScaler;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _nodeScaler = GetComponent<NodeScaler>(); // null if not present

    } 

    void OnEnable()
    {
        Register(leftGripAction,  OnLeftGrip,  OnLeftRelease);
        Register(rightGripAction, OnRightGrip, OnRightRelease);
    }

    void OnDisable()
    {
        Unregister(leftGripAction,  OnLeftGrip,  OnLeftRelease);
        Unregister(rightGripAction, OnRightGrip, OnRightRelease);
    }

    private void Register(InputActionReference r,
        Action<InputAction.CallbackContext> performed,
        Action<InputAction.CallbackContext> canceled)
    {
        if (r == null) return;
        r.action.performed += performed;
        r.action.canceled  += canceled;
        r.action.Enable();
    }

    private void Unregister(InputActionReference r,
        Action<InputAction.CallbackContext> performed,
        Action<InputAction.CallbackContext> canceled)
    {
        if (r == null) return;
        r.action.performed -= performed;
        r.action.canceled  -= canceled;
    }

    // ── callbacks ─────────────────────────────────────────────

    private void OnLeftGrip(InputAction.CallbackContext ctx)     => TryGrab(_leftHand, _leftInRange, true);
    private void OnRightGrip(InputAction.CallbackContext ctx)    => TryGrab(_rightHand, _rightInRange, false);
    private void OnLeftRelease(InputAction.CallbackContext ctx)  => TryRelease(true);
    private void OnRightRelease(InputAction.CallbackContext ctx) => TryRelease(false);
    // ── logic ─────────────────────────────────────────────────


    private bool _heldByLeft = false;

    private void TryGrab(Transform hand, bool inRange, bool isLeft)
    {
        if (_nodeScaler != null && _nodeScaler.IsScaling) return;
        if (_heldBy != null || !inRange || hand == null) return;

        _heldBy        = hand;
        _heldByLeft    = isLeft;
        _localOffset   = _heldBy.InverseTransformPoint(transform.position);
        _localRotation = Quaternion.Inverse(_heldBy.rotation) * transform.rotation;
        _rb.isKinematic = true;
    }

    private void TryRelease(bool isLeft)
    {
        if (_heldBy == null || _heldByLeft != isLeft) return;
        _rb.isKinematic = false;
        _heldBy = null;
    }

    // ── triggers ──────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GameController")) return;

        var tag = other.GetComponent<ControllerTag>();
        if (tag == null)
        {
            Debug.LogWarning($"[Grippable] {other.name} is tagged GameController but missing ControllerHandTag!");
            return;
        }

        if (tag.isLeft) { _leftInRange  = true;  _leftHand  = other.transform; }
        else            { _rightInRange = true;  _rightHand = other.transform; }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("GameController")) return;
        var tag = other.GetComponent<ControllerTag>();
        if (tag == null) return;

        if (tag.isLeft)
        {
            _leftInRange = false;
            if (!(_heldBy == other.transform)) _leftHand = null;
        }
        else
        {
            _rightInRange = false;
            if (!(_heldBy == other.transform)) _rightHand = null;
        }
    }

    void Update()
    {
        if (_heldBy == null) return;
        transform.position = _heldBy.TransformPoint(_localOffset);
        transform.rotation = _heldBy.rotation * _localRotation;
    }
}