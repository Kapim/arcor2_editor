using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;


public class APHeadUp : HeadUpMenu {
    public HeadUpButton MoveBtn, RobotAimingBtn, RemoveBtn, EditBtn, AdjustSpeedBtn, PivotBtn, PlaneSelectBtn;
    public FixedJoystick TranslateJoystick, AdjustSpeedJoystick;
    private ActionPoint currentActionPoint;

    public TMPro.TMP_Text SpeedLabel;
    public GameObject PlanePrefab;

    private GameObject plane;

    /// <summary>
    /// Speed of transition in meters per seconds
    /// </summary>
    private float speed;

    public enum MenuStateEnum {
        Closed,
        Opened,
        Translating,
        AdjustingSpeed,
        SelectingPivot,
        SelectingPlane
    }

    private MenuStateEnum menuState = MenuStateEnum.Closed;

    public float Speed {
        get => speed;
        set {
            if (speed < 0)
                speed = 0;
            else
                speed = value;
            SpeedLabel.text = speed.ToString("0.00") + " m/s";
        }
    }

    public MenuStateEnum MenuState {
        get => menuState;
        set {
            menuState = value;
            //HideEverything();
            switch (value) {
                case MenuStateEnum.Opened:
                    HideEverything();
                    MoveBtn.gameObject.SetActive(true);
                    RobotAimingBtn.gameObject.SetActive(true);
                    RemoveBtn.gameObject.SetActive(true);
                    EditBtn.gameObject.SetActive(true);
                    break;
                case MenuStateEnum.Translating:
                    RobotAimingBtn.gameObject.SetActive(false);
                    RemoveBtn.gameObject.SetActive(false);
                    EditBtn.gameObject.SetActive(false);
                    AdjustSpeedBtn.gameObject.SetActive(true);
                    PivotBtn.gameObject.SetActive(true);
                    PlaneSelectBtn.gameObject.SetActive(true);
                    TranslateJoystick.gameObject.SetActive(true);
                    AdjustSpeedJoystick.gameObject.SetActive(false);
                    break;
                case MenuStateEnum.AdjustingSpeed:
                    AdjustSpeedJoystick.gameObject.SetActive(true);
                    TranslateJoystick.gameObject.SetActive(false);
                    break;
            }
        }
    }

    private void HideEverything() {
        MoveBtn.gameObject.SetActive(false);
        RobotAimingBtn.gameObject.SetActive(false);
        RemoveBtn.gameObject.SetActive(false);
        EditBtn.gameObject.SetActive(false);
        AdjustSpeedBtn.gameObject.SetActive(false);
        PivotBtn.gameObject.SetActive(false);
        PlaneSelectBtn.gameObject.SetActive(false);
        TranslateJoystick.gameObject.SetActive(false);
        AdjustSpeedJoystick.gameObject.SetActive(false);
    }

    private void Start() {
        Speed = 0.5f; 
    }

    private void FixedUpdate() {
        if (MenuState == MenuStateEnum.Closed)
            return;
        else if (MenuState == MenuStateEnum.Translating) {

            if (TranslateJoystick.Horizontal != 0 || TranslateJoystick.Vertical != 0) {
                IO.Swagger.Model.Position position = currentActionPoint.Data.Position;
                Vector3 offset = (new Vector3(1, 0, 0) * TranslateJoystick.Horizontal * Time.fixedDeltaTime * Speed) + (new Vector3(0, 0, 1) * TranslateJoystick.Vertical * Time.fixedDeltaTime * Speed);
                currentActionPoint.transform.Translate(offset);
                ((ActionPoint3D) currentActionPoint).manipulationStarted = false;
                ((ActionPoint3D) currentActionPoint).updatePosition = true;
            } else {
                ((ActionPoint3D) currentActionPoint).manipulationStarted = true;
            }
        } else if (MenuState == MenuStateEnum.AdjustingSpeed) {
            Speed += AdjustSpeedJoystick.Vertical * Time.fixedDeltaTime * 0.5f;
        }
    }

    public void ShowMenu(ActionPoint actionPoint) {
        base.ShowMenu();
        currentActionPoint = actionPoint;
        MenuState = MenuStateEnum.Opened;
    }

    public override void HideMenu() {
        base.HideMenu();
        if (plane != null)
            Destroy(plane);
    }

    public void MoveBtnPressed() {
        TranslateJoystick.gameObject.SetActive(true);
        GameManager.Instance.SetEditorState(GameManager.EditorStateEnum.TransformingAP);
        MenuState = MenuStateEnum.Translating;
        plane = Instantiate(PlanePrefab, currentActionPoint.transform);
    }

    public void MoveBtnReleased() {
        TranslateJoystick.gameObject.SetActive(false);
        GameManager.Instance.SetEditorState(GameManager.EditorStateEnum.Normal);
        MenuState = MenuStateEnum.Opened;
        if (plane != null)
            Destroy(plane);
    }

    public void AdjustSpeedEntered() {
        MenuState = MenuStateEnum.AdjustingSpeed;
    }

    public void AdjustSpeedExited() {
        MenuState = MenuStateEnum.Translating;
    }
}
