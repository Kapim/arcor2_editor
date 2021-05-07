using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.UI;

public class RightButtonsMenu : Singleton<RightButtonsMenu>
{
    public ButtonWithTooltip SelectBtn, CollapseBtn, MenuTriggerBtn, AddActionBtn, RemoveBtn, MoveBtn, ExecuteBtn;
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
            Base.ActionPoint actionPoint;
            if (args.InteractiveObject is ActionPoint3D || args.InteractiveObject.GetType() == typeof(Action3D)) {
                if (args.InteractiveObject is Action3D action)
                    if (action.ActionPoint != null)
                        actionPoint = action.ActionPoint;
                    else {
                        return;
                    }
                else {
                    actionPoint = (Base.ActionPoint) args.InteractiveObject;
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
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
    }

    public void SetSelectorMode() {
        SelectBtn.gameObject.SetActive(true);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
    }

    public void SetActionMode() {
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(true);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
    }

    public void SetMoveMode() {
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(true);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
    }

    public void SetRemoveMode() {
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(true);
        ExecuteBtn.gameObject.SetActive(false);
    }

    public void SetRunMode() {
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(true);

    }

    public void MoveClick() {
        
        if (selectedObject is null)
            return;

        if (selectedObject.GetType() == typeof(PuckInput) ||
            selectedObject.GetType() == typeof(PuckOutput)) {
            selectedObject.StartManipulation();
            return;
        }

        SelectorMenu.Instance.Active = false;
        gameObject.SetActive(false);
        TransformMenu.Instance.Show(selectedObject, selectedObject.GetType() == typeof(DummyAimBox) || selectedObject.GetType() == typeof(DummyAimBoxTester), selectedObject.GetType() == typeof(DummyAimBoxTester));
        
    }

    public void RemoveClick() {
        if (selectedObject is null)
            return;

        SelectorMenu.Instance.Active = false;
        gameObject.SetActive(false);
        LeftMenu.Instance.ConfirmationDialog.Open("Remove object",
                         "Are you sure you want to remove " + selectedObject.GetName() + "?",
                         () => {
                             selectedObject.Remove();
                             gameObject.SetActive(true);
                             LeftMenu.Instance.ConfirmationDialog.Close();
                         },
                         () => {
                             gameObject.SetActive(true);
                             LeftMenu.Instance.ConfirmationDialog.Close();
                         });
    }

    public async void RunClick() {
        if (selectedObject is null)
            return;

        if (selectedObject.GetType() == typeof(StartAction)) {
            await GameManager.Instance.SaveProject();
            GameManager.Instance.ShowLoadingScreen("Running project", true);
            try {
                await Base.WebsocketManager.Instance.TemporaryPackage();
                MenuManager.Instance.MainMenu.Close();
            } catch (RequestFailedException ex) {
                Base.Notifications.Instance.ShowNotification("Failed to run temporary package", "");
                Debug.LogError(ex);
                GameManager.Instance.HideLoadingScreen(true);
            }
        } else if (selectedObject.GetType() == typeof(Action3D)) {
            try {
                await WebsocketManager.Instance.ExecuteAction(selectedObject.GetId(), false);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to execute action", ex.Message);
                return;
            }
        } else if (selectedObject.GetType() == typeof(ActionPoint3D)) {
            string robotId = "";
            foreach (IRobot r in SceneManager.Instance.GetRobots()) {
                robotId = r.GetId();
            }
            NamedOrientation o = ((ActionPoint3D) selectedObject).GetFirstOrientation();
            IRobot robot = SceneManager.Instance.GetRobot(robotId);
            await WebsocketManager.Instance.MoveToActionPointOrientation(robot.GetId(), (await robot.GetEndEffectorIds())[0], 0.5m, o.Id, false);
        }
    }



}
