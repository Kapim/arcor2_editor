using System.Collections;
using System.Collections.Generic;
using Base;
using RosSharp.Urdf;
using UnityEngine;

public class RobotEE : InteractiveObject {
    
    [SerializeField]
    private TMPro.TMP_Text eeName;
    public IO.Swagger.Model.Position Position;
    public IO.Swagger.Model.Orientation Orientation;

    public string RobotId, EEId;
    

    public void InitEE(IRobot robot, string eeId) {
        RobotId = robot.GetId();
        EEId = eeId;
        SetLabel(robot.GetName(), eeId);
    }

    public void SetLabel(string robotName, string eeName) {
        this.eeName.text = robotName + "/" + eeName;
    }

    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
    }

    public override void OnHoverStart() {
        eeName.gameObject.SetActive(true);
    }

    public override void OnHoverEnd() {
        eeName.gameObject.SetActive(false);
    }

    /// <summary>
    /// Takes world space pose of end effector, converts them to SceneOrigin frame and apply to RobotEE
    /// </summary>
    /// <param name="position">Position in world frame</param>
    /// <param name="orientation">Orientation in world frame</param>
    public void UpdatePosition(IO.Swagger.Model.Position position, IO.Swagger.Model.Orientation orientation) {
        Position = position;
        Orientation = orientation;
        transform.position = SceneManager.Instance.SceneOrigin.transform.TransformPoint(TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(position)));
        // rotation set according to this
        // https://answers.unity.com/questions/275565/what-is-the-rotation-equivalent-of-inversetransfor.html
        //transform.rotation = SceneManager.Instance.SceneOrigin.transform.rotation * TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(orientation));
    }

    public override string GetName() {
        return EEId;
    }

    public override string GetId() {
        return EEId;
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public override bool Movable() {
        return false;
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }

    public override void Remove() {
        throw new System.NotImplementedException();
    }

    public override bool Removable() {
        return false;
    }

    public override void Rename(string newName) {
        throw new System.NotImplementedException();
    }
}
