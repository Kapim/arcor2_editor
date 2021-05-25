using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class DummyBox : InteractiveObject {
    public string Name = "", id = "";
    public GameObject Visual;
    public OutlineOnClick OutlineOnClick;

    protected virtual void Awake() {
        id = Guid.NewGuid().ToString();
        GameManager.Instance.OnCloseProject += OnCloseProject;
    }

    private void OnCloseProject(object sender, EventArgs e) {
        try {
            Destroy(gameObject);
        } catch (MissingReferenceException) {

        }
    }

    protected virtual void Update() {
        if (Name == "")
            return;
        if (gameObject.transform.hasChanged) {
            PlayerPrefsHelper.SaveVector3(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxPos/" + Name, transform.localPosition);
            PlayerPrefsHelper.SaveQuaternion(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxRot/" + Name, transform.localRotation);
            transform.hasChanged = false;
        }
    }

    public void Init(string name) {
        Name = name;
        transform.localPosition = PlayerPrefsHelper.LoadVector3(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxPos/" + Name, new Vector3());
        transform.localRotation = PlayerPrefsHelper.LoadQuaternion(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxRot/" + Name, new Quaternion());
        Vector3 dim = PlayerPrefsHelper.LoadVector3(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxDim/" + Name, new Vector3(0.5f, 0.5f, 0.5f));
        SetDimensions(dim.x, dim.y, dim.z);
        //SelectorMenu.Instance.CreateSelectorItem(this);
    }

    public void Init(string name, float x, float y, float z) {
        Name = name;
        PlayerPrefsHelper.SaveVector3(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxPos/" + Name, transform.localPosition);
        PlayerPrefsHelper.SaveQuaternion(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxRot/" + Name, transform.localRotation);
        SetDimensions(x, y, z);
        string dummyBoxes = PlayerPrefsHelper.LoadString(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxes", "");
        if (string.IsNullOrEmpty(dummyBoxes))
            dummyBoxes = name;
        else
            dummyBoxes += ";" + name;
        PlayerPrefsHelper.SaveString(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxes", dummyBoxes);
        //SelectorMenu.Instance.CreateSelectorItem(this);
    }

    public override void Rename(string newName) {
        string dummyBoxes = PlayerPrefsHelper.LoadString(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxes", "");

        if (!string.IsNullOrEmpty(dummyBoxes)) {
            List<string> boxes = dummyBoxes.Split(';').ToList();
            boxes.Remove(Name);
            boxes.Add(newName);
            PlayerPrefsHelper.SaveString(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxes", string.Join(";", boxes));
        }
        Vector3 dim = PlayerPrefsHelper.LoadVector3(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxDim/" + Name, new Vector3(0.5f, 0.5f, 0.5f));


        Name = newName;
        PlayerPrefsHelper.SaveVector3(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxPos/" + Name, transform.localPosition);
        PlayerPrefsHelper.SaveQuaternion(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxRot/" + Name, transform.localRotation);
        SetDimensions(dim.x, dim.y, dim.z);
        SelectorMenu.Instance.UpdateSelectorItem(this);
    }

    public override string GetId() {
        return id;
    }

    public override string GetName() {
        return Name;
    }


    public override bool HasMenu() {
        return false;
    }

    public override bool Movable() {
        return true;
    }

    public override void OnClick(Click type) {

    }

    public override void OnHoverEnd() {
        OutlineOnClick.UnHighlight();
    }

    public override void OnHoverStart() {
        if (!enabled)
            return;
        OutlineOnClick.Highlight();
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public void SetDimensions(Vector3 dim) {
        SetDimensions(dim.x, dim.y, dim.z);
    }

    public void SetDimensions(float x, float y, float z) {

        PlayerPrefsHelper.SaveVector3(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxDim/" + Name, new Vector3(x, y, z));
        Visual.transform.localScale = new Vector3(x, y, z);
    }

    public Vector3 GetDimensions() {
        return Visual.transform.localScale;
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }

    public override void Remove() {
        string dummyBoxes = PlayerPrefsHelper.LoadString(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxes", "");
        if (!string.IsNullOrEmpty(dummyBoxes)) {
            List<string> boxes = dummyBoxes.Split(';').ToList();
            boxes.Remove(Name);
            PlayerPrefsHelper.SaveString(Base.ProjectManager.Instance.ProjectMeta.Id + "/DummyBoxes", string.Join(";", boxes));
        }
        Destroy(gameObject);

        SelectorMenu.Instance.DestroySelectorItem(this);
    }

    public override bool Removable() {
        return true;
    }

    public GameObject GetModelCopy() {
        GameObject copy = Instantiate(ProjectManager.Instance.DummyBoxVisual, Vector3.zero, Quaternion.identity, transform);
        copy.transform.localScale = Visual.transform.localScale;
        copy.transform.localRotation = Quaternion.identity;
        copy.transform.localPosition = Vector3.zero;

        return copy;
    }

    public override void Enable(bool enable) {
        base.Enable(enable);
        if (enable)
            SelectorMenu.Instance.CreateSelectorItem(this);
        else
            SelectorMenu.Instance.DestroySelectorItem(this);
    }
}
