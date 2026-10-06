// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-07 by Omid Ameri
// Last Modified: 2026-07-15 by Omid Ameri

// Course: P6

using DG.Tweening;
using UnityEngine;

public class LadyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform headTarget;
    [SerializeField] private Transform visualRoot;

    [Header("Position Around Player")]
    [SerializeField] private float forwardDistance = 1.5f;
    [SerializeField] private float sideDistance = 0.7f;
    [SerializeField] private float verticalOffset = -0.5f;
    [SerializeField] private float verticalVariation = 0.1f;

    [Header("Movement")]
    [SerializeField] private float minMoveDuration = 2.5f;
    [SerializeField] private float maxMoveDuration = 4f;
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private float maxWaitTime = 6f;

    [Header("Return To View")]
    [SerializeField] private float maxViewAngle = 55f;
    [SerializeField] private float returnToViewDuration = 1.5f;
    [SerializeField] private float viewCheckInterval = 0.25f;

    [Header("Facing User")]
    [SerializeField] private float rotationSmoothSpeed = 8f;
    [SerializeField] private float facingYawOffset = -90f;

    [Header("Floating")]
    [SerializeField] private float bobAmount = 0.04f;
    [SerializeField] private float bobSpeed = 1.2f;

    private Vector3 visualStartLocalPosition;

    private Tween movementTween;

    private float nextMoveTime;
    private float nextViewCheckTime;

    private bool isReturningToView;

    private void Start()
    {
        if (headTarget == null && Camera.main != null)
        {
            headTarget = Camera.main.transform;
        }

        if (visualRoot != null)
        {
            visualStartLocalPosition = visualRoot.localPosition;
        }

        ScheduleNextMove();
    }

    private void Update()
    {
        if (headTarget == null)
        {
            return;
        }

        UpdateFacingPlayer();
        UpdateFloating();
        CheckIfOutsideView();

        if (isReturningToView)
        {
            return;
        }

        if (Time.time >= nextMoveTime &&
            (movementTween == null || !movementTween.IsActive()))
        {
            MoveToRandomPosition();
        }
    }

    private void OnDisable()
    {
        movementTween?.Kill();
    }

    private void UpdateFacingPlayer()
    {
        Vector3 directionToPlayer =
            headTarget.position - transform.position;

        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(
            directionToPlayer.normalized,
            Vector3.up
        );

        Quaternion modelOffset = Quaternion.Euler(
            0f,
            facingYawOffset,
            0f
        );

        Quaternion targetRotation =
            lookRotation * modelOffset;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    private void UpdateFloating()
    {
        if (visualRoot == null)
        {
            return;
        }

        float bobOffset =
            Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        visualRoot.localPosition =
            visualStartLocalPosition +
            Vector3.up * bobOffset;
    }

    private void CheckIfOutsideView()
    {
        if (Time.time < nextViewCheckTime)
        {
            return;
        }

        nextViewCheckTime =
            Time.time + viewCheckInterval;

        Vector3 headForward = headTarget.forward;
        headForward.y = 0f;
        headForward.Normalize();

        Vector3 directionToLady =
            transform.position - headTarget.position;

        directionToLady.y = 0f;
        directionToLady.Normalize();

        float viewAngle = Vector3.Angle(
            headForward,
            directionToLady
        );

        if (viewAngle > maxViewAngle)
        {
            ReturnToPlayerView();
        }
    }

    private void ReturnToPlayerView()
    {
        if (isReturningToView)
        {
            return;
        }

        isReturningToView = true;

        movementTween?.Kill();

        Vector3 targetPosition =
            GetPositionInFrontOfPlayer(
                0f,
                0f
            );

        movementTween = transform.DOMove(
                targetPosition,
                returnToViewDuration
            )
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                isReturningToView = false;
                ScheduleNextMove();
            });
    }

    private void MoveToRandomPosition()
    {
        float randomSide = Random.Range(
            -sideDistance,
            sideDistance
        );

        float randomVertical = Random.Range(
            -verticalVariation,
            verticalVariation
        );

        Vector3 targetPosition =
            GetPositionInFrontOfPlayer(
                randomSide,
                randomVertical
            );

        float moveDuration = Random.Range(
            minMoveDuration,
            maxMoveDuration
        );

        movementTween?.Kill();

        movementTween = transform.DOMove(
                targetPosition,
                moveDuration
            )
            .SetEase(Ease.InOutSine)
            .OnComplete(ScheduleNextMove);
    }

    private Vector3 GetPositionInFrontOfPlayer(
        float sideOffset,
        float verticalRandomOffset)
    {
        Vector3 flatForward = headTarget.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector3 flatRight = headTarget.right;
        flatRight.y = 0f;
        flatRight.Normalize();

        return headTarget.position +
               flatForward * forwardDistance +
               flatRight * sideOffset +
               Vector3.up *
               (verticalOffset + verticalRandomOffset);
    }

    private void ScheduleNextMove()
    {
        nextMoveTime =
            Time.time +
            Random.Range(
                minWaitTime,
                maxWaitTime
            );
    }
}