using Base;
using RuntimeGizmos;
using UnityEngine;
using System.Collections.Generic;
using IO.Swagger.Model;
using TMPro;
using System;
using System.Threading.Tasks;
using System.Linq;

[RequireComponent(typeof(OutlineOnClick))]
[RequireComponent(typeof(Target))]
public class ActionPoint3D : Base.ActionPoint {

    public GameObject Sphere, Visual, CollapsedPucksVisual, Lock;
    public TextMeshPro ActionPointName;
    private Material sphereMaterial;
    [SerializeField]
    private OutlineOnClick outlineOnClick;
    public GameObject ActionsVisuals;


    private void Awake() {
    }



    private async void UpdatePose() {
        try {
            await WebsocketManager.Instance.UpdateActionPointPosition(Data.Id, Data.Position);
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to update action point position", e.Message);
            ResetPosition();
        }
    }

    private void LateUpdate() {
        // Fix of AP rotations - works on both PC and tablet
        //transform.rotation = Base.SceneManager.Instance.SceneOrigin.transform.rotation;
        ActionsVisuals.transform.rotation = Base.SceneManager.Instance.SceneOrigin.transform.rotation;
        //Visual.transform.rotation = Base.SceneManager.Instance.SceneOrigin.transform.rotation;
        if (Parent != null)
            orientations.transform.rotation = Parent.GetTransform().rotation;
        else
            orientations.transform.rotation = Base.SceneManager.Instance.SceneOrigin.transform.rotation;
    }


    public async void ShowMenu(bool enableBackButton = false) {
        throw new NotImplementedException();
    }


