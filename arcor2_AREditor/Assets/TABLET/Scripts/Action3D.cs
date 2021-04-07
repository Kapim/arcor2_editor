using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OutlineOnClick))]
public class Action3D : Base.Action {
    public Renderer Visual;

    private Color32 colorDefault = new Color32(229, 215, 68, 255);
    private Color32 colorRunnning = new Color32(255, 0, 255, 255);

    private bool selected = false;
    [SerializeField]
    protected OutlineOnClick outlineOnClick;



    private void Start() {
        GameManager.Instance.OnStopPackage += OnProjectStop;
    }

    private void OnEnable() {
        GameManager.Instance.OnSceneInteractable += OnDeselect;
    }

    private void OnDisable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnSceneInteractable -= OnDeselect;
        }
    }

    private void OnProjectStop(object sender, System.EventArgs e) {
        StopAction();
    }

    public override void RunAction() {
        Visual.material.color = colorRunnning;
        foreach (IO.Swagger.Model.ActionParameter p in Data.Parameters) {
            if (p.Type == "pose") {
                string orientationId = JsonConvert.DeserializeObject<string>(p.Value);
                ProjectManager.Instance.HighlightOrientation(orientationId, true);
            }
        }
    }

    public override void StopAction() {
        if (Visual != null) {
            Visual.material.color = colorDefault;
        }
        foreach (IO.Swagger.Model.ActionParameter p in Data.Parameters) {
            if (p.Type == "pose") {
                string orientationId = JsonConvert.DeserializeObject<string>(p.Value);
                ProjectManager.Instance.HighlightOrientation(orientationId, false);
            }
        }
    }

    public override void UpdateName(string newName) {
        base.UpdateName(newName);
        NameText.text = newName;
    }

    public override void ActionUpdateBaseData(IO.Swagger.Model.BareAction aData = null) {
        base.ActionUpdateBaseData(aData);
        NameText.text = aData.Name;
    }

    public bool CheckClick() {
        if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.SelectingAction) {
            GameManager.Instance.ObjectSelected(this);
            return false;
        }
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return false;
        }
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
            Notifications.Instance.ShowNotification("Not allowed", "Editation of action only allowed in project editor");
            return false;
        }
        return true;
    }

    public override void OnClick(Click type) {
        /*if (!CheckClick())
            return;
        if (type == Click.MOUSE_RIGHT_BUTTON || type == Click.TOUCH) {
            OpenMenu();
        }*/
        //Output.Show();
        Output.OnClick(Click.MOUSE_LEFT_BUTTON);
    }

    private void OnDeselect(object sender, EventArgs e) {
        if (selected) {
            ActionPoint.HighlightAP(false);
            selected = false;
        }
    }

    public override void OnHoverStart() {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingAction) {
            if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.InteractionDisabled) {
                if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning)
                    return;
            } else {
                return;
            }
        }
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor &&
            GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning) {
            return;
        }
        outlineOnClick.Highlight();
        NameText.gameObject.SetActive(true);
        if (Input.ConnectionExists()) {
            //Input.LineToConnection.GetComponent<LineRenderer>().startWidth = 0.0038f;
            Input.LineToConnection.GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.SelectedMat;
            Input.LineToConnection.GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.SelectedMat;
            Input.GetConnection().GetComponent<LineRenderer>().startWidth = 0.0038f;
            Input.GetConnection().GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.SelectedMat;
        }

        if (Output.ConnectionExists()) {
            Output.LineToConnection.GetComponent<LineRenderer>().startWidth = 0.0038f;
            Output.LineToConnection.GetComponent<LineRenderer>().endWidth = 0.0038f;
            Output.GetConnection().GetComponent<LineRenderer>().startWidth = 0.0038f;
            Output.LineToConnection.GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.SelectedMat;
            Output.LineToConnection.GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.SelectedMat;
            Output.GetConnection().GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.SelectedMat;
        }
    }

    public override void OnHoverEnd() {
        outlineOnClick.UnHighlight();
        NameText.gameObject.SetActive(false);
        if (Input.ConnectionExists()) {
            Input.LineToConnection.GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.DefaultMat;
            Input.LineToConnection.GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.DefaultMat;
            Input.GetConnection().GetComponent<LineRenderer>().startWidth = 0.0008f;
            Input.GetConnection().GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.DefaultMat;
        }

        if (Output.ConnectionExists()) {
            Output.LineToConnection.GetComponent<LineRenderer>().startWidth = 0.0008f;
            Output.LineToConnection.GetComponent<LineRenderer>().endWidth = 0.0008f;
            Output.GetConnection().GetComponent<LineRenderer>().startWidth = 0.0008f;
            Output.LineToConnection.GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.DefaultMat;
            Output.LineToConnection.GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.DefaultMat;
            Output.GetConnection().GetComponent<LineRenderer>().material = ConnectionManagerArcoro.Instance.DefaultMat;
        }
    }

    public override void Enable(bool enable) {
        base.Enable(enable);
        if (GameManager.Instance.GreyVsHide) {
            foreach (Renderer renderer in outlineOnClick.Renderers)
                if (enable)
                    renderer.material.color = new Color(0.9f, 0.84f, 0.27f);
                else
                    renderer.material.color = Color.gray;
        } else {
            Visual.enabled = enable;
        }

        if (enable)
            SelectorMenu.Instance.CreateSelectorItem(this);
        else
            SelectorMenu.Instance.DestroySelectorItem(this);

    }

    public override string GetName() {
        return Data.Name;
    }

    public override void OpenMenu() {
        ActionMenu.Instance.CurrentAction = this;
        MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
        selected = true;
        ActionPoint.HighlightAP(true);
    }

    public void UpdateConnections() {
        if (Input.ConnectionExists()) {
            Input.transform.position = ClosestPointOnCircle(Input.GetConnectedTo().transform.position);
        }
        if (Output.ConnectionExists()) {
            Output.transform.position = ClosestPointOnCircle(Output.GetConnectedTo().transform.position);
        }
    }

    public override bool HasMenu() {
        return true;
    }

    public override void StartManipulation() {
        throw new NotImplementedException();
    }

    public override void Remove() {
        Input.Remove();
        Output.Remove();
        WebsocketManager.Instance.RemoveAction(Data.Id, false);
    }

    public override bool Removable() {
        return true;
    }

    public async override void Rename(string newName) {
        try {
            await WebsocketManager.Instance.RenameAction(GetId(), newName);
            Notifications.Instance.ShowToastMessage("Action renamed");
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename action", e.Message);
            throw;
        }
    }
}
