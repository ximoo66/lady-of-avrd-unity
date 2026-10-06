// Authors: Linus Ziesel, Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-07-18 by Noah Wendt

using UnityEngine;

/// <summary>
/// Provides a collection of animatable gestures and expressions triggered by the AI through tool calls.
/// </summary>
public class ToolCalls : MonoBehaviour
{
    public Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        AIManagerMain.OnStartListening += StartListening;
        AIManagerMain.OnStopListening += StopListening;
    }

    private void StartListening()
    {
        animator.SetBool("isListening", true);
    }

    private void StopListening()
    {
        animator.SetBool("isListening", false);
    }

    [Tool("Nod your head to show that you are happy.")]
    public void HappyIdle()
    {
        LadyMoodSwitch.Instance.ChangeColor("yellow");
        animator.SetTrigger("HappyIdle");
        Debug.Log("Happy called");
    }

    [Tool("Make a gesture with your hand to indicate the user should hold on or go slower")]
    public void HoldOn()
    {
        animator.SetTrigger("HoldOn");
    }

    [Tool("Show that you are disappointed.")]
    public void Disappointed()
    {
        LadyMoodSwitch.Instance.ChangeColor("blue");
        animator.SetTrigger("Disappointed");
        Debug.Log("Disappointed called");
    }

    [Tool("If you tell of some spot where you are working or similar, point behind you.")]
    public void PointingBack()
    {
        animator.SetTrigger("PointingBack");
        Debug.Log("PointingBack called");
    }
    [Tool("If you tell of some spot where you are working or similar, point behind you.")]
    public void PointingFront()
    {
        animator.SetTrigger("PointingFront");
        Debug.Log("PointingBack called");
    }
    [Tool("Make a happy gesture if you like what you are talking about.")]
    public void HappyGesture()
    {
        LadyMoodSwitch.Instance.ChangeColor("orange");
        animator.SetTrigger("HappyGesture");
        Debug.Log("HappyGesture called");
    }

    private string[] _angryTirggers = { "Angry1", "Angry2" };
    [Tool("Show that you are angry.")]
    public void AngryGesture()
    {
        LadyMoodSwitch.Instance.ChangeColor("red");
        animator.SetTrigger(_angryTirggers[Random.Range(0, _angryTirggers.Length)]);
        Debug.Log("Angry called");
    }

    private string[] _noddingTriggers = {"Nodding1", "Nodding2" };
    [Tool("Just nod if something you say needs a little expression.")]
    public void Nodding()
    {
        LadyMoodSwitch.Instance.ChangeColor("green");
        animator.SetTrigger(_showAroundTriggers[Random.Range(0, _showAroundTriggers.Length)]);
        Debug.Log("Nodding called");
    }

    private string[] _shakingHeadTriggers = {"ShakingHead1", "ShakingHead2"};
    [Tool("Show that you are disappointed.")]
    public void ShakingHead()
    {
        LadyMoodSwitch.Instance.ChangeColor("purple");
        animator.SetTrigger(_shakingHeadTriggers[Random.Range(0, _shakingHeadTriggers.Length)]);
        Debug.Log("ShakingHead called");
    }

    [Tool("Show that you are disappointed.")]
    public void LookAway()
    {
        LadyMoodSwitch.Instance.ChangeColor("blue");
        animator.SetTrigger("LookAway");
        Debug.Log("LookAway called");
    }

    private string[] _showAroundTriggers = {"ShowAround1", "ShowAround2" };
    [Tool("show around if you are explaining a larger context")]
    public void ShowAround()
    {
        LadyMoodSwitch.Instance.ChangeColor("green");
        animator.SetTrigger(_showAroundTriggers[Random.Range(0, _showAroundTriggers.Length)]);
    }
    
    [Tool("waving")]
    public void Waving()
    {
        LadyMoodSwitch.Instance.ChangeColor("purple");
        animator.SetTrigger("Waving");
    }
    
    [Tool("show that you are unsure")]
    public void ShiftingWeight()
    {
        LadyMoodSwitch.Instance.ChangeColor("pink");
        animator.SetTrigger("ShiftWeight");
    }

    

}