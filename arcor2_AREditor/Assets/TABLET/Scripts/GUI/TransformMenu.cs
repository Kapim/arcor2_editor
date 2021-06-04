using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using RosSharp.Urdf;
using TrilleonAutomation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using System.Threading.Tasks;
using UnityEditor;

[RequireComponent(typeof(CanvasGroup))]
public class TransformMenu : Singleton<TransformMenu> {
    public InteractiveObject InteractiveObject;
    public TransformWheel TransformWheel;
    public GameObject Wheel, StepButtons;
    public CoordinatesBtnGroup Coordinates;
    public TranformWheelUnits Units, UnitsDegrees;
    private GameObject model;
    public TwoStatesToggle RotateTranslateBtn, RobotTabletBtn;
    //public NStateToggle ModeSelector;
    private float prevValue = 0;
    private bool manuallySelected = false;

    private LineConnection connectionToModel;

    private Vector3 offsetPosition = new Vector3(), interPosition = new Vector3(), cameraOrig = new Vector3();
    //private Quaternion offsetRotation = new Quaternion(), interRotation = Quaternion.identity;

    public CanvasGroup CanvasGroup;

    public bool HandHolding = false, DummyAimBox = false;

    private Vector3 cameraPrev = new Vector3(), cameraOffset = new Vector3();

    private bool robot = false;

    public bool Tester {
        get;
        private set;
    }


    private string robotId;

    private RobotEE endEffector;

    public GameObject DummyBoxing;

    public List<Image> Arrows, ArrowsTester, Dots, DotsBackgrounds;
    private List<GameObject> dummyPoints = new List<GameObject>();
    private int currentArrowIndex;
    public Button NextArrowBtn, PreviousArrowBtn, StepuUpButton, StepDownButton, HandBtn;
    public ButtonWithTooltip SetPivotBtn, ConfirmBtn, ResetBtn;

    public string SelectedAxis = "all";

