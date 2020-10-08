using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HeadUpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    public UnityEvent OnPointerDownEvent;

    
    public void OnPointerDown(PointerEventData eventData) {
        Debug.LogError("OnPointerDown");
    }

    public void OnPointerUp(PointerEventData eventData) {
        Debug.LogError("OnPointerUp");
    }
}
