// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-30 by Omid Ameri
// Last Modified: 2026-07-15 by Omid Ameri

// Course: P6

using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerOrbInteractor : MonoBehaviour
{
    [Header("Controller Detection")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float hoverRadius = 0.08f;

    [Header("Held Orb Target")]
    [Tooltip("A child object near the controller where the orb follows while recording.")]
    [SerializeField] private Transform orbHoldTarget;

    [Header("Orb Detection")]
    [SerializeField] private LayerMask orbLayerMask;

    [Header("Trigger Input")]
    [SerializeField] private InputActionReference triggerAction;
    [SerializeField] private float triggerPressedThreshold = 0.6f;

    [Header("OVR Fallback")]
    [SerializeField] private bool useOvrFallback = true;
    [SerializeField]
    private OVRInput.Controller ovrController =
        OVRInput.Controller.RTouch;

    [Header("Debug")]
    [SerializeField] private bool drawDebugSphere = true;
    [SerializeField] private bool showDebugLogs = true;

    private readonly Collider[] overlapResults = new Collider[12];

    private OrbController hoveredOrb;
    private ModeOrbMicHoldTarget activeModeOrb;

    private bool wasTriggerHeld;

    private void OnEnable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.Disable();
        }

        EndModeOrbHoldIfNeeded();
        SetHoveredOrb(null);
    }

    private void Update()
    {
        UpdateHoveredOrb();
        UpdateTriggerInput();
    }

    private void UpdateHoveredOrb()
    {
        if (activeModeOrb != null &&
            activeModeOrb.IsHeldByController)
        {
            return;
        }

        Transform point = interactionPoint != null
            ? interactionPoint
            : transform;

        int hitCount = Physics.OverlapSphereNonAlloc(
            point.position,
            hoverRadius,
            overlapResults,
            orbLayerMask,
            QueryTriggerInteraction.Collide
        );

        OrbController nearestOrb = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            OrbController candidate =
                overlapResults[i].GetComponentInParent<OrbController>();

            if (candidate == null)
            {
                continue;
            }

            ModeOrbMicHoldTarget modeOrb =
                candidate.GetComponent<ModeOrbMicHoldTarget>();

            if (modeOrb == null)
            {
                continue;
            }

            float distance = Vector3.SqrMagnitude(
                candidate.transform.position - point.position);

            if (distance >= nearestDistance)
            {
                continue;
            }

            nearestDistance = distance;
            nearestOrb = candidate;
        }

        SetHoveredOrb(nearestOrb);
    }

    private void UpdateTriggerInput()
    {
        bool isTriggerHeld = ReadTriggerHeld();

        if (isTriggerHeld && !wasTriggerHeld)
        {
            HandleTriggerPressed();
        }

        if (!isTriggerHeld && wasTriggerHeld)
        {
            HandleTriggerReleased();
        }

        wasTriggerHeld = isTriggerHeld;
    }

    private void HandleTriggerPressed()
    {
        Log("Trigger pressed.");

        if (hoveredOrb == null)
        {
            Log("Trigger pressed, but no orb is hovered.");
            return;
        }

        ModeOrbMicHoldTarget modeOrb =
            hoveredOrb.GetComponent<ModeOrbMicHoldTarget>();

        if (modeOrb == null)
        {
            Log($"No ModeOrbMicHoldTarget on {hoveredOrb.name}");
            return;
        }

        StartModeOrbHold(modeOrb);
    }

    private void HandleTriggerReleased()
    {
        Log("Trigger released.");

        EndModeOrbHoldIfNeeded();
    }

    private void StartModeOrbHold(
        ModeOrbMicHoldTarget modeOrb)
    {
        if (modeOrb == null)
        {
            return;
        }

        activeModeOrb = modeOrb;

        Transform followTarget = orbHoldTarget != null
            ? orbHoldTarget
            : transform;

        if (hoveredOrb != null)
        {
            hoveredOrb.Unfocus();
        }

        activeModeOrb.BeginMicHold(followTarget);

        Log($"Started mode orb hold: {activeModeOrb.name}");
    }

    private void EndModeOrbHoldIfNeeded()
    {
        if (activeModeOrb == null)
        {
            return;
        }

        activeModeOrb.EndMicHold();

        Log($"Ended mode orb hold: {activeModeOrb.name}");

        activeModeOrb = null;

        SetHoveredOrb(null);
    }

    private bool ReadTriggerHeld()
    {
        if (triggerAction != null &&
            triggerAction.action.enabled)
        {
            float triggerValue =
                triggerAction.action.ReadValue<float>();

            if (triggerValue >= triggerPressedThreshold)
            {
                return true;
            }
        }

        if (useOvrFallback)
        {
            float ovrTriggerValue = OVRInput.Get(
                OVRInput.Axis1D.PrimaryIndexTrigger,
                ovrController
            );

            if (ovrTriggerValue >= triggerPressedThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private void SetHoveredOrb(
        OrbController newHoveredOrb)
    {
        if (hoveredOrb == newHoveredOrb)
        {
            return;
        }

        if (hoveredOrb != null)
        {
            Log($"Unhover orb: {hoveredOrb.name}");
            hoveredOrb.Unfocus();
        }

        hoveredOrb = newHoveredOrb;

        if (hoveredOrb != null)
        {
            Log($"Hover orb: {hoveredOrb.name}");
            hoveredOrb.Focus();
        }
    }

    private void Log(string message)
    {
        if (!showDebugLogs)
        {
            return;
        }

        Debug.Log(
            $"[ControllerOrbInteractor] {message}");
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugSphere)
        {
            return;
        }

        Transform point = interactionPoint != null
            ? interactionPoint
            : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            point.position,
            hoverRadius);
    }
}