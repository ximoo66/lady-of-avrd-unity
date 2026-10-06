// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-28 by Noah Wendt
// Last Modified: 2026-05-28 by Noah Wendt

using UnityEngine;
using UnityEngine.Playables;

public class StartRestartTimeline : MonoBehaviour
{
    public PlayableDirector director;
    
    public Animator animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        director =  gameObject.GetComponent<PlayableDirector>();
    }
    
    
    [ContextMenu("start restart")]
    public void Restart()
    {
        director.Stop();
        director.time = 0;
        director.Evaluate(); 
        director.Play();
        animator.SetTrigger("PointingFront");
        animator.SetTrigger("ShowAround");
    }
}
