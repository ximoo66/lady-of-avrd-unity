// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-30 by Omid Ameri
// Last Modified: 2026-06-30 by Omid Ameri

// Course: P6

using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(OrbController))]
public class ActiveOrbMicHoldTarget : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] private AIManagerMain aiManagerMain;

    [Header("Visual References")]
    [SerializeField] private Renderer orbRenderer;
    [SerializeField] private Light orbLight;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip micStartAudio;
    [SerializeField] private AudioClip micStopAudio;

    [Header("Shader")]
    [SerializeField] private string colorPropertyName = "_Color";

    [Header("Mic Colors")]
    [ColorUsage(true, true)]
    [SerializeField] private Color microphoneOffColor = Color.red;

    [ColorUsage(true, true)]
    [SerializeField] private Color microphoneOnColor = Color.green;

    [SerializeField] private float colorIntensity = 1.5f;

    [Header("Held Orb")]
    [Tooltip("Final scale of the orb while it is held around the controller.")]
    [SerializeField] private Vector3 heldLocalScale = new Vector3(0.08f, 0.08f, 0.08f);

    [Tooltip("Small offset from the controller target. Keep zero if controller should be inside the orb.")]
    [SerializeField] private Vector3 heldOffset = Vector3.zero;

    [Header("Tween")]
    [SerializeField] private float attachDuration = 0.18f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private Ease attachEase = Ease.OutCubic;
    [SerializeField] private Ease returnEase = Ease.OutCubic;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private OrbController orbController;
    private MaterialPropertyBlock propertyBlock;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;

    private Transform followTarget;

    private bool isRecording;
    private bool isHeldByController;
    private bool warnedMissingProperty;

    public bool IsHeldByController => isHeldByController;

    private void Awake()
    {
        orbController = GetComponent<OrbController>();
        propertyBlock = new MaterialPropertyBlock();

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale = transform.localScale;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        AIManagerMain.OnStartListening += HandleStartListening;
        AIManagerMain.OnStopListening += HandleStopListening;
        AIManagerMain.OnError += HandleStopListening;
    }

    private void OnDisable()
    {
        AIManagerMain.OnStartListening -= HandleStartListening;
        AIManagerMain.OnStopListening -= HandleStopListening;
        AIManagerMain.OnError -= HandleStopListening;

        transform.DOKill();
    }

    private void Start()
    {
        ApplyColor(microphoneOffColor);
    }

    private void LateUpdate()
    {
        if (isHeldByController && followTarget != null)
        {
            FollowControllerTarget();
        }

        ApplyColor(isRecording ? microphoneOnColor : microphoneOffColor);
    }

    public void BeginMicHold(Transform controllerFollowTarget)
    {
        if (isHeldByController)
        {
            return;
        }

        if (controllerFollowTarget == null)
        {
            Debug.LogWarning("ActiveOrbMicHoldTarget received no controller follow target.");
            return;
        }

        if (aiManagerMain == null)
        {
            Debug.LogWarning("ActiveOrbMicHoldTarget has no AIManagerMain assigned.");
            return;
        }

        isHeldByController = true;
        followTarget = controllerFollowTarget;

        transform.DOKill();

        if (orbController != null)
        {
            orbController.Unfocus();
            orbController.enabled = false;
        }

        Log($"Start recording in mode: {aiManagerMain.currentMode}");

        aiManagerMain.StartRec();
        PlayAudio(micStartAudio);

        Vector3 targetPosition = GetHeldTargetPosition();

        Sequence attachSequence = DOTween.Sequence();
        attachSequence.Join(transform.DOMove(targetPosition, attachDuration).SetEase(attachEase));
        attachSequence.Join(transform.DOScale(heldLocalScale, attachDuration).SetEase(attachEase));
    }

    public void EndMicHold()
    {
        if (!isHeldByController)
        {
            return;
        }

        isHeldByController = false;
        followTarget = null;

        if (aiManagerMain != null && isRecording)
        {
            Log($"Stop and send recording in mode: {aiManagerMain.currentMode}");
            aiManagerMain.StopAndSend();
            PlayAudio(micStopAudio);
        }

        ReturnToMenu();
    }

    private void FollowControllerTarget()
    {
        transform.position = GetHeldTargetPosition();
    }

    private Vector3 GetHeldTargetPosition()
    {
        return followTarget.position + followTarget.TransformDirection(heldOffset);
    }

    private void ReturnToMenu()
    {
        transform.DOKill();

        if (transform.parent != originalParent)
        {
            transform.SetParent(originalParent, true);
        }

        Sequence returnSequence = DOTween.Sequence();

        returnSequence.Join(transform.DOLocalMove(originalLocalPosition, returnDuration).SetEase(returnEase));
        returnSequence.Join(transform.DOLocalRotateQuaternion(originalLocalRotation, returnDuration).SetEase(returnEase));
        returnSequence.Join(transform.DOScale(originalLocalScale, returnDuration).SetEase(returnEase));

        returnSequence.OnComplete(() =>
        {
            if (orbController != null)
            {
                orbController.enabled = true;
                orbController.ReturnToIdle();
            }
        });
    }

    private void HandleStartListening()
    {
        isRecording = true;
        ApplyColor(microphoneOnColor);
    }

    private void HandleStopListening()
    {
        isRecording = false;
        ApplyColor(microphoneOffColor);
    }

    private void ApplyColor(Color color)
    {
        if (orbRenderer == null)
        {
            return;
        }

        Color finalColor = color * colorIntensity;

        orbRenderer.GetPropertyBlock(propertyBlock);

        if (orbRenderer.sharedMaterial != null &&
            orbRenderer.sharedMaterial.HasProperty(colorPropertyName))
        {
            propertyBlock.SetColor(colorPropertyName, finalColor);
        }
        else if (!warnedMissingProperty)
        {
            warnedMissingProperty = true;

            Debug.LogWarning(
                $"Material does not have shader property '{colorPropertyName}'. " +
                "Check the Shader Graph Reference name."
            );
        }

        orbRenderer.SetPropertyBlock(propertyBlock);

        if (orbLight != null)
        {
            orbLight.color = color;
            orbLight.intensity = colorIntensity;
        }
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }

    private void Log(string message)
    {
        if (!showDebugLogs)
        {
            return;
        }

        Debug.Log($"[ActiveOrbMicHoldTarget] {message}");
    }
}
