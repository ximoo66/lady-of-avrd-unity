// Authors: Omid Ameri, Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-16 by Omid Ameri
// Last Modified: 2026-06-27 by Noah Wendt

// Course: P6

using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class OrbMenuFollower : MonoBehaviour
{
    private Transform _mainCam;
    private Tween _tweener;
    public float tweenDuration = 1f;
    public Ease tweenEase = Ease.InOutSine;
    public float maxDistanceToMainCam = 1.2f;
    public Vector3 offsetToMainCam = new Vector3(0.0f, 0.5f, 0.5f);

    private void Start()
    {
        _mainCam = Camera.main.transform;
        if (_mainCam == null)
            Debug.LogError("OrbMenuFollower: Main camera not set");
    }

    private void Update()
    {
        var d = Vector3.Distance(this.transform.position,  _mainCam.position);
        if (d > maxDistanceToMainCam && _tweener == null)
        {
            TweenToStartOffset();
        }
    }

    [Button]
    private void TweenToStartOffset()
    {
        var endPos = _mainCam.transform.position + _mainCam.transform.forward * offsetToMainCam.z;
        endPos.y = _mainCam.position.y + offsetToMainCam.y;
        _tweener =  this.transform
            .DOMove(endPos, tweenDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _tweener = null;
            });
        transform.LookAt(_mainCam.transform);
        
        var euler = Quaternion.LookRotation(_mainCam.position - transform.position).eulerAngles;
        euler.x = 0;
        euler.z = 0;
        
        transform
            .DOLocalRotate(euler, tweenDuration)
            .SetAutoKill();
    }
}