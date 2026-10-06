// Authors: Omid Ameri, Ekaterina Siling, Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-16 by Omid Ameri
// Last Modified: 2026-06-27 by Noah Wendt

// Course: P6

using UnityEngine;

public class OrbOVRHandInteractor : MonoBehaviour
{
    [Header("Hand")]
    [SerializeField] private OVRHand ovrHand;
    [SerializeField] private Transform rayOrigin;

    [Header("Ray Settings")]
    [SerializeField] private float rayDistance = 2.5f;
    [SerializeField] private float sphereCastRadius = 0.04f;
    [SerializeField] private LayerMask orbLayerMask = ~0;

    [Header("Debug")]
    [SerializeField] private LineRenderer debugLine;

    private OrbController focusedOrb;
    private bool wasPinching;

    private void Update()
    {
        UpdateFocus();
        UpdatePinchSelection();
    }

    private void UpdateFocus()
    {
        Transform origin = rayOrigin != null ? rayOrigin : transform;

        bool hasHit = Physics.SphereCast(
            origin.position,
            sphereCastRadius,
            origin.forward,
            out RaycastHit hit,
            rayDistance,
            orbLayerMask,
            QueryTriggerInteraction.Ignore);

        OrbController hitOrb = null;
        Vector3 endPoint = origin.position + origin.forward * rayDistance;

        if (hasHit)
        {
            hitOrb = hit.collider.GetComponentInParent<OrbController>();
            endPoint = hit.point;
        }

        SetFocusedOrb(hitOrb);
        UpdateDebugLine(origin.position, endPoint);
    }

    private void UpdatePinchSelection()
    {
        if (ovrHand == null)
        {
            return;
        }

        bool isPinching = ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        if (isPinching && !wasPinching && focusedOrb != null)
        {
            focusedOrb.Select();
        }

        wasPinching = isPinching;
    }

    private void SetFocusedOrb(OrbController newFocusedOrb)
    {
        if (focusedOrb == newFocusedOrb)
        {
            return;
        }

        if (focusedOrb != null)
        {
            focusedOrb.Unfocus();
        }

        focusedOrb = newFocusedOrb;

        if (focusedOrb != null)
        {
            focusedOrb.Focus();
        }
    }

    private void UpdateDebugLine(Vector3 startPoint, Vector3 endPoint)
    {
        if (debugLine == null)
        {
            return;
        }

        debugLine.positionCount = 2;
        debugLine.SetPosition(0, startPoint);
        debugLine.SetPosition(1, endPoint);
    }
}