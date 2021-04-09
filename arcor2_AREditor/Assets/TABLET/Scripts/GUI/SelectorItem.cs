using System.Collections;
using System.Collections.Generic;
using Base;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.UI;

public class SelectorItem : MonoBehaviour
{
    public TMPro.TMP_Text Label;
    public Image Icon;
    public InteractiveObject InteractiveObject;
    public Button Button;
    public Image SelectionBorder, ManualSelector;
    public float Score;
    private long lastUpdate;
    private bool selected;
    public Sprite ActionPoint, ActionObject, Robot, RobotEE, Orientation, ActionInput, ActionOutput, Action, Others, Connection;
    public Button CollapsableButton;
    public GameObject CollapsableButtonIcon;
    public bool Collapsable, Collapsed;
    public GameObject SublistContent;
    private string name;



    public void SetText(string text) {
        name = text;
        Label.text = text;
    }
    public void SetObject(InteractiveObject interactiveObject, float score, long currentIteration) {
        InteractiveObject = interactiveObject;
        Score = score;
        Button.onClick.AddListener(() => SelectorMenu.Instance.SetSelectedObject(this, true));
        lastUpdate = currentIteration;
        CollapsableButton.gameObject.SetActive(false);
        if (interactiveObject.GetType() == typeof(RobotActionObject)) {
            Icon.sprite = Robot;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(ActionObject)) || interactiveObject.GetType() == typeof(DummyBox) || interactiveObject.GetType().IsSubclassOf(typeof(DummyBox))) {
            Icon.sprite = ActionObject;
        } else if (interactiveObject.GetType() == typeof(PuckInput)) {
            Icon.sprite = ActionInput;
        } else if (interactiveObject.GetType() == typeof(PuckOutput)) {
            Icon.sprite = ActionOutput;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(Base.Action))) {
            Icon.sprite = Action;
        } else if (interactiveObject.GetType().IsSubclassOf(typeof(Base.ActionPoint))) {
            Collapsable = true;
            CollapsableButton.gameObject.SetActive(true);
            Icon.sprite = ActionPoint;
        } else if (interactiveObject.GetType() == typeof(RobotEE)) {
            Icon.sprite = RobotEE;
        } else if (interactiveObject.GetType() == typeof(APOrientation)) {
            Icon.sprite = Orientation;
        } else if (interactiveObject.GetType() == typeof(ConnectionLine)) {
            Icon.sprite = Connection;
        } else {
            Icon.sprite = Others;
        }
    }

    public void CollapseBtnCb() {
        Collapsed = !Collapsed;
        ActionPoint3D actionPoint = (ActionPoint3D) InteractiveObject;
        if (Collapsed) {
            CollapsableButtonIcon.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
            actionPoint.ActionsCollapsed = true;
            actionPoint.UpdatePositionsOfPucks();
        } else {
            CollapsableButtonIcon.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
            actionPoint.ActionsCollapsed = false;
            actionPoint.UpdatePositionsOfPucks();
        }
    }

    public void UpdateScore(float score, long currentIteration) {
        lastUpdate = currentIteration;
        Label.text = name + " (" + score.ToString() + ")";
        Score = score;
    }

    public long GetLastUpdate() {
        return lastUpdate;
    }

    public void SetSelected(bool selected, bool manually) {
        if (InteractiveObject != null) {
            if (selected) {
                InteractiveObject.SendMessage("OnHoverStart");
            } else {
                if (this.selected)
                    InteractiveObject.SendMessage("OnHoverEnd");
            }
        }   
        this.selected = selected;
        if (manually) {
            SelectionBorder.gameObject.SetActive(false);
            ManualSelector.gameObject.SetActive(selected);
        } else {
            ManualSelector.gameObject.SetActive(false);
            SelectionBorder.gameObject.SetActive(selected);
        }
    }

    public bool IsSelected() {
        return selected;
    }



}