    private Transform originalParent;


    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
        dummyPoints.Add(null);
        dummyPoints.Add(null);
        dummyPoints.Add(null);
        dummyPoints.Add(null);
    }

    private void Update() {
        if (model == null)
            return;
        if (robot) {
            if (endEffector != null) {
                model.transform.position = endEffector.transform.position;
                Coordinates.X.SetValueMeters(endEffector.transform.position.x);
                Coordinates.X.SetDeltaMeters(InteractiveObject.transform.localPosition.x);
                Coordinates.Y.SetValueMeters(endEffector.transform.position.y);
                Coordinates.Y.SetDeltaMeters(InteractiveObject.transform.localPosition.y);
                Coordinates.Z.SetValueMeters(endEffector.transform.position.z);
                Coordinates.Z.SetDeltaMeters(InteractiveObject.transform.localPosition.z);
                //UpdateTranslate(GetPositionValue(TransformWheel.GetValue()));
                return;
            }
        }
        float newValue = 0;
        if (RotateTranslateBtn.CurrentState == "rotate") {
            newValue = GetRotationValue(TransformWheel.GetValue());
            if (prevValue != newValue)
                UpdateRotate(newValue - prevValue);
            Quaternion delta = TransformConvertor.UnityToROS(GameManager.Instance.Gizmo.transform.localRotation);
            if (InteractiveObject is ActionPoint3D actionPoint)
                delta = TransformConvertor.UnityToROS(Quaternion.Inverse(actionPoint.GetRotation()) * GameManager.Instance.Gizmo.transform.localRotation);
            Quaternion newrotation = TransformConvertor.UnityToROS(model.transform.rotation * Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation));
            //Coordinates.X.SetValueDegrees(newrotation.eulerAngles.x);
            Coordinates.X.SetDeltaDegrees(delta.eulerAngles.x);
            //Coordinates.Y.SetValueDegrees(newrotation.eulerAngles.y);
            Coordinates.Y.SetDeltaDegrees(delta.eulerAngles.y);
            //Coordinates.Z.SetValueDegrees(newrotation.eulerAngles.z);
            Coordinates.Z.SetDeltaDegrees(delta.eulerAngles.z);

        } else {
            newValue = GetPositionValue(TransformWheel.GetValue());
            if (HandHolding || prevValue != newValue)
                UpdateTranslate(newValue - prevValue);
            //Coordinates.X.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).x);
            if (InteractiveObject is ActionPoint3D actionPoint) {
                Vector3 gizmoVector = GameManager.Instance.ModelGizmo.transform.forward;
                Vector3 transformVector = InteractiveObject.transform.position - model.transform.position;
                float angle = Vector3.Angle(gizmoVector, transformVector);
                var length = transformVector.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
                //InteractiveObject.transform.Translate(gizmoVector * lengthNormalized, Space.World);


                Coordinates.X.SetDeltaMeters(length);
                gizmoVector = GameManager.Instance.ModelGizmo.transform.right;
                angle = Vector3.Angle(gizmoVector, transformVector);
                length = transformVector.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
                Coordinates.Y.SetDeltaMeters(length);
                gizmoVector = GameManager.Instance.ModelGizmo.transform.up;
                angle = Vector3.Angle(gizmoVector, transformVector);
                length = transformVector.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
                Coordinates.Z.SetDeltaMeters(length);
            } else {
                Coordinates.X.SetDeltaMeters(TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition).x / 50);
                /*Vector3 gizmoVector = model.transform.forward;
                float angle = Vector3.Angle(gizmoVector, cameraVector);
                float length = cameraVector.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
                //cameraOffset += Vector3.forward * length;
                //lengthNormalized = GetRoundedValue(cameraOffset.z);
                Debug.LogError(lengthNormalized);
                //cameraOffset -= Vector3.forward * lengthNormalized;
                model.transform.Translate(model.transform.forward * lengthNormalized, Space.World);*/
                //Coordinates.X.SetDeltaMeters(TransformConvertor.UnityToROS(model.transform.position - InteractiveObject.transform.position).x);
                //Coordinates.Y.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).y);
                Coordinates.Y.SetDeltaMeters(TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition).y / 50);
                //Coordinates.Z.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).z);
                Coordinates.Z.SetDeltaMeters(TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition).z / 50);
            }

        }
        
        
        prevValue = newValue;
        
    }
    private void Start() {
        WebsocketManager.Instance.OnRobotMoveToPoseEvent += OnRobotMoveToPoseEvent;
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;
    }

    private void OnObjectSelectedChangedEvent(object sender, InteractiveObjectEventArgs args) {
        if (args.InteractiveObject == null)
            SetPivotBtn.SetInteractivity(false, "No object selected");
        else
            SetPivotBtn.SetInteractivity(true);
    }

    private void OnRobotMoveToPoseEvent(object sender, RobotMoveToPoseEventArgs args) {
        if (args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToPoseData.MoveEventTypeEnum.End ||
            args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToPoseData.MoveEventTypeEnum.Failed) {
            StepuUpButton.interactable = true;
            StepDownButton.interactable = true;
        }
    }

    private float GetPositionValue(float v) {
        switch (Units.GetValue()) {
            case "m":
                return v;
            case "cm":
                return v * 0.01f;
            case "mm":
                return v * 0.001f;
            case "μm":
                return v * 0.000001f;
            default:
                return v;
        };
    }

    private int ComputePositionValue(float value) {
        switch (Units.GetValue()) {
            case "cm":
                return (int) (value * 100);
            case "mm":
                return (int) (value * 1000);
            case "μm":
                return (int) (value * 1000000);
            default:
                return (int) value;
        };
    }

    private float GetRotationValue(float v) {
        switch (UnitsDegrees.GetValue()) {
            case "°":
                return v;
            case "'":
                return v / 60f;
            case "''":
                return v / 3600f;
            default:
                return v;
        };
    }

    private int ComputeRotationValue(float value) {
        switch (UnitsDegrees.GetValue()) {
            case "°":
                return (int) value;
            case "'":
                return (int) (value * 60);
            case "''":
                return (int) (value * 3600);
            default:
                return (int) value;
        };
    }

    private async void UpdateTranslate(float wheelValue) {
        if (model == null)
            return;

        /*if (wheelValue != 0) {
            ConfirmBtn.SetInteractivity(true);
            ResetBtn.SetInteractivity(true);
        }*/
        if (HandHolding) {
            /*Vector3 cameraNow = TransformConvertor.UnityToROS(InteractiveObject.transform.InverseTransformPoint(Camera.main.transform.position));
            offsetPosition.x = GetRoundedValue(cameraNow.x - cameraOrig.x);
            offsetPosition.y = GetRoundedValue(cameraNow.y - cameraOrig.y);
            offsetPosition.z = GetRoundedValue(cameraNow.z - cameraOrig.z);*/
            Vector3 cameraNow = Camera.main.transform.position;
            Vector3 cameraVector = cameraNow - cameraPrev;
            Vector3 gizmoVector;
            float length, lengthNormalized;
            float angle;
            switch (SelectedAxis) {
                case "x":
                    /* cameraOffset = model.transform.InverseTransformDirection(Vector3.forward).normalized * (cameraNow.z - cameraPrev.z);
                     float offset = GetRoundedValue(cameraOffset.z);
                     model.transform.Translate(cameraOffset, Space.Self);*/
                    //cameraOffset -= cameraOffset;
                    /*float dist = (Quaternion.Inverse(model.transform.rotation) * (cameraNow - cameraPrev)).magnitude;
                    model.transform.Translate(Vector3.forward * dist, Space.Self);*/
                    gizmoVector = GameManager.Instance.Gizmo.transform.forward;
                    angle = Vector3.Angle(gizmoVector, cameraVector);
                    length = cameraVector.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
                    cameraOffset += Vector3.forward * length;
                    lengthNormalized = GetRoundedValue(cameraOffset.z);
                    Debug.LogError(lengthNormalized);
                    cameraOffset -= Vector3.forward * lengthNormalized;
                    InteractiveObject.transform.Translate(gizmoVector * lengthNormalized, Space.World);
                    break;
                case "y":
                    /*cameraOffset += Vector3.right * (cameraNow.x - cameraPrev.x);
                    offset = GetRoundedValue(cameraOffset.x);
                    model.transform.Translate(Vector3.right * offset);
                    cameraOffset -= Vector3.right * offset;*/
                    //model.transform.position += new Vector3(GetRoundedValue(cameraNow.x - cameraPrev.x), 0, 0);
                    //cameraOffset = model.transform.InverseTransformDirection(Vector3.right) * (cameraNow.x - cameraPrev.x);
                    //offset = GetRoundedValue(cameraOffset.z);
                    //model.transform.Translate(cameraOffset, Space.World);
                    //cameraOffset -= cameraOffset;
                    gizmoVector = GameManager.Instance.Gizmo.transform.right;
                    angle = Vector3.Angle(gizmoVector, cameraVector);
                    length = cameraVector.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
                    cameraOffset += Vector3.right * length;
                    lengthNormalized = GetRoundedValue(cameraOffset.x);
                    Debug.LogError(lengthNormalized);
                    cameraOffset -= Vector3.right * lengthNormalized;
                    InteractiveObject.transform.Translate(gizmoVector * lengthNormalized, Space.World);
                    break;
                case "z":
                    /*cameraOffset += Vector3.up * (cameraNow.y - cameraPrev.y);
                    offset = GetRoundedValue(cameraOffset.y);
                    model.transform.Translate(Vector3.up * offset);
                    cameraOffset -= Vector3.up * offset;*/
                    //model.transform.position += new Vector3(0, GetRoundedValue(cameraNow.y - cameraPrev.y), 0);
                    /*cameraOffset = model.transform.InverseTransformDirection(Vector3.up) * (cameraNow.y - cameraPrev.y);
                    offset = GetRoundedValue(cameraOffset.z);
                    model.transform.Translate(cameraOffset, Space.Self);*/
                    //cameraOffset -= cameraOffset;
                    gizmoVector = GameManager.Instance.Gizmo.transform.up;
                    angle = Vector3.Angle(gizmoVector, cameraVector);
                    length = cameraVector.magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);
                    cameraOffset += Vector3.up * length;
                    lengthNormalized = GetRoundedValue(cameraOffset.y);
                    Debug.LogError(lengthNormalized);
                    cameraOffset -= Vector3.up * lengthNormalized;
                    InteractiveObject.transform.Translate(gizmoVector * lengthNormalized, Space.World);
                    break;
                case "all":
                    cameraOffset += new Vector3(cameraNow.x - cameraPrev.x, cameraNow.y - cameraPrev.y, cameraNow.z - cameraPrev.z);
                    Vector3 vectorOffset = new Vector3(GetRoundedValue(cameraOffset.x), GetRoundedValue(cameraOffset.y), GetRoundedValue(cameraOffset.z));
                    InteractiveObject.transform.position += vectorOffset;
                    cameraOffset -= vectorOffset;
                    //model.transform.position += new Vector3(GetRoundedValue(cameraNow.x - cameraPrev.x), GetRoundedValue(cameraNow.y - cameraPrev.y), GetRoundedValue(cameraNow.z - cameraPrev.z));
                    break;
            }
            //model.transform.position += new Vector3(cameraNow.x - cameraPrev.x, cameraNow.y - cameraPrev.y, cameraNow.z - cameraPrev.z);
            cameraPrev = cameraNow;

        } else {

            switch (SelectedAxis) {
                case "x":
                    InteractiveObject.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.right), GameManager.Instance.Gizmo.transform);
                    break;
                case "y":
                    InteractiveObject.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.up), GameManager.Instance.Gizmo.transform);
                    break;
                case "z":
                    InteractiveObject.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.forward), GameManager.Instance.Gizmo.transform);
                    break;
            }
        }
        /*if (InteractiveObject.GetType() == typeof(ActionPoint3D)) {
            Vector3 position = TransformConvertor.ROSToUnity(interPosition + offsetPosition);
            model.transform.localPosition = ((ActionPoint3D) InteractiveObject).GetRotation() * position;
            
        } else if (InteractiveObject.GetType() == typeof(DummyBox)) {
            Vector3 position = TransformConvertor.ROSToUnity(interPosition + offsetPosition);
            model.transform.localPosition = position;
            
        }

        Vector3 newPosition = TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition + model.transform.localPosition);
        Coordinates.X.SetValueMeters(newPosition.x);
        Coordinates.X.SetDeltaMeters(offsetPosition.x + interPosition.x);
        Coordinates.Y.SetValueMeters(newPosition.y);
        Coordinates.Y.SetDeltaMeters(offsetPosition.y + interPosition.y);
        Coordinates.Z.SetValueMeters(newPosition.z);
        Coordinates.Z.SetDeltaMeters(offsetPosition.z + interPosition.z);*/
    }

    private void UpdateRotate(float wheelValue) {
        
        if (HandHolding) {

        } else {

            switch (SelectedAxis) {
                case "x":
                    Debug.LogError(InteractiveObject.transform.rotation);
                    GameManager.Instance.Gizmo.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.right));
                    Debug.LogError(InteractiveObject.transform.rotation);
                    break;
                case "y":
                    GameManager.Instance.Gizmo.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.up));
                    break;
                case "z":
                    GameManager.Instance.Gizmo.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.forward));
                    break;
            }

        }
    }

    public float GetRoundedValue(float value) {
        switch (Units.GetValue()) {
            case "cm":
                return (float) Math.Round(value * 100) / 100f;
            case "mm":
                return (float) Math.Round(value * 1000) / 1000;
            case "μm":
                return (float) Math.Round(value * 1000000) / 1000000;
            default:
                return (float) Math.Round(value);
        };
    }

    public void SwitchToTranslate() {
        ResetTransformWheel();
        TransformWheel.Units = Units;
        Units.gameObject.SetActive(true);
        UnitsDegrees.gameObject.SetActive(false);
        //ResetPosition();
        //UpdateTranslate(0);
    }

    public void SwitchToRotate() {
        ResetTransformWheel();
        TransformWheel.Units = UnitsDegrees;
        Units.gameObject.SetActive(false);
        UnitsDegrees.gameObject.SetActive(true);
        
    }

    public void SwitchToTablet() {
        //SelectorMenu.Instance.Active = false;
        ResetTransformWheel();
        TransformWheel.gameObject.SetActive(true);
        Wheel.gameObject.SetActive(true);
        StepButtons.gameObject.SetActive(false);
        RotateTranslateBtn.SetInteractivity(true);
        //SetPivotBtn.gameObject.SetActive(false);
        HandBtn.interactable = true;
        //ResetPosition();
        //UpdateTranslate(0);
        robot = false;
    }

    public void SwitchToRobot() {
        if (endEffector == null) {
            endEffector = FindObjectOfType<RobotEE>();
            if (endEffector == null) {
                Notifications.Instance.ShowNotification("Robot not ready", "Scene not started");
                return;
            }
        }
        //SelectorMenu.Instance.Active = false;
        //TransformWheel.gameObject.SetActive(false);
        Wheel.gameObject.SetActive(false);
        StepButtons.gameObject.SetActive(true);
        //SetPivotBtn.gameObject.SetActive(false);
        if (robot) {
            robot = false;
            SwitchToTranslate();
        }
        RotateTranslateBtn.SetInteractivity(false);
        HandBtn.interactable = true;
        ResetPosition(default, true);
        robot = true;
        //UpdateTranslate(0);
    }

   

    public void SetPivot() {
        //ResetPosition();

        //UpdateTranslate(0);
        InteractiveObject interactiveObject = SelectorMenu.Instance.GetSelectedObject();
        if (interactiveObject is Action3D action)
            interactiveObject = action.ActionPoint;
        if (interactiveObject != null) {
            ConfirmBtn.SetInteractivity(true);
            ResetBtn.SetInteractivity(true);
            model.transform.position = interactiveObject.transform.position;
            
        }

    }

    public void RobotHoldPressed() {
        _ = WebsocketManager.Instance.HandTeachingMode(robotId: robotId, enable: true);
    }

    public async void RobotHoldReleased() {
        await WebsocketManager.Instance.HandTeachingMode(robotId: robotId, enable: false);
        IO.Swagger.Model.Position position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(endEffector.transform.position)));
        await WebsocketManager.Instance.MoveToPose(robotId, endEffector.GetId(), 1, position, DataHelper.QuaternionToOrientation(Quaternion.Euler(180, 0, 0)), false);
    }

    public void TabletHoldPressed() {
        cameraPrev = Camera.main.transform.position;
        cameraOffset = Vector3.zero;
        HandHolding = true;
        
    }

    public void TabletHoldReleased() {
        HandHolding = false;
        ConfirmBtn.SetInteractivity(true);
        ResetBtn.SetInteractivity(true);
    }


