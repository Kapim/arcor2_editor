using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using Base;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.UI;
using static Base.GameManager;

[RequireComponent(typeof(CanvasGroup))]
public class LeftMenu : Base.Singleton<LeftMenu> {

    private CanvasGroup CanvasGroup;

    public Button FocusButton, RobotButton, AddButton, SettingsButton, HomeButton;
    public Button AddMeshButton, MoveButton, RemoveButton, SetActionPointParentButton, AddActionButton;
    public GameObject HomeButtons, SettingsButtons, AddButtons, MeshPicker;
    public TMPro.TMP_Text ProjectName, SelectedObjectText;

    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
    }

    private bool updateButtonsInteractivity = false;

    private bool requestingObject = false;

    private void OnEditorStateChanged(object sender, EditorStateEventArgs args) {
        switch (args.Data) {
            case GameManager.EditorStateEnum.Normal:
                requestingObject = false;
                updateButtonsInteractivity = true;
                break;
            case GameManager.EditorStateEnum.InteractionDisabled:
                updateButtonsInteractivity = false;

                

                break;
            case GameManager.EditorStateEnum.Closed:
                updateButtonsInteractivity = false;
                break;
            case EditorStateEnum.SelectingAction:
            case EditorStateEnum.SelectingActionInput:
            case EditorStateEnum.SelectingActionObject:
            case EditorStateEnum.SelectingActionOutput:
            case EditorStateEnum.SelectingActionPoint:
            case EditorStateEnum.SelectingActionPointParent:
                requestingObject = true;
                break;
        }
    }

    private void Update() {
        UpdateVisibility();
        if (!updateButtonsInteractivity)
            return;

        if (MenuManager.Instance.CheckIsAnyRightMenuOpened()) {
            SetActiveSubmenu(LeftMenuSelection.None);
            FocusButton.GetComponent<Image>().enabled = false;

            RobotButton.interactable = false;
            AddButton.interactable = false;
            SettingsButton.interactable = false;
            HomeButton.interactable = false;
            return;
        }

        FocusButton.GetComponent<Image>().enabled = true;
        RobotButton.interactable = true;
        AddButton.interactable = true;
        SettingsButton.interactable = true;
        HomeButton.interactable = true;

        

        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (requestingObject || selectedObject == null) {
            SelectedObjectText.text = "";
            MoveButton.interactable = false;
            RemoveButton.interactable = false;
            SetActionPointParentButton.interactable = false;
            AddActionButton.interactable = false;
        } else {
            SelectedObjectText.text = selectedObject.GetName() + "\n" + selectedObject.GetType();

            MoveButton.interactable = selectedObject.Movable();
            RemoveButton.interactable = selectedObject.Removable();

            SetActionPointParentButton.interactable = selectedObject is ActionPoint3D;
            AddActionButton.interactable = selectedObject is ActionPoint3D;
        }

        //InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        //if (requestingObject || selectedObject == null) {
        //    ConnectionsBtn.interactable = false;
        //    MoveBtn.interactable = false;
        //    MenuBtn.interactable = false;
        //    InteractBtn.interactable = false;

        //} else {
        //    ConnectionsBtn.interactable = selectedObject.GetType() == typeof(PuckInput) ||
        //         selectedObject.GetType() == typeof(PuckOutput);

        //    MoveBtn.interactable = selectedObject.Movable();
        //    MenuBtn.interactable = selectedObject.HasMenu();
        //    InteractBtn.interactable = selectedObject.GetType() == typeof(Recalibrate) ||
        //        selectedObject.GetType() == typeof(CreateAnchor);
        //}
        /*
        if (GameManager.Instance.ProjectOpened)
            ;
        if(SceneManager.Instance.SceneMeta.Nam)
        */
        if (SceneManager.Instance.SceneMeta != null)
            ProjectName.text = "Project: \n" + SceneManager.Instance.SceneMeta.Name;
    }

    private void UpdateVisibility() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen ||
            GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected ||
            MenuManager.Instance.MainMenu.CurrentState == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Open) {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 0;
        } else {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.alpha = 1;
        }
    }

    public void FocusButtonClick() {
        MenuManager.Instance.HideAllMenus();
        SelectorMenu.Instance.gameObject.SetActive(true);
        SetActiveSubmenu(LeftMenuSelection.None);
        Notifications.Instance.ShowNotification("Not implemented", "");

    }

    public void RobotButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.None);
        Notifications.Instance.ShowNotification("Not implemented", "");

    }

    public void AddButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Add, !AddButtons.activeInHierarchy);

    }

    public void SettingsButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Settings, !SettingsButtons.activeInHierarchy);


    }

    public void HomeButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Home, !HomeButtons.activeInHierarchy);
        /*
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.GetType() == typeof(Recalibrate)) {
            ((Recalibrate) selectedObject).OnClick(Clickable.Click.TOUCH);
        } else if (selectedObject.GetType() == typeof(CreateAnchor)) {
            ((CreateAnchor) selectedObject).OnClick(Clickable.Click.TOUCH);
        }
        */
    }

    #region Add submenu button click methods

    public void AddActionClick() {
        Notifications.Instance.ShowNotification("Not implemented", "");
    }

    public void AddActionPointClick() {
        GameManager.Instance.AddActionPointExperiment();
        SetActiveSubmenu(LeftMenuSelection.None);
    }

    public void AddMeshClick() {
        if (AddMeshButton.GetComponent<Image>().enabled) {
            AddMeshButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            MeshPicker.SetActive(false);
        } else {
            AddMeshButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            MeshPicker.SetActive(true);
        }
        
    }

    #endregion

    #region Settings submenu button click methods

    public void SetActionPointParentClick() {
        Notifications.Instance.ShowNotification("Not implemented", "");

    }

    public void MoveClick() {
        Notifications.Instance.ShowNotification("Not implemented", "");
        MoveButton.GetComponent<Image>().enabled = true;

    }

    public void RemoveClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;

        selectedObject.Remove();
        SetActiveSubmenu(LeftMenuSelection.None);
        //Notifications.Instance.ShowToastMessage(selectedObject.GetName() + " removed successfuly");
    }


    #endregion

    #region Home submenu button click methods

    #endregion

    #region Mesh picker click methods

    public void BlueBoxClick() {
        Notifications.Instance.ShowNotification("Not implemented", "");
        SelectorMenu.Instance.gameObject.SetActive(true);
        MeshPicker.SetActive(false);
        SetActiveSubmenu(LeftMenuSelection.None);
    }

    #endregion


    private void SetActiveSubmenu(LeftMenuSelection which, bool active = true) {
        DeactivateAllSubmenus();
        if (!active)
            return;
        switch (which) {
            case LeftMenuSelection.None:
                break;
            case LeftMenuSelection.Add:
                AddButtons.SetActive(active);
                AddButton.GetComponent<Image>().enabled = active;
                break;
            case LeftMenuSelection.Settings:
                SettingsButtons.SetActive(active);
                SettingsButton.GetComponent<Image>().enabled = active;
                break;
            case LeftMenuSelection.Home:
                HomeButtons.SetActive(active);
                HomeButton.GetComponent<Image>().enabled = active;
                break;
        }
    }

    private void DeactivateAllSubmenus() {
        SelectorMenu.Instance.gameObject.SetActive(true);
        MeshPicker.SetActive(false);

        HomeButtons.SetActive(false);
        SettingsButtons.SetActive(false);
        AddButtons.SetActive(false);

        FocusButton.GetComponent<Image>().enabled = false;
        RobotButton.GetComponent<Image>().enabled = false;
        AddButton.GetComponent<Image>().enabled = false;
        SettingsButton.GetComponent<Image>().enabled = false;
        HomeButton.GetComponent<Image>().enabled = false;

        AddMeshButton.GetComponent<Image>().enabled = false;
        MoveButton.GetComponent<Image>().enabled = false;

    }
}

public enum LeftMenuSelection{
    None, Add, Settings, Home
}
