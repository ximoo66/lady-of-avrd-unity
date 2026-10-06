// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-20 by Omid Ameri
// Last Modified: 2026-06-20 by Omid Ameri

// Course: P6

using UnityEngine;
using UnityEngine.InputSystem;

public class OrbDirectHandInteractor : MonoBehaviour
{
    [Header("Hand Detection")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float hoverRadius = 0.04f;

    [Header("Orb Detection")]
    [SerializeField] private LayerMask orbLayerMask;

    [Header("Pinch Input")]
    [SerializeField] private InputActionReference pinchAction;
    [SerializeField] private OVRHand fallbackOvrHand;
    [SerializeField] private float selectionCooldown = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugSphere = true;

    private readonly Collider[] overlapResults = new Collider[12];

    private OrbController hoveredOrb;
    private bool wasPinching;
    private float nextAllowedSelectionTime;

    private void OnEnable()
    {
        if (pinchAction == null)
        {
            return;
        }

        pinchAction.action.Enable();
    }

    private void OnDisable()
    {
        if (pinchAction != null)
        {
            pinchAction.action.Disable();
        }

        SetHoveredOrb(null);
    }

    private void Update()
    {
        UpdateHoveredOrb();
        UpdatePinchInput();
    }

    private void UpdateHoveredOrb()
    {
        Transform point = interactionPoint != null ? interactionPoint : transform;

        int hitCount = Physics.OverlapSphereNonAlloc(
            point.position,
            hoverRadius,
            overlapResults,
            orbLayerMask,
            QueryTriggerInteraction.Collide);

        OrbController nearestOrb = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            OrbController candidate = overlapResults[i].GetComponentInParent<OrbController>();

            if (candidate == null)
            {
                continue;
            }

            float distance = Vector3.SqrMagnitude(candidate.transform.position - point.position);

            if (distance >= nearestDistance)
            {
                continue;
            }

            nearestDistance = distance;
            nearestOrb = candidate;
        }

        SetHoveredOrb(nearestOrb);
    }

    private void UpdatePinchInput()
    {
        bool isPinching = ReadPinch();

        if (isPinching && !wasPinching)
        {
            TrySelectHoveredOrb();
        }

        wasPinching = isPinching;
    }

    private bool ReadPinch()
    {
        if (pinchAction != null && pinchAction.action.enabled)
        {
            return pinchAction.action.WasPressedThisFrame();
        }

        if (fallbackOvrHand != null)
        {
            return fallbackOvrHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        }

        return false;
    }

    private void TrySelectHoveredOrb()
    {
        if (hoveredOrb == null || Time.time < nextAllowedSelectionTime)
        {
            return;
        }

        nextAllowedSelectionTime = Time.time + selectionCooldown;

        hoveredOrb.Select();

        ConversationActiveOrbToggle conversationToggle =
            hoveredOrb.GetComponent<ConversationActiveOrbToggle>();

        if (conversationToggle != null)
        {
            conversationToggle.ToggleConversationRecording();
        }
    }

    private void SetHoveredOrb(OrbController newHoveredOrb)
    {
        if (hoveredOrb == newHoveredOrb)
        {
            return;
        }

        if (hoveredOrb != null)
        {
            hoveredOrb.Unfocus();
        }

        hoveredOrb = newHoveredOrb;

        if (hoveredOrb != null)
        {
            hoveredOrb.Focus();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugSphere)
        {
            return;
        }

        Transform point = interactionPoint != null ? interactionPoint : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(point.position, hoverRadius);
    }
}