    public override Vector3 GetScenePosition() {
        return TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(Data.Position));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position">Global position of AP</param>
    public override void SetScenePosition(Vector3 position) {
        Data.Position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position));
    }

    public override Quaternion GetSceneOrientation() {
        //return TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(Data.Orientations[0].Orientation));
        return GetRotation();
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        //Data.Orientations.Add(new IO.Swagger.Model.NamedOrientation(id: "default", orientation:DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(orientation))));
    }

    public override void UpdatePositionsOfPucks() {
        CollapsedPucksVisual.SetActive(ProjectManager.Instance.AllowEdit && ActionsCollapsed);
        if (ProjectManager.Instance.AllowEdit && ActionsCollapsed) {
            foreach (Action3D action in Actions.Values) {
                action.transform.localPosition = new Vector3(0, 0, 0);
                action.transform.localScale = new Vector3(0, 0, 0);
                action.Enable(false);
                action.UpdateConnections();
            }
            
        } else {
            int i = 1;
            foreach (Action3D action in Actions.Values) {
                action.transform.localPosition = new Vector3(0, i * 0.015f, 0);
                ++i;
                action.transform.localScale = new Vector3(1, 1, 1);
                action.Enable(true);
                action.UpdateConnections();
            }
        }        
    }
    
    public override bool ProjectInteractable() {
        return base.ProjectInteractable() && GameManager.Instance.SceneInteractable;
    }

    public override void ActivateForGizmo(string layer) {
        base.ActivateForGizmo(layer);
        Sphere.layer = LayerMask.NameToLayer(layer);
    }

    /// <summary>
    /// Changes size of shpere representing action point
    /// </summary>
    /// <param name="size"><0; 1> - 0 means invisble, 1 means 10cm in diameter</param>
    public override void SetSize(float size) {
        Visual.transform.localScale = new Vector3(size / 10, size / 10, size / 10);
    }

    public override (List<string>, Dictionary<string, string>) UpdateActionPoint(IO.Swagger.Model.ActionPoint projectActionPoint) {
        (List<string>, Dictionary<string, string>) result = base.UpdateActionPoint(projectActionPoint);
        ActionPointName.text = projectActionPoint.Name;
        return result;
    }

    public override void UpdateOrientation(NamedOrientation orientation) {
        base.UpdateOrientation(orientation);
    }

    public override void AddOrientation(NamedOrientation orientation) {
        base.AddOrientation(orientation);
    }

    public override void HighlightAP(bool highlight) {
        if (highlight) {
            outlineOnClick.Highlight();
        } else {
            outlineOnClick.UnHighlight();
        }
    }

    public override void OnHoverStart() {
        if (!enabled)
            return;
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionPoint &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionPointParent) {
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
        
        HighlightAP(true);
        ActionPointName.gameObject.SetActive(true);
    }

    public override void OnHoverEnd() {
        HighlightAP(false);
        ActionPointName.gameObject.SetActive(false);
        Lock.SetActive(false);
    }


    public override void ActionPointBaseUpdate(IO.Swagger.Model.BareActionPoint apData) {
        base.ActionPointBaseUpdate(apData);
        ActionPointName.text = apData.Name;
    }

    public override void InitAP(IO.Swagger.Model.ActionPoint apData, float size, IActionPointParent parent = null) {
        base.InitAP(apData, size, parent);
        sphereMaterial = Sphere.GetComponent<Renderer>().material;
        ActionPointName.text = apData.Name;

        
    }

    public override void UpdateColor() {
        if (Enabled && !(IsLocked && !IsLockedByMe))
            sphereMaterial.color = new Color(0.51f, 0.51f, 0.89f);
        else
            sphereMaterial.color = Color.gray;
    }

    public override async void OpenMenu() {
        throw new NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public async override void StartManipulation() {
        throw new NotImplementedException();
    }

    internal GameObject GetModelCopy() {
        GameObject sphere = Instantiate(Sphere);
        Destroy(sphere.GetComponent<SphereCollider>());
        sphere.transform.localScale = Visual.transform.localScale;
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localRotation = Quaternion.identity;
        return sphere;
    }

    public async override Task<RequestResult> Removable() {
        return new RequestResult(true);
    }

    public async override void Remove() {
        RemoveAsync();
    }

    public override async Task RemoveAsync() {
        Debug.LogError($"request to remove ap: {GetName()}");
        try {
            List<Base.Action> actions = Actions.Values.ToList();
            for (int i = Actions.Count - 1; i >= 0; --i) {
                if (actions[i] is Action3D action3D)
                    await action3D.RemoveAsync();
            }
            foreach (Base.ActionPoint ap in ProjectManager.Instance.ActionPoints.Values) {
                if (ap.Parent != null && ap.Parent.GetId() == GetId()) {
                    await ap.RemoveAsync();
                }
            }
            Debug.LogError($"going to remove ap: {GetName()}");
            await WebsocketManager.Instance.RemoveActionPoint(GetId(), false) ;
        } catch (RequestFailedException ex) {
            Debug.LogError("remove fucked up");
            Notifications.Instance.ShowNotification("Failed to remove AP " + GetName(), ex.Message);
        }
    }

    public async override Task Rename(string name) {
        try {
            await WebsocketManager.Instance.RenameActionPoint(GetId(), name);
            Notifications.Instance.ShowToastMessage("Action point renamed");
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename action point", e.Message);
        }
    }

    public override string GetObjectTypeName() {
        return "Action point";
    }

    public override void OnObjectLocked(string owner) {
        base.OnObjectLocked(owner);
        if (owner != LandingScreen.Instance.GetUsername())
            ActionPointName.text = GetLockedText();
    }

    public override void OnObjectUnlocked() {
        base.OnObjectUnlocked();
        ActionPointName.text = GetName();
    }

    public override void OnClick(Click type) {
        throw new NotImplementedException();
    }

    public override void CloseMenu() {
        throw new NotImplementedException();
    }

    public override void EnableVisual(bool enable) {
        Visual.SetActive(enable);
    }

    /// <summary>
    /// Sets rotation of AP in unity coord system
    /// </summary>
    /// <param name="quaternion"></param>
    public void SetRotation(Quaternion quaternion) {
        PlayerPrefsHelper.SaveQuaternion(GetId(), quaternion);
        transform.localRotation = quaternion;
    }

    /// <summary>
    /// Gets rotation of AP in unity coord system
    /// </summary>
    /// <param name="quaternion"></param>
    public Quaternion GetRotation() {
        return PlayerPrefsHelper.LoadQuaternion(GetId(), Quaternion.identity);
    }
}
