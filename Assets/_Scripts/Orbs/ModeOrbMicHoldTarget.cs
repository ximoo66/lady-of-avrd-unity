// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-15 by Omid Ameri
// Last Modified: 2026-07-15 by Omid Ameri

// Course: P6

using DG.Tweening;
using UnityEngine;

public class ModeOrbMicHoldTarget : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private LLMMode mode;

    [Header("References")]
    [SerializeField] private OrbController orbController;
    [SerializeField] private Renderer orbRenderer;
    [SerializeField] private Light orbLight;
    [SerializeField] private AudioSource audioSource;

    [Header("Held Appearance")]
    [SerializeField] private Vector3 heldLocalScale = Vector3.one * 0.06f;
    [SerializeField] private Color recordingColor = Color.green;

    [Header("Animation")]
    [SerializeField] private float moveToHandDuration = 0.2f;
    [SerializeField] private float returnDuration = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioClip micStartAudio;
    [SerializeField] private AudioClip micStopAudio;

    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private AIManagerMain aiManagerMain;
    private MaterialPropertyBlock propertyBlock;

    private Transform holdTarget;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;

    private bool isHeldByController;
    private bool isRecording;

    public bool IsHeldByController => isHeldByController;

    private void Awake()
    {
        aiManagerMain = FindFirstObjectByType<AIManagerMain>();

        if (aiManagerMain == null)
        {
            Debug.LogError(
                $"[{nameof(ModeOrbMicHoldTarget)}] AIManagerMain was not found in the scene.",
                this
            );
        }

        propertyBlock = new MaterialPropertyBlock();

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale = transform.localScale;

        if (orbController == null)
        {
            orbController = GetComponent<OrbController>();
        }

        if (orbRenderer == null)
        {
            orbRenderer = GetComponentInChildren<Renderer>();
        }

        if (orbLight == null)
        {
            orbLight = GetComponentInChildren<Light>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void LateUpdate()
    {
        if (!isHeldByController || holdTarget == null)
        {
            return;
        }

        transform.position = holdTarget.position;
    }

    public void BeginMicHold(Transform newHoldTarget)
    {
        if (isHeldByController || newHoldTarget == null)
        {
            return;
        }

        if (aiManagerMain == null)
        {
            Debug.LogError(
                $"[{nameof(ModeOrbMicHoldTarget)}] Cannot start recording. AIManagerMain is missing.",
                this
            );

            return;
        }

        isHeldByController = true;
        holdTarget = newHoldTarget;

        transform.DOKill();

        if (orbController != null)
        {
            orbController.Unfocus();
            orbController.enabled = false;
        }

        aiManagerMain.SwitchMode(mode);
        aiManagerMain.StartRec();

        isRecording = true;

        ApplyColor(recordingColor);
        PlayAudio(micStartAudio);

        transform.DOMove(
                holdTarget.position,
                moveToHandDuration
            )
            .SetEase(Ease.OutSine);

        transform.DOScale(
                heldLocalScale,
                moveToHandDuration
            )
            .SetEase(Ease.OutSine);
    }

    public void EndMicHold()
    {
        if (!isHeldByController)
        {
            return;
        }

        isHeldByController = false;
        holdTarget = null;

        transform.DOKill();

        if (isRecording && aiManagerMain != null)
        {
            aiManagerMain.StopAndSend();

            isRecording = false;

            PlayAudio(micStopAudio);
        }

        transform.SetParent(originalParent, true);

        transform.DOLocalMove(
                originalLocalPosition,
                returnDuration
            )
            .SetEase(Ease.InOutSine);

        transform.DOLocalRotateQuaternion(
                originalLocalRotation,
                returnDuration
            )
            .SetEase(Ease.InOutSine);

        transform.DOScale(
                originalLocalScale,
                returnDuration
            )
            .SetEase(Ease.InOutSine)
            .OnComplete(FinishReturn);
    }

    private void FinishReturn()
    {
        if (orbController == null)
        {
            return;
        }

        orbController.enabled = true;
        orbController.ReturnToIdle();
    }

    private void ApplyColor(Color color)
    {
        if (orbRenderer != null)
        {
            orbRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(ColorProperty, color);
            orbRenderer.SetPropertyBlock(propertyBlock);
        }

        if (orbLight != null)
        {
            orbLight.color = color;
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
}