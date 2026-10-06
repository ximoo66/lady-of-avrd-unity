//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script allows player to scale a node by holding it with one hand and dragging with the other
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class NodeScaler : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference leftGripAction;
    public InputActionReference rightGripAction;

    [Header("Scale Limits")]
    public float minScale = 0.3f;
    public float maxScale = 3.0f;

    

    private Transform _handA = null; // whichever gripped the node first
    private Transform _handB = null; // the other one

    private bool _handAHeld = false;
    private bool _handBHeld = false;

    private Transform _leftControllerTransform;
    private Transform _rightControllerTransform;

    private bool  _isScaling       = false;
    private float _initialDistance = 0f;
    private float _initialScale    = 0f;

    private bool _leftInRange  = false;
    private bool _rightInRange = false;
    private float _originalScale;
    private MindMapNodeAnimator _animator;
    [Header("Scale Line")]
    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private float lineWidth = 0.005f;
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.6f);

    public LineRenderer        _scaleLine;
    public bool IsScaling => _isScaling;



    private Action<InputAction.CallbackContext> _onLeftGrip;
    private Action<InputAction.CallbackContext> _onRightGrip;
    private Action<InputAction.CallbackContext> _onLeftRelease;
    private Action<InputAction.CallbackContext> _onRightRelease;

    private void Awake()
    {
        _animator = GetComponent<MindMapNodeAnimator>();

        foreach (var tag in FindObjectsByType<ControllerTag>(FindObjectsSortMode.None))
        {
            if (tag.isLeft) _leftControllerTransform  = tag.transform;
            else            _rightControllerTransform = tag.transform;
        }
        _originalScale = transform.localScale.x;

        _onLeftGrip    = _ => OnGrip(_leftControllerTransform);
        _onRightGrip   = _ => OnGrip(_rightControllerTransform);
        _onLeftRelease = _ => OnRelease(_leftControllerTransform);
        _onRightRelease= _ => OnRelease(_rightControllerTransform);

        _scaleLine = Instantiate(lineRendererPrefab);
        _scaleLine.transform.SetParent(null);
        _scaleLine.positionCount = 2;
        _scaleLine.startWidth = lineWidth;
        _scaleLine.endWidth = lineWidth;
        _scaleLine.gameObject.SetActive(false);
        BuildScaleLine();
    }

    private void OnEnable()
    {
        Register(leftGripAction,  _onLeftGrip,  _onLeftRelease);
        Register(rightGripAction, _onRightGrip, _onRightRelease);
    }

    private void OnDisable()
    {
        Unregister(leftGripAction,  _onLeftGrip,  _onLeftRelease);
        Unregister(rightGripAction, _onRightGrip, _onRightRelease);
    }
    private void OnGrip(Transform controller)
    {
        bool inRangeForThisHand = (controller == _leftControllerTransform) ? _leftInRange : _rightInRange;

        if (_handA == null && inRangeForThisHand)
        {
            _handA     = controller;
            _handAHeld = true;
            TryBeginScale();
        }
        else if (_handA != null && _handB == null && controller != _handA)
        {
            _handB     = controller;
            _handBHeld = true;
            TryBeginScale();
        }
    }

    private void OnRelease(Transform controller)
    {
        if (controller == _handA)
        {
            _handA     = null;
            _handAHeld = false;
            StopScaling();
        }
        else if (controller == _handB)
        {
            _handB     = null;
            _handBHeld = false;
            StopScaling();
        }
    }
    private void OnDestroy()
    {
        if (_scaleLine != null) Destroy(_scaleLine.gameObject);
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

    private void TryBeginScale()
    {
        if (!_handAHeld || !_handBHeld) return;
        if (_handA == null || _handB == null) return;
        if (_isScaling) return;

        _initialDistance          = Vector3.Distance(_handA.position, _handB.position);
        _initialScale             = transform.localScale.x;
        _isScaling                = true;
        _animator.scalingOverride = true;
        _scaleLine.gameObject.SetActive(true);

        Debug.Log($"[NodeScaler] Started — dist: {_initialDistance:F3} scale: {_initialScale:F3}");
    }

    private void StopScaling()
    {
        if (!_isScaling) return;

        _animator.UpdateBaseScale(transform.localScale);
        _animator.scalingOverride = false;
        _isScaling                = false;
        _scaleLine.gameObject.SetActive(false);

        Debug.Log($"[NodeScaler] Baked — new base: {transform.localScale.x:F3}");
    }

    private void Update()
    {
        if (!_isScaling) return;
        Debug.Log("Is scaling");

        if (_handA == null || _handB == null) return;
        if (_initialDistance < 0.001f) return;

        float currentDist = Vector3.Distance(_handA.position, _handB.position);
        float newScale    = Mathf.Clamp(
            _initialScale * (currentDist / _initialDistance),
            _originalScale * minScale, 
            _originalScale * maxScale  
        );

        transform.localScale = Vector3.one * newScale;

        _scaleLine.SetPosition(0, _handA.position);
        _scaleLine.SetPosition(1, _handB.position);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GameController")) return;
        var tag = other.GetComponent<ControllerTag>();
        if (tag == null) return;

        if (tag.isLeft) _leftInRange  = true;
        else            _rightInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("GameController")) return;
        var tag = other.GetComponent<ControllerTag>();
        if (tag == null) return;

        if (tag.isLeft)
        {
            _leftInRange = false;
            if (!_handAHeld || _handA != _leftControllerTransform)
                if (_handA == _leftControllerTransform) _handA = null;
        }
        else
        {
            _rightInRange = false;
            if (_handB == _rightControllerTransform && !_handBHeld) _handB = null;
        }
    }


    private void BuildScaleLine()
    {
        var lineGO = new GameObject("ScaleLine");
        lineGO.transform.SetParent(null);

        _scaleLine               = lineGO.AddComponent<LineRenderer>();
        _scaleLine.positionCount = 2;
        _scaleLine.startWidth    = lineWidth;
        _scaleLine.endWidth      = lineWidth;
        _scaleLine.useWorldSpace = true;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color"); // fallback

        if (shader != null)
        {
            _scaleLine.material = new Material(shader);
            _scaleLine.material.color = lineColor;
        }
        else
        {
            Debug.LogError("[NodeScaler] Could not find a valid shader for scale line.");
        }
        // Gradient: node color on one end, fades to white
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.4f, 0.8f, 1f), 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.9f, 1f)
            }
        );
        _scaleLine.colorGradient = gradient;

        lineGO.SetActive(false);
    }
}