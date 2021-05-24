using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesBtnGroup : MonoBehaviour
{
    public CoordBtn X, Y, Z;
    private CoordBtn selectedBtn;
    public bool AllAxis = false;


    private void Awake() {
        X.Select();
        selectedBtn = X;
    }

    public void DisableAll() {
        X.Deselect();
        Y.Deselect();
        Z.Deselect();
    }

    public void Enable(CoordBtn btn) {
        DisableAll();
        selectedBtn = btn;
        TransformMenu.Instance.ResetTransformWheel();
        btn.Select();
    }

    public string GetSelectedAxis() {
        if (AllAxis)
            return "all";
        else
            return selectedBtn.Axis;
    }
}
