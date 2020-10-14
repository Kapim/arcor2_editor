using System;
using Base;
using Unity;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class HeadUpMenu : MonoBehaviour
{


    protected CanvasGroup canvasGroup;

    public virtual void ShowMenu() {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }
    public virtual void HideMenu() {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    protected virtual void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }
}
