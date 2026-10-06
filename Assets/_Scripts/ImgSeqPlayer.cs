// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-11 by Linus Ziesel
// Last Modified: 2026-07-12 by Linus Ziesel

using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ImgSeqPlayer : MonoBehaviour
{
    [Header("Drag all frame sprites here, in order")]
    public Sprite[] frames;

    [Header("Playback")]
    public float framesPerSecond = 25f;
    public bool loop = true;
    public bool playOnAwake = true;
    public bool playOnValidate = true;

    public SpriteRenderer spriteRenderer;
    private int _currentFrame;
    private float _timer;
    private bool _isPlaying;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (playOnAwake) Play();
    }

    private void OnValidate()
    {
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (playOnValidate) Play();
    }

    private void OnDisable()
    {
        spriteRenderer.enabled = false;
    }

    void Update()
    {
        if (!_isPlaying || frames == null || frames.Length == 0) return;

        _timer += Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;

        if (_timer >= frameDuration)
        {
            _timer -= frameDuration;
            _currentFrame++;

            if (_currentFrame >= frames.Length)
            {
                if (loop)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = frames.Length - 1;
                    _isPlaying = false;
                }
            }

            spriteRenderer.sprite = frames[_currentFrame];
        }
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0) return;
        _currentFrame = 0;
        _timer = 0f;
        spriteRenderer.sprite = frames[0];
        _isPlaying = true;
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Resume()
    {
        _isPlaying = true;
    }
}
