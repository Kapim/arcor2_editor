using System;
using UnityEngine;
using UnityEngine.Events;

public class GizmoArrow : Base.Clickable {

    public UnityEvent OnHoverCallback;

    public override void OnClick(Click type) {
    }

    public override void OnHoverEnd() {
    }

    public override void OnHoverStart() {
        OnHoverCallback.Invoke();
    }


}
