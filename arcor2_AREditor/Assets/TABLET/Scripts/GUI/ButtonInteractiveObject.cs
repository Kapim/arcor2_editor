using System;
using UnityEngine;
using UnityEngine.Events;

public class ButtonInteractiveObject : InteractiveObject {

    public string Name;
    public UnityEvent Callback;
    public GameObject Border;
    public override string GetId() {
        return Name;
    }

    public override string GetName() {
        return Name;
    }

    public override bool HasMenu() {
        return false;
    }

    public override bool Movable() {
        return false;
    }

    public override void OnClick(Click type) {
        Callback.Invoke();
    }

    public override void OnHoverEnd() {

        Border.SetActive(false);
    }

    public override void OnHoverStart() {
        Border.SetActive(true);
    }

    public override void OpenMenu() {
        throw new NotImplementedException();
    }

    public override bool Removable() {
        return false;
    }

    public override void Remove() {
        throw new NotImplementedException();
    }

    public override void Rename(string newName) {
        throw new NotImplementedException();
    }

    public override void StartManipulation() {
        throw new NotImplementedException();
    }
}
