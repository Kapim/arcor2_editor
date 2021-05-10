using System;
using System.Runtime.Remoting.Messaging;
using Base;
using UnityEngine;

public class DummyAimBoxTester : DummyAimBox, IActionPointParent {
 
    protected override void Awake() {
        base.Awake();
        //Base.ProjectManager.Instance.OnActionPointAddedToScene += OnActionPointAddedToScene;
        Name = "Tester";
    }


    protected override void OnActionPointAddedToScene(object sender, Base.ActionPointEventArgs args) {
        if (args.ActionPoint.Data.Name == "dabap2") {
            ActionPoint = args.ActionPoint;
            transform.SetParent(ActionPoint.transform);
            transform.localPosition = Vector3.zero;
            transform.rotation = GameManager.Instance.Scene.transform.rotation;
            LeftMenu.Instance.MoveClick();
            SelectorMenu.Instance.CreateSelectorItem(this);
            SelectorMenu.Instance.SetSelectedObject(this, true);
            SelectorMenu.Instance.UpdateFilters();

        }
    }

    public override async void AimFinished() {
        if (!Reseted)
            return;
        SetVisibility(true);
        bool aimed1 = PlayerPrefsHelper.LoadBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/aimed1", false);
        bool aimed2 = PlayerPrefsHelper.LoadBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/aimed2", false);
        if (!aimed1) {
            PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/aimed1", true);
            PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/aimed2", false);
            
            WebsocketManager.Instance.UpdateActionPointPosition(ActionPoint.GetId(), new IO.Swagger.Model.Position(-0.3m, 0.435m, 0));
        } else if (!aimed2) {
            PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/aimed1", false);
            PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/aimed2", true);
            WebsocketManager.Instance.UpdateActionPointPosition(ActionPoint.GetId(), new IO.Swagger.Model.Position(-0.35m, 0.435m, 0));
        }
        Reseted = false;
    }

    public override void SetVisibility(bool visible) {
        Visible = visible;
        PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/visible", visible);

    }

    public override async void Remove() {
        try {
            await WebsocketManager.Instance.RemoveActionPoint(ActionPoint.Data.Id);
            PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/visible", false);
            PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/inScene", false);
            try {
                Destroy(gameObject);
            } catch (MissingReferenceException) { }
            for (int i = 0; i < 4; ++i)
                PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + "/Tester/PointAimed/" + i, false);
            SelectorMenu.Instance.DestroySelectorItem(this);
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to remove BlueBox", e.Message);
        }
        
    }

}
