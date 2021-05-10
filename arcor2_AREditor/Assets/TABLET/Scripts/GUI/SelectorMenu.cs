using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using System;
using System.Linq;
using static Base.GameManager;
using TMPro;
using UnityEngine.Events;
using System.Threading.Tasks;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SelectorMenu : Singleton<SelectorMenu> {
    public GameObject SelectorItemPrefab;

    public CanvasGroup CanvasGroup;
    public GameObject ContentAim, ContentAlphabet, ContentNoPose, ContainerAim, ContainerAlphabet, BottomBtns,
        APHeader, APList;
    private List<SelectorItem> selectorItemsAimMenu = new List<SelectorItem>();
    private List<SelectorItem> selectorItemsNoPoseMenu = new List<SelectorItem>();
    private List<SelectorItem> selectorItemsAPDetail = new List<SelectorItem>();
    public event AREditorEventArgs.InteractiveObjectEventHandler OnObjectSelectedChangedEvent;

    public Dictionary<string, SelectorItem> SelectorItems = new Dictionary<string, SelectorItem>();

    public bool ManuallySelected;

    private long iteration = 0;

    private EditorStateEnum editorState = EditorStateEnum.Normal;

    private bool requestingObject = false;

    public bool Active;

    public ToggleIconButton RobotsToggle, ObjectsToggle, PointsToggle, ActionsToggle, IOToggle, OthersToggle;
    private SelectorItem lastSelectedItem = null;

    private void Start() {
        GameManager.Instance.OnCloseProject += OnCloseProjectScene;
        GameManager.Instance.OnCloseScene += OnCloseProjectScene;
        SceneManager.Instance.OnSceneChanged += OnSceneChanged;
        GameManager.Instance.OnOpenSceneEditor += OnSceneChanged;
        ProjectManager.Instance.OnProjectChanged += OnProjectChanged;
        GameManager.Instance.OnOpenProjectEditor += OnProjectChanged;
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        ProjectManager.Instance.OnLoadProject += OnLoadProjectScene;
        SceneManager.Instance.OnLoadScene += OnLoadProjectScene;
        GameManager.Instance.OnRunPackage += OnLoadProjectScene;
        Active = true;
        gameObject.SetActive(false);
    }

    private void OnLoadProjectScene(object sender, EventArgs e) {
        _ = UpdateFilters();
    }

    public async Task UpdateFilters() {
        await ShowRobots(RobotsToggle.Toggled, false);
        ShowActionObjects(ObjectsToggle.Toggled, false);
        ShowActionPoints(PointsToggle.Toggled, false);
        //ShowActions(ActionsToggle.Toggled, false);
        ShowIO(IOToggle.Toggled, false);
        ShowOthers(true, false);
    }

    private void OnEditorStateChanged(object sender, EditorStateEventArgs args) {
        editorState = args.Data;
        switch (editorState) {
            case EditorStateEnum.Normal:
                DeselectObject(true);
                requestingObject = false;
                Active = true;
                break;
            case EditorStateEnum.Closed:
            case EditorStateEnum.InteractionDisabled:
                DeselectObject(true);
                requestingObject = false;
                Active = false;
                break;
            case EditorStateEnum.SelectingAction:
            case EditorStateEnum.SelectingActionInput:
            case EditorStateEnum.SelectingActionObject:
            case EditorStateEnum.SelectingActionOutput:
            case EditorStateEnum.SelectingActionPoint:
            case EditorStateEnum.SelectingActionPointParent:
                DeselectObject(true);
                requestingObject = true;
                Active = true;
                break;
        }
    }

    private void OnCloseProjectScene(object sender, System.EventArgs e) {
        foreach (SelectorItem selectorItem in SelectorItems.Values) {
            Destroy(selectorItem.transform.parent.gameObject);
        }
        selectorItemsAimMenu.Clear();
        SelectorItems.Clear();
        selectorItemsNoPoseMenu.Clear();
        ManuallySelected = false;
    }

    private class SelectorItemComparer : IComparer<SelectorItem> {
        public int Compare(SelectorItem x, SelectorItem y) {
            return x.Score.CompareTo(y.Score);
        }
    }


    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
    }


    private void Update() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen ||
            GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected ||
            MenuManager.Instance.IsAnyMenuOpened) {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 0;
        } else {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.alpha = 1;
        }

    }
    /*
    public void ForceUpdateMenus() {
        UpdateAlphabetMenu();
        if (ContentNoPose.activeSelf)
            UpdateNoPoseMenu();
    }*/

    private void OnProjectChanged(object sender, System.EventArgs e) {
        //UpdateAlphabetMenu();
        UpdateNoPoseMenu();
    }

    private void OnSceneChanged(object sender, System.EventArgs e) {
        //UpdateAlphabetMenu();
        UpdateNoPoseMenu();
    }

    private void SelectedObjectChanged(SelectorItem selectorItem, bool force = false) {
        if (force || selectorItem != lastSelectedItem) {
            OnObjectSelectedChangedEvent?.Invoke(this, new InteractiveObjectEventArgs(selectorItem == null ? null : selectorItem.InteractiveObject));
            if (selectorItem == null)
                DeselectObject(false);
            lastSelectedItem = selectorItem;

            return;
            if (selectorItem == null || ((RectTransform) ContentAim.transform).rect.height <= 760)
                return;

            GameObject content;
            if (ContainerAim.activeSelf) {
                content = ContentAim;
            } else if (ContainerAlphabet.activeSelf) {
                content = ContentAlphabet;
            } else {
                return;
            }
            int yPos = (int) content.transform.localPosition.y - 380;
            int position = (int) (0 - selectorItem.transform.parent.localPosition.y);
            if (selectorItem.InteractiveObject.GetType() == typeof(Action3D)) {
                position += (int) (0 - selectorItem.transform.parent.parent.parent.localPosition.y) + 120;
            }
            Debug.LogError(position + " - " + yPos);
            Debug.LogError(Math.Abs(position - (yPos + 760)));
            if (position < yPos ||
                position > yPos + 760 ||
                Math.Abs(position - (yPos + 760)) < 60) {
                content.transform.localPosition = new Vector3(content.transform.localPosition.x, position, content.transform.localPosition.z);
            }
            /// TODO!!!!!!!!!!!!!!!! vypnout inerciu před přesunem a obnovit po něm
        }
    }

    private SelectorItem GetSelectorItem(InteractiveObject io) {
        foreach (SelectorItem item in selectorItemsAimMenu) {
            if (string.Compare(item.InteractiveObject.GetId(), io.GetId()) == 0) {
                return item;
            }
        }
        return null;
    }

    private void RemoveItemWithId(string id, List<SelectorItem> selectorItems) {
        for (int i = selectorItems.Count - 1; i >= 0; --i) {
            if (selectorItems[i].InteractiveObject.GetId() == id) {
                selectorItems.RemoveAt(i);
            }
        }
    }

    private void PrintItems() {
        Debug.Log(selectorItemsAimMenu.Count);
        foreach (SelectorItem item in selectorItemsAimMenu) {
            Debug.LogError(item.InteractiveObject.GetName() + ": " + item.Score + " (" + item.InteractiveObject.GetId() + ")");
        }
    }

    //public void UpdateAimMenu(Vector3? aimingPoint) {
    public void UpdateAimMenu(List<Tuple<float, InteractiveObject>> items) {
        /*
        if (aimingPoint.HasValue) {
            foreach (SelectorItem item in selectorItems.Values) {
                try {
                    if (item.InteractiveObject == null)
                        continue;
                    float dist = item.InteractiveObject.GetDistance(aimingPoint.Value);
                    if (item.InteractiveObject is ActionObjectNoPose || (item.Collapsed && dist > 0.2)) // add objects max 20cm away from point of impact
                        continue;
                    items.Add(new Tuple<float, InteractiveObject>(dist, item.InteractiveObject));
                } catch (MissingReferenceException ex) {
                    Debug.LogError(ex);
                }
            }
            //items.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        } */
        if (ContainerAim.activeSelf) {
            int itemsWithoutActions = 10;
            foreach (Tuple<float, InteractiveObject> item in items) {
                if (item.Item2.GetType() == typeof(ActionObjectNoPose))
                    continue;
                if (true) { //selectorItemsAimMenu.Count < 6 || item.Item1 <= selectorItemsAimMenu.Last().Score) {
                    if (!SelectorItems.ContainsKey(item.Item2.GetId())) {
                        continue;
                    }
                    SelectorItem selectorItem = GetSelectorItem(item.Item2);
                    if (selectorItem == null) {
                        selectorItem = SelectorItems[item.Item2.GetId()];
                        selectorItemsAimMenu.Add(selectorItem);
                        if (selectorItem.InteractiveObject.GetType() != typeof(Action3D))
                            AddItemToAimingList(selectorItem);
                    } else {
                        if (selectorItem.transform.parent.parent != ContentAim.transform) {
                            if (selectorItem.InteractiveObject.GetType() != typeof(Action3D))
                                AddItemToAimingList(selectorItem);
                        }
                    }
                    selectorItem.UpdateScore(item.Item1, iteration);
                    if (selectorItem.InteractiveObject is ActionPoint3D actionPoint && !selectorItem.Collapsed) {
                        itemsWithoutActions += actionPoint.Actions.Count;
                    }
                }

            }

            ++iteration;
            selectorItemsAimMenu.Sort(new SelectorItemComparer());
            for (int i = selectorItemsAimMenu.Count - 1; i >= 0; --i) {
                /*if (selectorItemsAimMenu.Count < itemsWithoutActions) {
                    break;
                }*/
                /*if (!(selectorItemsAimMenu[i].IsSelected() && manuallySelected) && selectorItemsAimMenu[i].Collapsed &&
                    (iteration - selectorItemsAimMenu[i].GetLastUpdate()) > 5 && !(selectorItemsAimMenu[i].InteractiveObject is Action3D)) {
                    selectorItemsAimMenu[i].transform.parent.SetParent(ContentAlphabet.transform);
                    selectorItemsAimMenu.RemoveAt(i);
                    continue;
                }*/ 
                if (!selectorItemsAimMenu[i].Collapsed ||
                    selectorItemsAimMenu[i].InteractiveObject.GetType() == typeof(Action3D) ||
                    selectorItemsAimMenu[i].IsSelected() && ManuallySelected) {
                    continue;
                }
                if ((iteration - selectorItemsAimMenu[i].GetLastUpdate()) > 5 ||
                    selectorItemsAimMenu.Count > itemsWithoutActions) {
                    selectorItemsAimMenu[i].transform.parent.SetParent(ContentAlphabet.transform);
                    selectorItemsAimMenu.RemoveAt(i);
                }
            }

            
        }
        if (!ManuallySelected) {
            bool selected = false;
            if (items.Count > 0) {
                if (ContainerAim.activeSelf) {
                    if (selectorItemsAimMenu.Count > 0) {
                        SetSelectedObject(selectorItemsAimMenu.First(), false);
                        selected = true;
                    }
                } else if (ContentAlphabet.activeSelf && items.Count > 0) {

                    foreach (Tuple<float, InteractiveObject> item in items) {
                        if (item.Item2.Enabled) {
                            SetSelectedObject(item.Item2, false);
                            selected = true;
                            break;
                        }
                    }

                }
            }            
            if (items.Count == 0 || !selected) {
                SelectedObjectChanged(null);
            }
        }


        /*
        if (ContainerAim.activeSelf) {
            int count = 0;
            for (int i = selectorItemsAimMenu.Count - 1; i >= 0; --i) {
                if (!(selectorItemsAimMenu[i].IsSelected() && manuallySelected) && selectorItemsAimMenu[i].Collapsed && (iteration - selectorItemsAimMenu[i].GetLastUpdate()) > 5) {
                    selectorItemsAimMenu[i].transform.parent.SetParent(ContentAlphabet.transform);
                    selectorItemsAimMenu.RemoveAt(i);
                }
            }
            List<SelectorItem> newItems = new List<SelectorItem>();
            foreach (Tuple<float, InteractiveObject> item in items) {
                if (item.Item2.GetType() == typeof(ActionObjectNoPose))
                    continue;
                if (selectorItemsAimMenu.Count < 6 || item.Item1 <= selectorItemsAimMenu.Last().Score) {
                    if (!selectorItems.ContainsKey(item.Item2.GetId())) {
                        continue;
                    }
                    SelectorItem selectorItem = GetSelectorItem(item.Item2);
                    if (selectorItem == null) {
                        selectorItem = selectorItems[item.Item2.GetId()];
                        selectorItemsAimMenu.Add(selectorItem);
                        if (!(selectorItem.InteractiveObject is Action3D))
                            AddItemToAimingList(selectorItem);
                        newItems.Add(selectorItem);
                    } else {
                        if (selectorItem.transform.parent.parent != ContentAim.transform) {
                            selectorItemsAimMenu.Add(selectorItem);
                            if (!(selectorItem.InteractiveObject is Action3D))
                                AddItemToAimingList(selectorItem);
                        }
                    }
                    selectorItem.UpdateScore(item.Item1, iteration);
                }

            }
            int maxCountWithActions = 6;
            foreach (var item in selectorItemsAimMenu) {
                if (item.InteractiveObject is ActionPoint3D actionPoint) {
                    maxCountWithActions += actionPoint.Actions.Count;
                }
            }
            selectorItemsAimMenu.Sort(new SelectorItemComparer());
            while (selectorItemsAimMenu.Count > maxCountWithActions) {
                if (!selectorItemsAimMenu.Last().Collapsed) {
                    break;
                }
                if (selectorItemsAimMenu.Last().IsSelected() && manuallySelected) {
                    if (!selectorItemsAimMenu[selectorItemsAimMenu.Count - 2].Collapsed)
                        break;
                    SelectorItem item = selectorItemsAimMenu.Last();
                    selectorItemsAimMenu.RemoveAt(selectorItemsAimMenu.Count - 1);
                    selectorItemsAimMenu.Insert(selectorItemsAimMenu.Count - 2, item);
                }

                selectorItemsAimMenu.Last().transform.parent.SetParent(ContentAlphabet.transform);
                selectorItemsAimMenu.RemoveAt(selectorItemsAimMenu.Count - 1);
            }
        }
        if (!manuallySelected) {
            bool selected = false;
            if (ContainerAim.activeSelf) {
                if (selectorItemsAimMenu.Count > 0) {
                    SetSelectedObject(selectorItemsAimMenu.First(), false);
                    selected = true;
                }
            } else if (ContentAlphabet.activeSelf && items.Count > 0) {

                foreach (Tuple<float, InteractiveObject> item in items) {
                    if (item.Item2.Enabled) {
                        SetSelectedObject(items.First().Item2, false);
                        selected = true;
                        break;
                    }
                }

            }
            if (items.Count == 0 || !selected) {
                DeselectObject(false);
            }
        }
        ++iteration;
        */
    }

    public void AddItemToAimingList(SelectorItem selectorItem) {
        /*if (selectorItem.InteractiveObject is Action3D action) {
            ActionPoint ap = action.ActionPoint;
            if (selectorItems.TryGetValue(ap.GetId(), out SelectorItem selectorItemAP)) {

            }
        }*/
        selectorItem.transform.parent.SetParent(ContentAim.transform);

    }

    private void RemoveItem(int index, List<SelectorItem> selectorItems) {
        if (selectorItems[index].IsSelected()) {
            ManuallySelected = false;
            selectorItems[index].SetSelected(false, true);
        }
        Destroy(selectorItems[index].transform.parent.gameObject);
        selectorItems.RemoveAt(index);
    }

    public void SetSelectedObject(InteractiveObject interactiveObject, bool manually = false) {
        if (SelectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem item)) {
            SetSelectedObject(item, manually);
        }
    }

    public void SetSelectedObject(SelectorItem selectorItem, bool manually = false) {
        if (manually && requestingObject) {
            GameManager.Instance.ObjectSelected(selectorItem.InteractiveObject);
        } else {
            if (manually) {
                if (selectorItem.IsSelected() && ManuallySelected) {
                    selectorItem.SetSelected(false, manually);
                    ManuallySelected = false;
                    SelectedObjectChanged(null);
                    return;
                }
            }
            DeselectObject(manually);
            selectorItem.SetSelected(true, manually);
            if (manually) {
                ManuallySelected = true;
                
            }
            SelectedObjectChanged(selectorItem, ManuallySelected);
        }
    }

    public void DeselectObject(bool manually = true) {
        if (manually) {
            ManuallySelected = false;
            
        }
        /*foreach (SelectorItem item in selectorItems.Values.ToList()) {
            item.SetSelected(false, manually);
        }*/
        
        //if (lastSelectedItem != null && selectorItems.TryGetValue(lastSelectedItem.InteractiveObject.GetId(), out SelectorItem selectorItem)) {
        if (lastSelectedItem != null)
            lastSelectedItem.SetSelected(false, manually);
        //} 
    }

    public void DestroySelectorItem(string id) {
        if (SelectorItems.TryGetValue(id, out SelectorItem selectorItem)) {
            TryRemoveFromList(selectorItem.InteractiveObject, selectorItemsAimMenu);
            TryRemoveFromList(selectorItem.InteractiveObject, selectorItemsNoPoseMenu);
            if (selectorItem.IsSelected()) {
                DeselectObject(true);
            }
            Destroy(selectorItem.transform.parent.gameObject);
            SelectorItems.Remove(id);
        }
    }

    public void DestroySelectorItem(InteractiveObject interactiveObject) {
        DestroySelectorItem(interactiveObject.GetId());
    }

    public void CreateSelectorItem(InteractiveObject interactiveObject) {
        if (string.IsNullOrEmpty(interactiveObject.GetId()))
            return;
        if (interactiveObject.GetName() == "dabap" || interactiveObject.GetName() == "dabap2")
            return;
        if (SelectorItems.ContainsKey(interactiveObject.GetId()))
            return;
        SelectorItem selectorItem = Instantiate(SelectorItemPrefab).GetComponentInChildren<SelectorItem>();
        if (interactiveObject.GetType() == typeof(Action3D)) {
            Action3D action = (Action3D) interactiveObject;
            if (SelectorItems.TryGetValue(action.ActionPoint.GetId(), out SelectorItem selectorItemAP)) {
                selectorItem.transform.parent.SetParent(selectorItemAP.SublistContent.transform);
            } else {
                selectorItem.transform.parent.SetParent(ContentAlphabet.transform);
            }
        } else {
            selectorItem.transform.parent.SetParent(ContentAlphabet.transform);
        }
        //selectorItem.gameObject.SetActive(false);
        Debug.LogError(interactiveObject.GetName());
        selectorItem.SetText(interactiveObject.GetName());
        selectorItem.SetObject(interactiveObject, 0, iteration);
        SelectorItems.Add(interactiveObject.GetId(), selectorItem);
    }

    public void UpdateSelectorItem(InteractiveObject interactiveObject) {
        if (SelectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem selectorItem)) {
            selectorItem.Label.text = interactiveObject.GetName();
        }
    }

    private void TryRemoveFromList(InteractiveObject io, List<SelectorItem> selectorItems) {
        for (int i = 0; i < selectorItems.Count; ++i) {
            if (string.Compare(selectorItems[i].InteractiveObject.GetId(), io.GetId()) == 0) {
                selectorItems.RemoveAt(i);
                return;
            }
        }
    }


    /*public void UpdateAlphabetMenu() {
        //ClearMenu(selectorItemsAlphabetMenu);
        List<string> idsToRemove = selectorItems.Keys.ToList();
        foreach (InteractiveObject io in GameManager.Instance.GetAllInteractiveObjects()) {
            if (!io.Enabled)
                continue;
            if (selectorItems.TryGetValue(io.GetId(), out SelectorItem item)) {
                item.transform.SetParent(ContentAlphabet.transform);
                item.transform.SetAsLastSibling();
                idsToRemove.Remove(io.GetId());
                if (item.InteractiveObject == null)
                    item.InteractiveObject = io;
            } else {
                SelectorItem newItem = CreateSelectorItem(io, ContentAlphabet.transform, 0);
                selectorItems.Add(io.GetId(), newItem);
            }
        }
        foreach (string id in idsToRemove) {
            if (selectorItems.TryGetValue(id, out SelectorItem item)) {
                if (item.IsSelected()) {
                    item.SetSelected(false, true);
                    if (manuallySelected) {
                        manuallySelected = false;
                    }
                }
                Destroy(item.gameObject);
                selectorItems.Remove(id);
                RemoveItemWithId(id, selectorItemsAimMenu);
            }

        }
        // force update of left menu icons
        SelectedObjectChanged(lastSelectedObject, true);

    }*/


    public void UpdateNoPoseMenu() {
        if (!ContentNoPose.activeSelf || !GameManager.Instance.Scene.activeSelf)
            return;
        selectorItemsNoPoseMenu.Clear();
        foreach (InteractiveObject interactiveObject in SceneManager.Instance.GetAllActionObjectsWithoutPose()) {
            if (!interactiveObject.Enabled)
                continue;
            if (SelectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem selectorItem)) {
                selectorItemsNoPoseMenu.Add(selectorItem);
                selectorItem.transform.parent.SetParent(ContentNoPose.transform);
            }
        }
    }

    private void ClearMenu(List<SelectorItem> selectorItems) {
        foreach (SelectorItem item in selectorItems) {
            Destroy(item.transform.parent.gameObject);
        }
        selectorItems.Clear();
    }

    public void SwitchToAim() {
        ContainerAim.SetActive(true);
        ContentNoPose.SetActive(false);
        ContainerAlphabet.SetActive(false);

        BottomBtns.SetActive(true);
        PointsToggle.Button.interactable = true;
        if (ManuallySelected) {
            InteractiveObject selectedItem = GetSelectedObject();
            foreach (SelectorItem item in selectorItemsAimMenu) {
                if (item.InteractiveObject.GetId() == selectedItem.GetId()) {

                    return;
                }
            }
            ManuallySelected = false;
            DeselectObject(true);
        }
    }

    public void SwitchToNoPose() {
        PointsToggle.Button.interactable = true;
        ContainerAim.SetActive(false);
        ContentNoPose.SetActive(true);
        ContainerAlphabet.SetActive(false);
        BottomBtns.SetActive(true);
        UpdateNoPoseMenu();
    }

    public void SwitchToAlphabet() {
        foreach (SelectorItem item in SelectorItems.Values) {
            if ((item.InteractiveObject.GetType() != typeof(Action3D)) && item.transform.parent.parent != ContentAlphabet.transform) {
                item.transform.parent.SetParent(ContentAlphabet.transform);
            }
        }
        selectorItemsAimMenu.Clear();
        selectorItemsNoPoseMenu.Clear();
        PointsToggle.Button.interactable = true;
        ContainerAim.SetActive(false);
        ContentNoPose.SetActive(false);
        ContainerAlphabet.SetActive(true);
        BottomBtns.SetActive(true);
    }

    public void SwitchToAPDetail(ActionPoint3D actionPoint) {
        //selectorItemsAPDetail.Clear();
        if (SelectorItems.TryGetValue(actionPoint.GetId(), out SelectorItem selectorItem)) {
            selectorItem.transform.parent.SetParent(APHeader.transform);
            //selectorItemsAPDetail.Add(selectorItem);
            foreach (string actionId in actionPoint.Actions.Keys) {
                if (SelectorItems.TryGetValue(actionId, out SelectorItem actionItem)) {
                    actionItem.transform.parent.SetParent(APList.transform);
                    //selectorItemsAPDetail.Add(selectorItem);
                }
            }
            PointsToggle.Button.interactable = false;
            ContainerAim.SetActive(false);
            ContentNoPose.SetActive(false);
            ContainerAlphabet.SetActive(false);
            BottomBtns.SetActive(false);
        }
        
    }

    public InteractiveObject GetSelectedObject() {
        /*
        foreach (SelectorItem item in selectorItems.Values.ToList()) {
            if (item.IsSelected())
                return item.InteractiveObject;
        }
        return null;*/
        if (lastSelectedItem == null)
            return null;
        else
            return lastSelectedItem.InteractiveObject;
    }

    public async Task ShowRobots(bool show, bool updateMenus) {
        if (SceneManager.Instance.SceneStarted)
            ProjectManager.Instance.EnableAllRobotsEE(show);
        SceneManager.Instance.EnableAllRobots(show);
    }

    public void ShowActionObjects(bool show, bool updateMenus) {
        SceneManager.Instance.EnableAllActionObjects(show, false);
    }

    public void ShowActionPoints(bool show, bool updateMenus) {
        ProjectManager.Instance.EnableAllActionPoints(show);
        ProjectManager.Instance.EnableAllOrientations(show);
    }

    public void ShowIO(bool show, bool updateMenus) {
        ProjectManager.Instance.EnableAllActionInputs(show);
        ProjectManager.Instance.EnableAllActionOutputs(show);
    }

    public void ShowActions(bool show, bool updateMenus) {
        ProjectManager.Instance.EnableAllActions(show);
    }

    public void ShowOthers(bool show, bool updateMenus) {
        GameManager.Instance.EnableServiceInteractiveObjects(show);
    }

    public void ShowRobots(bool show) {
        _ = ShowRobots(show, true);
    }

    public void ShowActionObjects(bool show) {
        ShowActionObjects(show, true);
    }

    public void ShowActionPoints(bool show) {
        ShowActionPoints(show, true);
    }

    public void ShowIO(bool show) {
        ShowIO(show, true);
    }

    public void ShowActions(bool show) {
        ShowActions(show, true);
    }

    public void ShowOthers(bool show) {
        ShowOthers(show, true);
    }

}
