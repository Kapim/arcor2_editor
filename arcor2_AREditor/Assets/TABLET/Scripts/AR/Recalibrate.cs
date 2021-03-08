using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class Recalibrate : InteractiveObject
    {


    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        Calibrate();
    }

    public void Calibrate() {
        CalibrationManager.Instance.Recalibrate();
    }

    private void OnEnable() {
        Enabled = true;
        SelectorMenu.Instance.ForceUpdateMenus();
    }

    private void OnDisable() {
        Enabled = false;
        SelectorMenu.Instance.ForceUpdateMenus();
    }

    public override void Enable(bool enable) {
        base.Enable(enable);
        SelectorMenu.Instance.ForceUpdateMenus();
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }

    public override string GetName() {
        return "Calibration cube";
    }

    public override string GetId() {
        return "ReCalibration cube";
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public override bool Movable() {
        return false;
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }

    public override void Remove() {
        throw new System.NotImplementedException();
    }

    public override bool Removable() {
        return false;
    }

    public override void Rename(string newName) {
        throw new System.NotImplementedException();
    }
}
