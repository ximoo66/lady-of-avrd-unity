// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-09 by Omid Ameri
// Last Modified: 2026-06-16 by Omid Ameri

// Course: P6

using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class OrbController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private OrbData orbData;

    [Header("Visual References")]
    [SerializeField] private Renderer orbRenderer;
    [SerializeField] private Light orbLight;
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Transform indicatorAnchor;

    [Header("Idle Motion")]
    [SerializeField] private bool useIdleMotion = true;
    [SerializeField] private float bobAmplitude = 0.04f;
    [SerializeField] private float bobSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 20f;

    [Header("State Feedback")]
    [SerializeField] private float hoverScaleMultiplier = 1.15f;
    [SerializeField] private float selectedScaleMultiplier = 1.3f;
    [SerializeField] private float processingPulseSpeed = 4f;
    [SerializeField] private float processingPulseAmount = 0.08f;
    [SerializeField] private float scaleSmoothSpeed = 10f;
    [SerializeField] private bool showLabelOnHover = true;

    [Header("Selection Test")]
    [SerializeField] private bool returnToIdleAfterSelect = true;
    [SerializeField] private float returnToIdleDelay = 0.4f;

    [Header("Shader Color")]
    [SerializeField] private string colorPropertyName = "_Color";
    [SerializeField] private float idleColorIntensity = 1f;
    [SerializeField] private float hoverColorIntensity = 1.4f;
    [SerializeField] private float selectedColorIntensity = 2f;
    [SerializeField] private float processingColorIntensity = 2.5f;
    private MaterialPropertyBlock propertyBlock;

    private Material orbMaterialInstance;
    private GameObject spawnedIndicatorInstance;
    private AudioSource audioSource;

    private Vector3 startLocalPosition;
    private Vector3 baseScale;
    private Vector3 targetScale;

    private float motionOffset;
    private OrbInteractionState currentState = OrbInteractionState.Idle;

    public OrbData OrbData => orbData;
    public OrbInteractionState CurrentState => currentState;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        baseScale = transform.localScale;
        targetScale = baseScale;
        motionOffset = Random.Range(0f, 100f);

        audioSource = GetComponent<AudioSource>();

        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        ApplyDataToVisuals();
        SetState(OrbInteractionState.Idle);
    }

    private void Update()
    {
        UpdateIdleMotion();
        UpdateScaleFeedback();
    }

    public void SetData(OrbData newOrbData)
    {
        orbData = newOrbData;
        ApplyDataToVisuals();
    }

    public void Focus()
    {
        if (currentState is OrbInteractionState.Selected or OrbInteractionState.Processing)
        {
            return;
        }

        SetState(OrbInteractionState.Hover);
    }

    public void Unfocus()
    {
        if (currentState is OrbInteractionState.Selected or OrbInteractionState.Processing)
        {
            return;
        }

        SetState(OrbInteractionState.Idle);
    }

    public void Select()
    {
        SetState(OrbInteractionState.Selected);

        ModeSelectorOrb modeSelectorOrb = GetComponent<ModeSelectorOrb>();

        if (modeSelectorOrb != null)
        {
            modeSelectorOrb.SelectMode();
        }

        Debug.Log($"Orb selected: {(orbData != null ? orbData.OrbName : name)}");

        if (returnToIdleAfterSelect)
        {
            CancelInvoke(nameof(ReturnToIdle));
            Invoke(nameof(ReturnToIdle), returnToIdleDelay);
        }
    }

    public void Cancel()
    {
        SetState(OrbInteractionState.Cancelled);

        CancelInvoke(nameof(ReturnToIdle));
        Invoke(nameof(ReturnToIdle), returnToIdleDelay);
    }

    public void SetProcessing()
    {
        SetState(OrbInteractionState.Processing);
    }

    public void SetResult()
    {
        SetState(OrbInteractionState.Result);
    }

    public void ReturnToIdle()
    {
        SetState(OrbInteractionState.Idle);
    }

    public void SetState(OrbInteractionState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case OrbInteractionState.Idle:
                targetScale = baseScale;
                SetLabelVisible(false);
                SetLightIntensity(1f);
                ApplyShaderColor(idleColorIntensity);
                break;

            case OrbInteractionState.Hover:
                targetScale = baseScale * hoverScaleMultiplier;
                SetLabelVisible(showLabelOnHover);
                SetLightIntensity(1.7f);
                ApplyShaderColor(hoverColorIntensity);
                PlayOneShot(orbData != null ? orbData.HoverAudio : null);
                break;

            case OrbInteractionState.Selected:
                targetScale = baseScale * selectedScaleMultiplier;
                SetLabelVisible(true);
                SetLightIntensity(2.2f);
                ApplyShaderColor(selectedColorIntensity);
                PlayOneShot(orbData != null ? orbData.SelectAudio : null);
                break;

            case OrbInteractionState.Processing:
                SetLabelVisible(true);
                SetLightIntensity(2.6f);
                PlayOneShot(orbData != null ? orbData.ProcessingAudio : null);
                break;

            case OrbInteractionState.Result:
                targetScale = baseScale * hoverScaleMultiplier;
                SetLabelVisible(true);
                SetLightIntensity(2f);
                PlayOneShot(orbData != null ? orbData.ResultAudio : null);
                break;

            case OrbInteractionState.Cancelled:
                targetScale = baseScale;
                SetLabelVisible(false);
                SetLightIntensity(0.8f);
                PlayOneShot(orbData != null ? orbData.CancelAudio : null);
                break;
        }
    }

    [ContextMenu("Test State/Idle")]
    private void TestIdle()
    {
        SetState(OrbInteractionState.Idle);
    }

    [ContextMenu("Test State/Hover")]
    private void TestHover()
    {
        SetState(OrbInteractionState.Hover);
    }

    [ContextMenu("Test State/Selected")]
    private void TestSelected()
    {
        Select();
    }

    [ContextMenu("Test State/Processing")]
    private void TestProcessing()
    {
        SetProcessing();
    }

    [ContextMenu("Test State/Result")]
    private void TestResult()
    {
        SetResult();
    }

    [ContextMenu("Test State/Cancelled")]
    private void TestCancelled()
    {
        Cancel();
    }

    private void CreateMaterialInstance()
    {
        if (orbRenderer == null)
        {
            return;
        }

        orbMaterialInstance = orbRenderer.material;
    }

    private void ApplyDataToVisuals()
    {
        if (orbData == null)
        {
            Debug.LogWarning($"{name} has no OrbData assigned.");
            return;
        }

        ApplyColor();
        ApplyIcon();
        ApplyLabel();
        ApplyIndicator();
    }

    private void ApplyColor()
    {
        ApplyShaderColor(idleColorIntensity);
    }
    private void ApplyShaderColor(float intensity)
    {
        if (orbData == null || orbRenderer == null)
        {
            return;
        }

        Color finalColor = orbData.OrbColor * intensity;

        orbRenderer.GetPropertyBlock(propertyBlock);

        if (orbRenderer.sharedMaterial != null &&
            orbRenderer.sharedMaterial.HasProperty(colorPropertyName))
        {
            propertyBlock.SetColor(colorPropertyName, finalColor);
        }

        orbRenderer.SetPropertyBlock(propertyBlock);

        if (orbLight != null)
        {
            orbLight.color = orbData.OrbColor;
            orbLight.intensity = intensity;
        }
    }

    private void ApplyIcon()
    {
        if (iconRenderer == null)
        {
            return;
        }

        iconRenderer.sprite = orbData.Icon;
        iconRenderer.gameObject.SetActive(orbData.Icon != null);
    }

    private void ApplyLabel()
    {
        if (labelText == null)
        {
            return;
        }

        labelText.text = orbData.OrbName;
    }

    private void ApplyIndicator()
    {
        if (indicatorAnchor == null)
        {
            return;
        }

        ClearIndicator();

        if (orbData.IndicatorPrefab == null)
        {
            return;
        }

        spawnedIndicatorInstance = Instantiate(
            orbData.IndicatorPrefab,
            indicatorAnchor.position,
            indicatorAnchor.rotation,
            indicatorAnchor);
    }

    private void ClearIndicator()
    {
        if (spawnedIndicatorInstance == null)
        {
            return;
        }

        Destroy(spawnedIndicatorInstance);
        spawnedIndicatorInstance = null;
    }

    private void UpdateIdleMotion()
    {
        if (!useIdleMotion)
        {
            return;
        }

        float bobOffset = Mathf.Sin((Time.time + motionOffset) * bobSpeed) * bobAmplitude;
        transform.localPosition = startLocalPosition + Vector3.up * bobOffset;

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void UpdateScaleFeedback()
    {
        if (currentState == OrbInteractionState.Processing)
        {
            float pulse = Mathf.Sin(Time.time * processingPulseSpeed) * processingPulseAmount;
            targetScale = baseScale * selectedScaleMultiplier + Vector3.one * pulse;
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSmoothSpeed);
    }

    private void SetLabelVisible(bool isVisible)
    {
        if (labelText == null)
        {
            return;
        }

        labelText.gameObject.SetActive(isVisible);
    }

    private void SetLightIntensity(float intensity)
    {
        if (orbLight == null)
        {
            return;
        }

        orbLight.intensity = intensity;
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}