/*

    public void StoreInterPosition() {
        prevValue = 0;
        interPosition += offsetPosition;
        //interRotation *= offsetRotation;
        offsetPosition = Vector3.zero;
        //offsetRotation = Quaternion.identity;

        if (RotateTranslateBtn.CurrentState == "translate")
            UpdateTranslate(0);
        else
            UpdateRotate(0);
    }*/

    public void SetSelectedAxis(string axis) {
        SelectedAxis = axis;
        ResetTransformWheel();
        if (SelectedAxis == "all")
            TransformWheel.SetInteractive(false);
        else
            TransformWheel.SetInteractive(true);
    }

    public void SetPivotMode() {
        /*
        HandBtn.gameObject.SetActive(false);
        SetPivotBtn.gameObject.SetActive(true);*/
    }

    private LineConnection CreateConnection(Transform obj, Transform model) {
        GameObject c = Instantiate(SceneManager.Instance.LineConnectionPrefab);
        c.transform.parent = transform;
        LineConnection newConnection = c.GetComponent<LineConnection>();
        newConnection.targets[0] = model;
        newConnection.targets[1] = obj;

        // add the connection to connections manager
        return newConnection;

    }

    public void Show(InteractiveObject interactiveObject, bool dummyAimBox = false, bool tester = false) {
        manuallySelected = SelectorMenu.Instance.ManuallySelected;
        
        SelectorMenu.Instance.DeselectObject();
        foreach (IRobot robot in SceneManager.Instance.GetRobots()) {
            robotId = robot.GetId();
        }
        if (!dummyAimBox) {
            ConfirmBtn.SetInteractivity(false, "Position / orientation not changed");
            ResetBtn.SetInteractivity(false, "Position / orientation not changed");
        }
        RotateTranslateBtn.SetState("translate");
        //RobotTabletBtn.SetInteractivity(true);
        RotateTranslateBtn.SetInteractivity(true);
        DummyBoxing.SetActive(false);
        offsetPosition = Vector3.zero;
        ResetTransformWheel();
        DummyAimBox = dummyAimBox;
        Tester = tester;
        SwitchToTranslate();
        if (DummyAimBox) {
            InteractiveObject = ((DummyAimBox) interactiveObject).ActionPoint;
            robot = true;
            SwitchToRobot();
        }
        else {
            InteractiveObject = interactiveObject;
            robot = false;
            SwitchToTablet();
        }
        originalParent = InteractiveObject.transform.parent;
        GameManager.Instance.Gizmo.transform.SetParent(interactiveObject.transform);
        GameManager.Instance.Gizmo.transform.localPosition = Vector3.zero;
        GameManager.Instance.Gizmo.transform.localRotation = Quaternion.identity;
        if (interactiveObject is ActionPoint3D actionPoint) {
            actionPoint.FixRotation = false;
            model = actionPoint.GetModelCopy();
            connectionToModel = CreateConnection(actionPoint.transform, model.transform);
            GameManager.Instance.Gizmo.transform.localRotation = actionPoint.GetRotation();
            //origRotation = TransformConvertor.UnityToROS(((ActionPoint3D) interactiveObject).GetRotation());
        } else if (interactiveObject is DummyBox dummyBox) {
            model = dummyBox.GetModelCopy();
            connectionToModel = CreateConnection(dummyBox.transform, model.transform);
            //origRotation = interactiveObject.transform.localRotation;
        } else if (DummyAimBox) {
            model = GetPointModel();
            for (int i = 0; i < 4; ++i) {
                bool aimed = PlayerPrefsHelper.LoadBool(Base.ProjectManager.Instance.ProjectMeta.Id + (Tester ? "/BlueBox" : "/Tester") + "/PointAimed/" + i, false);
                if (aimed)
                    Dots[i].color = Color.green;
                else
                    Dots[i].color = Color.red;
            }
            DummyBoxing.SetActive(true);
            //RobotTabletBtn.SetInteractivity(false);
            RotateTranslateBtn.SetInteractivity(false);
            currentArrowIndex = 0;
            SetArrowVisible(0);
        }
        if (model == null) {
            Hide();
            return;
        }
        model.transform.SetParent(originalParent);
        GameManager.Instance.ModelGizmo.transform.SetParent(model.transform);
        GameManager.Instance.ModelGizmo.SetActive(true);
        GameManager.Instance.ModelGizmo.transform.localRotation = Quaternion.identity;
        GameManager.Instance.ModelGizmo.transform.localScale = Vector3.one;
        GameManager.Instance.ModelGizmo.transform.localPosition = Vector3.zero;
        if (interactiveObject is ActionPoint3D ap) {
            GameManager.Instance.ModelGizmo.transform.localRotation = ap.GetRotation();
        }
        interactiveObject.transform.SetParent(model.transform);
        
        GameManager.Instance.Gizmo.SetActive(true);
        Sight.Instance.GizmoArrowsColliders.Clear();
        foreach (MeshCollider c in GameManager.Instance.Gizmo.transform.GetComponentsInChildren<MeshCollider>()) {
            Sight.Instance.GizmoArrowsColliders.Add(c);
        }
        foreach (MeshCollider c in model.transform.GetComponentsInChildren<MeshCollider>()) {
            Sight.Instance.GizmoArrowsColliders.Add(c);
        }
        enabled = true;
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        SelectorMenu.Instance.gameObject.SetActive(false);
    }

    private GameObject GetPointModel() {
        GameObject m = Instantiate(ProjectManager.Instance.ActionPointSphere, InteractiveObject.transform);
        m.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
        return m;
    }

    private void SetArrowVisible(int index) {
        for (int i = 0; i < 4; ++i) {
            Arrows[i].gameObject.SetActive(false);
            ArrowsTester[i].gameObject.SetActive(false);
            DotsBackgrounds[i].color = Color.clear;
        }
        if (Tester)
            ArrowsTester[index].gameObject.SetActive(true);
        else
            Arrows[index].gameObject.SetActive(true);
        DotsBackgrounds[index].color = Color.white;
        NextArrowBtn.interactable = index != 3;
        PreviousArrowBtn.interactable = index != 0;
    }
    public void NextArrow() {
        Debug.LogError("next");
        if (currentArrowIndex < 3)
            SetArrowVisible(++currentArrowIndex);
    }

    public void PreviousArrow() {
        Debug.LogError("previous");
        if (currentArrowIndex > 0)
            SetArrowVisible(--currentArrowIndex);

    }

    public void HideWithButton() {
        Hide();
        if (LeftMenu.Instance.CurrentMode == LeftMenu.Mode.AddAction) {
            SelectorMenu.Instance.DeselectObject();
            RightButtonsMenu.Instance.SetActionMode();
            RightButtonsMenu.Instance.gameObject.SetActive(true);
        } else if (LeftMenu.Instance.CurrentMode == LeftMenu.Mode.Move) {
            SelectorMenu.Instance.DeselectObject();
            RightButtonsMenu.Instance.SetMoveMode();
            RightButtonsMenu.Instance.gameObject.SetActive(true);
        } else {
            LeftMenu.Instance.MoveButton.GetComponent<Image>().enabled = false;
            //LeftMenu.Instance.RestoreSelector();
        }

        SelectorMenu.Instance.Active = true;
    }

    public async void Hide() {
        if (InteractiveObject != null) {
            
            InteractiveObject.transform.SetParent(originalParent);
            await SubmitPosition();
            if (InteractiveObject is ActionPoint3D actionPoint)
                actionPoint.FixRotation = true;
        }
        for (int i = 0; i < 4; ++i) {
            if (dummyPoints[i] != null) {
                Destroy(dummyPoints[i]);
            }
        }
        if (manuallySelected)
            SelectorMenu.Instance.SetSelectedObject(InteractiveObject, true);            
        InteractiveObject = null;
        GameManager.Instance.Gizmo.SetActive(false);
        GameManager.Instance.ModelGizmo.SetActive(false);
        GameManager.Instance.Gizmo.transform.SetParent(GameManager.Instance.Scene.transform);
        GameManager.Instance.ModelGizmo.transform.SetParent(GameManager.Instance.Scene.transform);
        Destroy(model);
        model = null;
        enabled = false;
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        Sight.Instance.GizmoArrowsColliders.Clear();
        if (connectionToModel != null)
            Destroy(connectionToModel.gameObject);
    }

    public void ResetTransformWheel() {

        /*if (handHolding)
            cameraOrig = TransformConvertor.UnityToROS(InteractiveObject.transform.InverseTransformPoint(Camera.main.transform.position));*/
        /*switch (Coordinates.GetSelectedAxis()) {
            case "x":
                if (RotateTranslateBtn.CurrentState == "rotate")
                    TransformWheel.InitList(ComputeRotationValue(offsetPosition.x));
                else
                    TransformWheel.InitList(ComputePositionValue(offsetPosition.x));
                break;
            case "y":
                if (RotateTranslateBtn.CurrentState == "rotate")
                    TransformWheel.InitList(ComputeRotationValue(offsetPosition.y));
                else
                    TransformWheel.InitList(ComputePositionValue(offsetPosition.y));
                break;
            case "z":
                if (RotateTranslateBtn.CurrentState == "rotate")
                    TransformWheel.InitList(ComputeRotationValue(offsetPosition.z));
                else
                    TransformWheel.InitList(ComputePositionValue(offsetPosition.z));
                break;
        }*/
        prevValue = 0;
        TransformWheel.SetInteractive(SelectedAxis != "all");
        TransformWheel.InitList(0);
    }
    public async Task SubmitPosition() {
        if (DummyAimBox) {
            Dots[currentArrowIndex].color = Color.green;
            PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + (Tester ? "/BlueBox" : "/Tester") + "/PointAimed/" + currentArrowIndex, true);
            if (dummyPoints[currentArrowIndex] != null) {
                GameManager.Instance.Gizmo.transform.SetParent(GameManager.Instance.Scene.transform);
                Destroy(dummyPoints[currentArrowIndex]);
            }
            model.transform.SetParent(GameManager.Instance.Scene.transform, true);
            dummyPoints[currentArrowIndex] = model;
            model = GetPointModel();

            GameManager.Instance.Gizmo.transform.SetParent(model.transform);
            GameManager.Instance.Gizmo.transform.localPosition = Vector3.zero;
            bool done = true;
            foreach (GameObject p in dummyPoints) {
                if (p == null) {
                    done = false;
                    break;
                }
            }
            if (done) {
                if (Tester) {
                    DummyAimBoxTester dummyAimBox = FindObjectOfType<DummyAimBoxTester>();
                    if (dummyAimBox != null)
                        dummyAimBox.AimFinished();
                } else {
                    DummyAimBox dummyAimBox = FindObjectOfType<DummyAimBox>();
                    if (dummyAimBox != null)
                        dummyAimBox.AimFinished();
                }
            }
        } else if (InteractiveObject is ActionPoint3D actionPoint) {
            if (model == null)
                return;
            try {
                if (!robot) {
                    //await WebsocketManager.Instance.UpdateActionPointPosition(InteractiveObject.GetId(), DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition + model.transform.localPosition)));
                    await WebsocketManager.Instance.UpdateActionPointPosition(InteractiveObject.GetId(), DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(model.transform.parent.InverseTransformPoint(InteractiveObject.transform.position))));
                } else {
                    await WebsocketManager.Instance.UpdateActionPointUsingRobot(InteractiveObject.GetId(), robotId, endEffector.EEId);
                }
                actionPoint.SetRotation(GameManager.Instance.Gizmo.transform.localRotation);
                //ResetPosition();
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action point position", e.Message);
            }
        } else if (InteractiveObject.GetType() == typeof(DummyBox)) {
            //InteractiveObject.transform.localRotation = TransformConvertor.ROSToUnity(origRotation * interRotation * offsetRotation);
            //InteractiveObject.transform.Translate(TransformConvertor.ROSToUnity(interPosition + offsetPosition));
            
            InteractiveObject.transform.hasChanged = true;
            //ResetPosition();
        }

    }


    public void ResetPosition() {
        ResetPosition(default, default);
    }

    public void ResetPosition(bool manually = false, bool forceConfirmInteracitve = false) {
        if (manually && DummyAimBox) {
            for (int i = 0; i < 4; ++i) {
                Dots[i].color = Color.red;
                PlayerPrefsHelper.SaveBool(Base.ProjectManager.Instance.ProjectMeta.Id + (Tester ? "/BlueBox" : "/Tester") + "/PointAimed/" + i, false);
                if (dummyPoints[i] != null) {
                    Destroy(dummyPoints[i]);
                }
                if (Tester) {
                    DummyAimBoxTester dummyAimBox = FindObjectOfType<DummyAimBoxTester>();
                    if (dummyAimBox != null) {
                        dummyAimBox.SetVisibility(false);
                        dummyAimBox.Reseted = true;
                    }
                } else {
                    DummyAimBox dummyAimBox = FindObjectOfType<DummyAimBox>();
                    if (dummyAimBox != null) {
                        dummyAimBox.SetVisibility(false);
                        dummyAimBox.Reseted = true;
                    }
                }
            }
        } else {
            if (forceConfirmInteracitve) {
                ConfirmBtn.SetInteractivity(true);
                ResetBtn.SetInteractivity(true);
            } else if (!DummyAimBox) {
                ConfirmBtn.SetInteractivity(false, "Position / orientation not changed");
                ResetBtn.SetInteractivity(false, "Position / orientation not changed");
            }

            InteractiveObject.transform.position = model.transform.position;
            InteractiveObject.transform.rotation = model.transform.rotation;
            ResetTransformWheel();
            if (InteractiveObject != null && model != null) {
                if (InteractiveObject.GetType() == typeof(ActionPoint3D)) {
                    InteractiveObject.transform.localRotation = ((ActionPoint3D) InteractiveObject).GetRotation();
                } /*else if (InteractiveObject.GetType() == typeof(DummyBox)) {
                    model.transform.localRotation = Quaternion.identity;
                }*/
            }
        }
        
    }

    public void RobotStepUp() {
            RobotStep(GetPositionValue(1));
        }


        public void RobotStepDown() {
            RobotStep(GetPositionValue(-1));
        }

        public async void RobotStep(float step) {
            if (robot && endEffector != null) {
                IO.Swagger.Model.Position position = endEffector.Position;
                Vector3 offset = Vector3.zero;

                switch (SelectedAxis) {
                    case "x":

                        offset = new Vector3(step, 0, 0);
                        break;
                    case "y":
                        offset = new Vector3(0, step, 0);
                        break;
                    case "z":
                        offset = new Vector3(0, 0, step);
                        break;
                }
                if (InteractiveObject.GetType() == typeof(ActionPoint3D)) {
                    position.X += DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(((ActionPoint3D) InteractiveObject).GetRotation()) * offset).X;
                    position.Y += DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(((ActionPoint3D) InteractiveObject).GetRotation()) * offset).Y;
                    position.Z += DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(((ActionPoint3D) InteractiveObject).GetRotation()) * offset).Z;
                    try {
                        StepButtons.SetActive(false);
                        IRobot r = SceneManager.Instance.GetRobot(robotId);

                        await WebsocketManager.Instance.MoveToPose(robotId, endEffector.EEId, (decimal) 1, position,
                            DataHelper.QuaternionToOrientation(Quaternion.Euler(180, 0, 0)), false);;
                    } catch (RequestFailedException) {

                    } finally {
                        StepButtons.SetActive(true);
                    }
                } else if (InteractiveObject.GetType() == typeof(DummyBox)) {
                    position.X += DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.localRotation) * offset).X;
                    position.Y += DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.localRotation) * offset).Y;
                    position.Z += DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.localRotation) * offset).Z;
                    try {
                        StepButtons.SetActive(false);
                        await WebsocketManager.Instance.MoveToPose(robotId, endEffector.EEId, (decimal) 1, position, DataHelper.QuaternionToOrientation(Quaternion.Euler(180, 0, 0)), false);
                    } catch (RequestFailedException) {

                    } finally {
                        StepButtons.SetActive(true);
                    }
                }

            }
        }

}
