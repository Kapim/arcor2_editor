using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class MultiButton : MonoBehaviour
{
    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    private bool holdInvoked = false;


    private void Update() {
        if (TransformMenu.Instance.CanvasGroup.alpha < 1)
            return;
        //Debug.LogError($"Update: {stopwatch.Elapsed}, is running: {stopwatch.IsRunning}, hold invoked: {holdInvoked}");
        if (stopwatch.IsRunning && !holdInvoked && stopwatch.Elapsed > TimeSpan.FromSeconds(0.4)) {
            holdInvoked = true;
            BtnHold.Invoke();
        }
    }

    public UnityEvent BtnClick, BtnHold, BtnRelease;
    public void BtnPressed() {
        if (stopwatch.IsRunning)
            stopwatch.Stop();
        BtnClick.Invoke();
        holdInvoked = false;
        stopwatch.Reset();
        stopwatch.Start();
        //Debug.LogError("pressed");
    }

    public void BtnReleased() {

        //Debug.LogError($"Released: {stopwatch.Elapsed}");
        
        if (stopwatch.Elapsed > TimeSpan.FromSeconds(0.4)) {
        
            BtnRelease.Invoke();
        }
        stopwatch.Stop();
        stopwatch.Reset();
    }
}
