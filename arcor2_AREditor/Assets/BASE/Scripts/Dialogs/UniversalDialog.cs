using System;
using UnityEngine;
using UnityEngine.Events;

public class UniversalDialog : Dialog
{
    [SerializeField]
    private TMPro.TMP_Text OKButtonLabelNormal, OKButtonLabelHighlighted, CancelButtonLabelNormal, CancelButtonLabelHighlighted;

    [SerializeField]
    protected ButtonWithTooltip okBtn, cancelBtn;

    protected UnityAction confirmCallback, cancelCallback;

    public bool Visible;

    public void SetConfirmLabel(string name) {
        OKButtonLabelNormal.text = name;
        OKButtonLabelHighlighted.text = name;
    }

    public void SetCancelLabel(string name) {
        CancelButtonLabelNormal.text = name;
        CancelButtonLabelHighlighted.text = name;
    }

    public void AddConfirmCallback(UnityAction callback) {
        //windowManager.onConfirm.AddListener(callback);
        confirmCallback = callback;
    }

    public void AddCancelCallback(UnityAction callback) {
        //windowManager.onCancel.AddListener(callback);
        cancelCallback = callback;
    }

    public void SetDescription(string description) {
        if (string.IsNullOrEmpty(description)) {
            windowManager.windowDescription.gameObject.SetActive(false);
        } else {
            windowManager.windowDescription.gameObject.SetActive(true);
            windowManager.windowDescription.text = description;
        }        
    }

    public void SetTitle(string title) {
        windowManager.windowTitle.text = title;
    }

    public virtual void Open(string title, string description, UnityAction confirmationCallback, UnityAction cancelCallback, string confirmLabel = "Confirm", string cancelLabel = "Cancel") {
        windowManager.onConfirm.RemoveAllListeners();
        windowManager.onCancel.RemoveAllListeners();
        SetTitle(title);
        SetDescription(description);
        AddConfirmCallback(confirmationCallback);
        AddCancelCallback(cancelCallback);
        SetConfirmLabel(confirmLabel);
        SetCancelLabel(cancelLabel);
        Open();
        Visible = true;
    }

    public override void Confirm() {
        confirmCallback?.Invoke();
        windowManager.CloseWindow();
        Visible = false;
    }

    public override void Close() {
        base.Close();
        cancelCallback?.Invoke();
        Visible = false;
    }
}
