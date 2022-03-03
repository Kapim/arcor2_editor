
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using RosSharp.Urdf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RightButtonsMenu : Singleton<RightButtonsMenu> {
    public ButtonWithTooltip SelectBtn, CollapseBtn, MenuTriggerBtn, AddActionBtn, RemoveBtn, MoveBtn, ExecuteBtn, ConnectionBtn;
    public Sprite CollapseIcon, UncollapseIcon;

    public Image RobotHandIcon;

    private InteractiveObject selectedObject;
    private ButtonInteractiveObject selectedButton;

    public ActionPickerMenu3D ActionPicker;

    public bool Connecting;
    private RobotEE robotEE;

    private void Awake() {
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;
        Sight.Instance.OnObjectSelectedChangedEvent += OnButtonObjectSelectedChangedEvent;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
        Connecting = false;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (args.Event.State == SceneStateData.StateEnum.Started) {
            if (SelectorMenu.Instance.lastSelectedItem != null &&
                (SelectorMenu.Instance.lastSelectedItem.InteractiveObject is RobotActionObject ||
                SelectorMenu.Instance.lastSelectedItem.InteractiveObject is RobotEE)) {
                RobotHandIcon.color = Color.white;
            }
        } else {
            RobotHandIcon.color = new Color(1, 1, 1, 0.4f);
        }
    }

    private void OnButtonObjectSelectedChangedEvent(object sender, ButtonInteractiveObjectEventArgs args) {
        selectedButton = args.InteractiveObject;
        MenuTriggerBtn.SetInteractivity(selectedButton != null, "Žádná položka není vybraná");
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
                    CollapseBtn.SetDescription("Zobrazit akce");
                } else {
                    CollapseBtn.GetComponent<IconButton>().Icon.sprite = CollapseIcon;
                    CollapseBtn.SetDescription("Schovat akce");
                }
            } else {
                CollapseBtn.SetInteractivity(false, "Vybraný objekt není akční bod nebo akce");
            }
            Task<RequestResult> tMove = Task.Run(() => selectedObject.Movable());
            Task<RequestResult> tRemove = Task.Run(() => selectedObject.Removable());
            UpdateMoveAndRemoveBtns(selectedObject.GetId(), tMove, tRemove);
            
            ExecuteBtn.SetInteractivity(selectedObject.GetType() == typeof(StartAction) || selectedObject.GetType() == typeof(Action3D) || selectedObject.GetType() == typeof(ActionPoint3D));
            AddActionBtn.SetInteractivity(selectedObject.GetType() == typeof(Action3D) ||
                selectedObject.GetType() == typeof(ActionPoint3D) ||
                selectedObject.GetType() == typeof(ConnectionLine) ||
                selectedObject is RobotEE || selectedObject is RobotActionObject);
            if (SceneManager.Instance.SceneStarted && (selectedObject is RobotEE || selectedObject is RobotActionObject)) {
                RobotHandIcon.color = Color.white;
            } else {
                RobotHandIcon.color = new Color(1, 1, 1, 0.4f);
            }
        } else {
            CollapseBtn.SetInteractivity(false, "Žádný vybraný objekt");
            SelectBtn.SetInteractivity(false, "Žádný vybraný objekt");
            RemoveBtn.SetInteractivity(false, "Žádný vybraný objekt");
            MoveBtn.SetInteractivity(false, "Žádný vybraný objekt");
            ExecuteBtn.SetInteractivity(false, "Žádný vybraný objekt");
            AddActionBtn.SetInteractivity(true);
            RobotHandIcon.color = new Color(1, 1, 1, 0.4f);
        }

    }

    private async void UpdateMoveAndRemoveBtns(string objId, Task<RequestResult> movable, Task<RequestResult> removable) {
        RequestResult move = await movable;
        RequestResult remove = await removable;

        if (selectedObject != null && objId != selectedObject.GetId()) // selected object was updated in the meantime
            return;
        MoveBtn.SetInteractivity(move.Success, $"Manipulovat s objektem\n({move.Message})");
        RemoveBtn.SetInteractivity(remove.Success, $"Odstranit objekt\n({remove.Message})");
        if (selectedObject is Action3D action) {
            MoveBtn.SetInteractivity(true);
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
            try {
                var actionPoint = ProjectManager.Instance.GetActionPoint(apId);
                actionPoint.SetApCollapsed(!actionPoint.ActionsCollapsed);
                OnObjectSelectedChangedEvent(this, new InteractiveObjectEventArgs(selectedObject));
            } catch (KeyNotFoundException) {

            }
            /*
            if (SelectorMenu.Instance.SelectorItems.TryGetValue(apId, out SelectorItem selectorItem)) {
                selectorItem.CollapseBtnCb();
                OnObjectSelectedChangedEvent(this, new InteractiveObjectEventArgs(selectedObject));
            }*/
        }
    }

    public void TriggerClick() {
        if (selectedButton != null)
            selectedButton.OnClick(Clickable.Click.MOUSE_LEFT_BUTTON);
    }

    public async void AddAction() {
        if (selectedObject != null && (selectedObject is Action3D || selectedObject is ActionPoint3D)) {

            ActionPicker.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
            ActionPicker.GetComponent<FaceCamera>().Update();
            ActionPicker.Show();
            if (selectedObject is Action3D action) {
                action.ActionPoint.SetApCollapsed(false);
            } else if (selectedObject is ActionPoint3D actionPoint) {
                actionPoint.SetApCollapsed(false);
            } else {
                return;
            }


        } else {
            string name = ProjectManager.Instance.GetFreeAPName("ab");
            AREditorResources.Instance.LeftMenuProject.ApToAddActionName = name;
            AREditorResources.Instance.LeftMenuProject.ActionCb = SelectAP;
            if (selectedObject != null) {
                if (selectedObject is ConnectionLine connectionLine) {
                    if (ProjectManager.Instance.LogicItems.TryGetValue(connectionLine.LogicItemId, out LogicItem logicItem)) {
                        logicItem.Input.Action.ActionPoint?.SetApCollapsed(false);
                        logicItem.Output.Action.ActionPoint?.SetApCollapsed(false);
                        ProjectManager.Instance.PrevAction = logicItem.Output.Action.GetId();
                        ProjectManager.Instance.NextAction = logicItem.Input.Action.GetId();
                        connectionLine.Remove();
                        GameManager.Instance.AddActionPointExperiment(name, false);
                    }
                } else if (selectedObject is RobotEE robotEE) {
                    GameManager.Instance.AddActionPointExperiment(name, false, robotEE);
                } else if (selectedObject is RobotActionObject robot) {
                    RobotEE ee = (await robot.GetAllEE())[0];
                    GameManager.Instance.AddActionPointExperiment(name, false, ee);
                } else {
                    GameManager.Instance.AddActionPointExperiment(name, false);
                }

            } else {
                GameManager.Instance.AddActionPointExperiment(name, false);
            }

        }
        SelectorMenu.Instance.Active = false;
        SetMenuTriggerMode();
    }

    public void SelectAP(ActionPoint3D actionPoint) {

        actionPoint.SetApCollapsed(false);
        //SelectorMenu.Instance.SetSelectedObject(actionPoint, true);
        AREditorResources.Instance.LeftMenuProject.APToRemoveOnCancel = actionPoint;
        ActionPicker.transform.position = actionPoint.transform.position - Camera.main.transform.forward * 0.05f;
        ActionPicker.Show();
    }

    public void SetMenuTriggerMode() {
        MenuTriggerBtn.gameObject.SetActive(true);
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
        ConnectionBtn.gameObject.SetActive(false);
    }

    public void SetSelectorMode() {
        SelectBtn.gameObject.SetActive(true);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
        ConnectionBtn.gameObject.SetActive(false);
    }

    public void SetActionMode() {
        SelectorMenu.Instance.DeselectObject();
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(true);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
        ConnectionBtn.gameObject.SetActive(false);
    }

    public void SetMoveMode() {
        SelectorMenu.Instance.DeselectObject();
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(true);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
        ConnectionBtn.gameObject.SetActive(false);
    }

    public void SetRemoveMode() {
        SelectorMenu.Instance.DeselectObject();
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(true);
        ExecuteBtn.gameObject.SetActive(false);
        ConnectionBtn.gameObject.SetActive(false);
    }

    public void SetRunMode() {
        SelectorMenu.Instance.DeselectObject();
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(true);
        ConnectionBtn.gameObject.SetActive(false);

    }

    public void SetConnectionsMode() {
        SelectorMenu.Instance.DeselectObject();
        SelectBtn.gameObject.SetActive(false);
        CollapseBtn.gameObject.SetActive(true);
        MenuTriggerBtn.gameObject.SetActive(false);
        AddActionBtn.gameObject.SetActive(false);
        MoveBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        ExecuteBtn.gameObject.SetActive(false);
        ConnectionBtn.gameObject.SetActive(true);
    }

    public void MoveClick() {

        if (selectedObject is null)
            return;

        
        //SelectorMenu.Instance.Active = false;
        gameObject.SetActive(false);

        InteractiveObject obj = selectedObject;
        if (selectedObject is Action3D action)
            obj = action.ActionPoint;
        //TransformMenu.Instance.Show(obj, selectedObject.GetType() == typeof(DummyAimBox) || selectedObject.GetType() == typeof(DummyAimBoxTester), selectedObject.GetType() == typeof(DummyAimBoxTester));
        _ = TransformMenu.Instance.Show(obj);
    }

    public void RemoveClick() {
        if (selectedObject is null)
            return;

        SelectorMenu.Instance.Active = false;
        //gameObject.SetActive(false);
        AREditorResources.Instance.LeftMenuProject.ConfirmationDialog.Open("Odstranit objekt",
                         "Jste si jistí že chce odstranit " + selectedObject.GetName() + "?",
                         () => {
                             selectedObject.Remove();
                             SetRemoveMode();
                             SelectorMenu.Instance.Active = true;
                             AREditorResources.Instance.LeftMenuProject.ConfirmationDialog.Close();
                         },
                         () => {
                             SetRemoveMode();
                             SelectorMenu.Instance.Active = true;
                         }, "Potvrdit", "Zrušit");
    }

    public async void RunClick() {
        if (selectedObject is null)
            return;

        if (selectedObject.GetType() == typeof(StartAction)) {
            GameManager.Instance.ShowLoadingScreen("Spouštím program...", true);
            try {
                await WebsocketManager.Instance.SaveProjectSync(false);
            } catch (RequestFailedException) {
                //pass, already saved probably
            }
            try {                
                await Base.WebsocketManager.Instance.TemporaryPackage();
            }
            catch (RequestFailedException ex) {
                Base.Notifications.Instance.ShowNotification("Nepodařilo se spustit program", "");
                Debug.LogError(ex);
                GameManager.Instance.HideLoadingScreen(true);
            }
            
        } else if (selectedObject.GetType() == typeof(Action3D)) {
            try {
                await WebsocketManager.Instance.ExecuteAction(selectedObject.GetId(), false);
            } catch (RequestFailedException) {
                Notifications.Instance.ShowNotification("Nepodařilo se vykonat akci", "");
                return;
            }
        } else if (selectedObject.GetType() == typeof(ActionPoint3D)) {
            string robotId = "";
            foreach (IRobot r in SceneManager.Instance.GetRobots()) {
                robotId = r.GetId();
            }
            NamedOrientation o = ((ActionPoint3D) selectedObject).GetFirstOrientation();
            IRobot robot = SceneManager.Instance.GetRobot(robotId);
            try {
                await WebsocketManager.Instance.MoveToActionPointOrientation(robot.GetId(), (await robot.GetEndEffectorIds())[0], 0.5m, o.Id, false);
            } catch (RequestFailedException) {
                Notifications.Instance.ShowNotification("Nepodařilo se přesunout robota", "Nedosažitelné místo");
                return;
            }

        }
    }

    public async void ConnectionClick() {
        if (selectedObject != null && selectedObject is Base.Action action) {

            if (Connecting) {
                Base.Action from = ConnectionManagerArcoro.Instance.GetActionConnectedToPointer();
                if (action.GetId() == from.GetId()) {
                    Notifications.Instance.ShowNotification("Nepodařilo se vytvořit propoj", "Tlačítko je potřeba držet a přetáhnout zaměřovač na navazující akci");
                    ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                    Connecting = false;
                    return;
                }
                if (action.Input.AnyConnection()) {
                    Notifications.Instance.ShowNotification("Nepodařilo se vytvořit propoj", "Propoj k této akci již existuje");
                    ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                    Connecting = false;
                    return;
                }
                
                try {
                    await WebsocketManager.Instance.AddLogicItem(from.GetId(), action.GetId(), from.GetProjectLogicIf(), false);                
                } catch (RequestFailedException ex) {
                    Notifications.Instance.ShowNotification("Nepodařilo se vytvořit propoj", ex.Message);
                    return;
                } finally {
                    ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                    Connecting = false;
                }
            } else {
                if (action.Output.AnyConnection()) {
                    Notifications.Instance.ShowNotification("Nepodařilo se vytvořit propoj", "Propoj k této akci již existuje");
                    return;
                }
                Connecting = true;
                ConnectionManagerArcoro.Instance.CreateConnectionToPointer(action.Output.gameObject);

            }
        } else {
            if (Connecting) {
                Connecting = false;
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
            }
        }
    }

    public void ResetConnectionMode() {
        if (Connecting) {
            ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
            Connecting = false;
        }
    }

    public async void RobotHandTeachingPush() {
        await SceneManager.Instance.SelectRobotAndEE();
        robotEE = SceneManager.Instance.SelectedEndEffector;
        await robotEE.Robot.WriteLock(false);
        _ = WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: true);
    }

    public async void RobotHandTeachingRelease() {
        if (!SceneManager.Instance.SceneStarted)
            return;
        await SceneManager.Instance.SelectRobotAndEE();

        await WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: false);
        await robotEE.Robot.WriteUnlock();
        IO.Swagger.Model.Position position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(robotEE.transform.position)));

        await WebsocketManager.Instance.MoveToPose(SceneManager.Instance.SelectedRobot.GetId(), "default", 1, position, DataHelper.QuaternionToOrientation(new Quaternion(0, 0, 1, 0)));
    }

    public void AddConnectionPush() {
        ConnectionClick();
    }

    public void AddConnectionRelease() {
        ConnectionClick();
    }



}
