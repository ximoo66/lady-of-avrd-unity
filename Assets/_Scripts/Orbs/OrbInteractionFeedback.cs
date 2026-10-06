// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-24 by Noah Wendt
// Last Modified: 2026-06-24 by Noah Wendt

using System;
using DG.Tweening;
using UnityEngine;

public class OrbInteractionFeedback : MonoBehaviour
{

    public Transform parent, target;
    public Vector3 startScale, endScale;

    private bool isFollowing;
    
    void OnTriggerPressed()
    {
        transform.DOMove(target.position, .3f);
        transform.DOScale(endScale, .3f).OnComplete(() =>
        {
            isFollowing = true;
        });
    }

    private void Update()
    {
        if (isFollowing)
            transform.position = target.position;
    }

    void OnTriggerReleased()
    {
        isFollowing = false;
        transform.DOScale(startScale, .3f);
        transform.DOMove(parent.position, .3f).SetEase(Ease.InOutSine);
    }
}
