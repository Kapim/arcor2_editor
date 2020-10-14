using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HeadUpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {

    public UnityEvent OnPointerDownEvent;
    public UnityEvent OnPointerUpEvent;
    public UnityEvent OnPointerEnterEvent;
    public UnityEvent OnPointerExitEvent;


    public bool Holding;
    private int debugCounter = 1, debugCounter2 = 1;


    public void OnPointerDown(PointerEventData eventData) {
        Holding = true;
        OnPointerDownEvent?.Invoke();        
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OnPointerEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (++debugCounter % 2 == 0)
            return;
        Holding = false;
        OnPointerExitEvent?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (++debugCounter2 % 2 == 0)
            return;
        Holding = false;
        OnPointerUpEvent?.Invoke();
    }
}
