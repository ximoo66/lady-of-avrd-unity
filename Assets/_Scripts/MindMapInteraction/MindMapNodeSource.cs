//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this is a script for an object that can spawn nodes. it's located in the menu. players use Grip button and drag to create new node

using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

public class MindMapNodeSource : MonoBehaviour
{
    [Header("References")]
    public MindMapManager mindMapManager;
    public InputActionReference grabAction;
    public float spawnOffset = 0.0f;
    public string defaultLabel = "New Node";

    private readonly HashSet<Collider> _controllersInside = new();
    private bool _wasPressedLastFrame = false;
    private int _spawnCount = 0;
    private bool _isControllerInside = false;
    private GameObject _controller;
    private GameObject _node = null;
    private void OnEnable()
    {
        if (grabAction != null)
        {
            grabAction.action.Enable();
            grabAction.action.performed += OnGrabPressed;
            grabAction.action.canceled += OnGrabReleased;
        }
        mindMapManager = FindObjectOfType<MindMapManager>();
        mindMapManager._activeSource = this;
            
            
    }

    private void OnGrabReleased(InputAction.CallbackContext obj)
    {
        if (_node != null)
        {
            _node.gameObject.transform.SetParent(null);
        }
    }

    private void OnGrabPressed(InputAction.CallbackContext obj)
    {
        Debug.Log("GRAB PRESSED");
        if (_isControllerInside)
        {
            SpawnAndTryGrab(_controller);
        }
    }

    private void OnDisable()
    {
        if (grabAction != null)
        {
            grabAction.action.Disable();
        }
        
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            _isControllerInside = true;
            _controller = other.gameObject;
        }
            

        Debug.Log($"Controller entered source: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            _isControllerInside = false;
            _controller = null;
        }
            
        
        Debug.Log($"Controller exited source: {other.name}");
    }
    

    private void SpawnAndTryGrab(GameObject controller)
    {
        _spawnCount++;
        string label = $"{defaultLabel} {_spawnCount}";

        MindMapNode node = mindMapManager.CreateBubble(label, null, controller.transform.position);
        if (node == null)
            return;

        Transform controllerTransform = controller.transform;
        node.transform.position = controllerTransform.position + controllerTransform.forward * spawnOffset;
        _node = node.gameObject;
        _node.gameObject.transform.SetParent(controller.transform);
        Debug.Log($"Spawned node {label}");


    }
}