using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConfirmationDialog : UniversalDialog {

    public override void Open(string title, string description, UnityAction confirmationCallback, UnityAction cancelCallback, string confirmLabel = "Confirm", string cancelLabel = "Cancel") {

        base.Open(title, description, confirmationCallback, cancelCallback, confirmLabel, cancelLabel);
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.2f;
        RightButtonsMenu.Instance.SetMenuTriggerMode();
    }


}
