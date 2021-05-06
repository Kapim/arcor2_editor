using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class RightButtonsMenu : Singleton<RightButtonsMenu>
{
    public ButtonWithTooltip SelectBtn, CollapseBtn, MenuTriggerBtn, AddActionBtn;
    public Sprite CollapseIcon, UncollapseIcon;

    private InteractiveObject selectedObject;
    private ButtonInteractiveObject selectedButton;

    public GameObject ActionPicker;


    private void Awake() {
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;
        Sight.Instance.OnObjectSelectedChangedEvent += OnButtonObjectSelectedChangedEvent;
    }

    private void OnButtonObjectSelectedChangedEvent(object sender, ButtonInteractiveObjectEventArgs args) {
        selectedButton = args.InteractiveObject;
        MenuTriggerBtn.SetInteractivity(selectedButton != null, "No item selected");
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

    public void TriggerClick() {
        selectedButton.OnClick(Clickable.Click.MOUSE_LEFT_BUTTON);
    }

    public void AddAction() {
        Debug.LogError(selectedObject);
        if (selectedObject != null && (selectedObject is Action3D || selectedObject is ActionPoint3D)) {

            ActionPicker.transform.position = selectedObject.transform.position - Camera.main.transform.forward * 0.05f;
            ActionPicker.SetActive(true);
            if (selectedObject is Action3D action) {
                action.ActionPoint.SetApCollapsed(false);
            } else if (selectedObject is ActionPoint3D actionPoint) {
                actionPoint.SetApCollapsed(false);             
            } else {
                return;
            }
                
            
        } else {
            string name = ProjectManager.Instance.GetFreeAPName("ap");
            LeftMenu.Instance.ApToAddActionName = name;
            LeftMenu.Instance.ActionCb = SelectAP;
            if (selectedObject != null && selectedObject is ConnectionLine connectionLine) {
                if (ProjectManager.Instance.LogicItems.TryGetValue(connectionLine.LogicItemId, out LogicItem logicItem)) {
                    logicItem.Input.Action.ActionPoint.SetApCollapsed(false);
                    logicItem.Output.Action.ActionPoint.SetApCollapsed(false);
                    ProjectManager.Instance.PrevAction = logicItem.Output.Action.GetId();
                    ProjectManager.Instance.NextAction = logicItem.Input.Action.GetId();
                    connectionLine.Remove();
                }
            }            
            GameManager.Instance.AddActionPointExperiment(name, false);
        } 
        SelectorMenu.Instance.Active = false;
        SetMenuTriggerMode();
    }

    public void SelectAP(ActionPoint3D actionPoint) {

        actionPoint.SetApCollapsed(false);
        //SelectorMenu.Instance.SetSelectedObject(actionPoint, true);
        LeftMenu.Instance.APToRemoveOnCancel = actionPoint;
        ActionPicker.transform.position = actionPoint.transform.position - Camera.main.transform.forward * 0.05f;
        ActionPicker.SetActive(true);
    }

    public void SetMenuTriggerMode() {
        MenuTriggerBtn.gameObject.SetActive(true);
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        AddActionBtn.gameObject.SetActive(false);
    }

    public void SetSelectorMode() {
        SelectBtn.gameObject.SetActive(true);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
    }

    public void SetActionMode() {
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(true);
    }


}
