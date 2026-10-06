// Authors: Ekaterina Siling
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-31 by Ekaterina Siling
// Last Modified: 2026-06-21 by Ekaterina Siling

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script is not used at the moment, MindMapConnection is used instead. reason: line rendering doesn;t work properly in MR

[RequireComponent(typeof(LineRenderer))]
public class MindMapLineConnection : MonoBehaviour
{
    public MindMapNode nodeA;
    public MindMapNode nodeB;
    
    [Header("Colors")]
    public Color _normalColor = Color.blue;
    public Color _selectedColor = Color.red;
    public Transform cylinder;
    public float thickness = 0.5f;
    
    
 
    private MindMapManager _mindMapManager;

    private HoldAction _holdAction;


    public void StartHold() => _holdAction.StartHold();
    public void StopHold()  => _holdAction.StopHold();
    
    [Header("Input")]
    public InputActionReference deleteAction;

    private bool _isTouched = false;



    private void OnEnable()
    {

        _mindMapManager = FindObjectOfType<MindMapManager>();
        if (deleteAction == null) return;
        deleteAction.action.Enable();
        deleteAction.action.performed += OnDeletePressed;
        deleteAction.action.canceled  += OnDeleteReleased;
    }

    private void OnDisable()
    {
        if (deleteAction == null) return;
        deleteAction.action.performed -= OnDeletePressed;
        deleteAction.action.canceled  -= OnDeleteReleased;
    }

    private void OnDeletePressed(InputAction.CallbackContext ctx)
    {
        if (_isTouched) _holdAction.StartHold();
    }

    private void OnDeleteReleased(InputAction.CallbackContext ctx)
    {
        _holdAction.StopHold();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            _isTouched = true;
            Select();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            _isTouched = false;
            Deselect();
        }
    }
    
    private void Awake()
    {
        _holdAction = GetComponent<HoldAction>();
        _holdAction.OnHoldCompleted += ExecuteDelete;

        _mindMapManager = FindObjectOfType<MindMapManager>();
    }

    public void Connect(MindMapNode a, MindMapNode b)
    {
        nodeA = a;
        nodeB = b;
        UpdateLine();
    }

    private void LateUpdate()
    {
        if (nodeA != null && nodeB != null)
            UpdateLine();
    }
    private void ExecuteDelete()
    {

        _mindMapManager.DisconnectBubbles(nodeA.label, nodeB.label);
    }


    private void UpdateLine()
    {
        var ptA = nodeA.GetBestConnectorToward(nodeB.Position);
        var ptB = nodeB.GetBestConnectorToward(nodeA.Position);

        //_lr.SetPosition(0, ptA.position);
        //_lr.SetPosition(1, ptB.position);
        Vector3 from = ptA.position;
        Vector3 to   = ptB.position;
        Vector3 dir  = to - from;

        if (dir.sqrMagnitude < 0.0001f) return;

        // Place at midpoint, rotate Y-axis toward target
        // (Unity cylinders are Y-up by default)
        transform.position = (from + to) * 0.5f;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);

        // Scale: XZ = thickness, Y = half distance (cylinder is 2 units tall)
        float dist = dir.magnitude;
        if (cylinder != null)
            cylinder.localScale = new Vector3(thickness, dist * 0.5f, thickness);
        else
            transform.localScale = new Vector3(thickness, dist * 0.5f, thickness);
    }

    public void Select()
    {
        Debug.Log("Selected line");
        GetComponent<Renderer>().material.color = _selectedColor;
        cylinder.gameObject.GetComponent<Renderer>().material.color = _selectedColor;

    }

    public void Deselect()
    {
        Debug.Log("Deselected line");

        cylinder.gameObject.GetComponent<Renderer>().material.color = _normalColor;

    }


}