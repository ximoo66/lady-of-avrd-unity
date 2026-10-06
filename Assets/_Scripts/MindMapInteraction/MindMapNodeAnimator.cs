//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: used for making the node feel more responsive by changing color and scale

using UnityEngine;

public class MindMapNodeAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer nodeRenderer;
    [SerializeField] private Light nodeLight;


    [Header("Hover")]
    [SerializeField] private float hoverScaleMultiplier = 1.15f;
    [SerializeField] private float scaleSmoothSpeed     = 10f;

    [Header("Colors")]
    [SerializeField] private Color idleColor      = new Color(121/255f, 85/255f, 191/255f);
    [SerializeField] private Color hoverColor     = new Color(0.6f, 0.8f, 1f);
    [SerializeField] private Color recordingColor = new Color(0f, 0.7f, 0.3f);
    [SerializeField] private string colorProperty = "_Color";

    
    private Color _nodeBaseColor;

    [Header("Recording Pulse")]
    [SerializeField] private float pulseSpeed  = 4f;
    [SerializeField] private float pulseAmount = 0.08f;

    // ── state ─────────────────────────────────────────────────

    private enum State { Idle, Hover, Recording }
    private State _state = State.Idle;

    private Vector3 _baseScale;
    private Vector3 _targetScale;
    private Vector3 _startLocalPosition;
    private float   _motionOffset;

    private MaterialPropertyBlock _propBlock;

    // ── lifecycle ─────────────────────────────────────────────

    
    private void Awake()
    {
        _baseScale          = transform.localScale;
        _targetScale        = _baseScale;

        _nodeBaseColor = idleColor;

        _propBlock          = new MaterialPropertyBlock();

        if (nodeRenderer == null)
            nodeRenderer = GetComponent<Renderer>();
    }

    private void Start() => ApplyState();

    private void Update()
    {
        UpdateScale();
    }

    // ── public API ────────────────────────────────────────────

    public void SetHover(bool isHovered)
    {
        if (_state == State.Recording) return;
        _state = isHovered ? State.Hover : State.Idle;
        ApplyState();
    }

    public void SetRecording(bool isRecording)
    {
        _state = isRecording ? State.Recording : State.Idle;
        ApplyState();
    }

    // ── state application ─────────────────────────────────────

    private void ApplyState()
    {
        switch (_state)
        {
            case State.Idle:
                _targetScale = _baseScale;
                SetColor(_nodeBaseColor); 
                SetLightIntensity(1f);
                break;
            
            case State.Hover:
                _targetScale = _baseScale * hoverScaleMultiplier;
                SetColor(_nodeBaseColor);
                SetLightIntensity(1.7f);
                break;

            case State.Recording:
                SetLightIntensity(2.5f);
                break;
        }
    }
    public void SetNodeColor(Color color)
    {
        _nodeBaseColor = color;
        SetColor(_nodeBaseColor);
    }

    // ── scale ─────────────────────────────────────

    public bool scalingOverride = false;

    private void UpdateScale()
    {
        if (scalingOverride) return;
        if (_state == State.Recording)
        {
            float pulse  = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            _targetScale = _baseScale * hoverScaleMultiplier + Vector3.one * pulse;

            // Pulse color between recordingColor and bright white
            float t     = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            SetColor(Color.Lerp(recordingColor, Color.white * 1.5f, t));
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            _targetScale,
            Time.deltaTime * scaleSmoothSpeed);
    }
    public void UpdateBaseScale(Vector3 newBase)
    {
        _baseScale   = newBase;
        _targetScale = newBase; // prevents snap back on next state change
    }

    // ── helpers ───────────────────────────────────────────────

    private void SetColor(Color color)
    {
        if (nodeRenderer == null) return;
        nodeRenderer.GetPropertyBlock(_propBlock);
        if (nodeRenderer.sharedMaterial != null &&
            nodeRenderer.sharedMaterial.HasProperty(colorProperty))
            _propBlock.SetColor(colorProperty, color);
        nodeRenderer.SetPropertyBlock(_propBlock);
    }

    private void SetLightIntensity(float intensity)
    {
        if (nodeLight != null)
            nodeLight.intensity = intensity;
    }
}