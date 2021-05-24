using System;
using UnityEngine;
using UnityEngine.Events;

public class GizmoArrow : Base.Clickable {

    public UnityEvent OnHoverCallback;
    public OutlineOnClick OutlineOnClick;
    public string ID;

    public override void OnClick(Click type) {
    }

    public override void OnHoverEnd() {
        if (OutlineOnClick != null) {
            OutlineOnClick.UnHighlight();
        }
    }

    public override void OnHoverStart() {
        OnHoverCallback.Invoke();
        if (OutlineOnClick != null) {
            OutlineOnClick.Highlight();
        }
    }



}
