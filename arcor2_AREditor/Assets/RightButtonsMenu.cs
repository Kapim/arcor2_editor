using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class RightButtonsMenu : Singleton<RightButtonsMenu>
{
    public ButtonWithTooltip SelectBtn, CollapseBtn;
    public Sprite CollapseIcon, UncollapseIcon;

    private InteractiveObject selectedObject;

    private void Awake() {
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;
    }

    private void OnObjectSelectedChangedEvent(object sender, InteractiveObjectEventArgs args) {
        selectedObject = args.InteractiveObject;
        if (SelectorMenu.Instance.ManuallySelected)
            SelectBtn.GetComponent<Image>().enabled = true;
        else
            SelectBtn.GetComponent<Image>().enabled = false;
        if (args.InteractiveObject != null) {
            SelectBtn.SetInteractivity(true);
            CollapseBtn.SetInteractivity(true);
            ActionPoint actionPoint;
            if (args.InteractiveObject is ActionPoint3D || args.InteractiveObject.GetType() == typeof(Action3D)) {
                if (args.InteractiveObject is Action3D action)
                    if (action.ActionPoint != null)
                        actionPoint = action.ActionPoint;
                    else {
                        return;
                    }
                else {
                    actionPoint = (ActionPoint) args.InteractiveObject;
                }
                if (actionPoint.ActionsCollapsed) {
                    CollapseBtn.GetComponent<IconButton>().Icon.sprite = UncollapseIcon;
                    CollapseBtn.SetDescription("Show actions");
                } else {
                    CollapseBtn.GetComponent<IconButton>().Icon.sprite = CollapseIcon;
                    CollapseBtn.SetDescription("Hide actions");
                }
            } else {
                CollapseBtn.SetInteractivity(false, "Selected object is not action point");
            }
        } else {
            CollapseBtn.SetInteractivity(false, "No object selected");
            SelectBtn.SetInteractivity(false, "No object selected");
        }
        
    }

    public void SelectClick() {
        if (selectedObject != null) {
            SelectorMenu.Instance.SetSelectedObject(selectedObject, true);
        }
    }

    public void CollapseClick() {
        if (selectedObject != null) {
            string apId = null;
            if (selectedObject is ActionPoint3D actionPoint3D)
                apId = actionPoint3D.GetId();
            else if (selectedObject is Action3D action && action.ActionPoint != null)
                apId = action.ActionPoint.GetId();
            else
                return;
            if (SelectorMenu.Instance.SelectorItems.TryGetValue(apId, out SelectorItem selectorItem)) {                
                selectorItem.CollapseBtnCb();
                OnObjectSelectedChangedEvent(this, new InteractiveObjectEventArgs(selectedObject));
            }
        }
    }
}
