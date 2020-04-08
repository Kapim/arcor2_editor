using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using IO.Swagger.Model;

namespace Base {

    public class StringEventArgs : EventArgs {
        public string Data {
            get; set;
        }

        public StringEventArgs(string data) {
            Data = data;
        }
    }

    public class GameStateEventArgs : EventArgs {
        public GameManager.GameStateEnum Data {
            get; set;
        }

        public GameStateEventArgs(GameManager.GameStateEnum data) {
            Data = data;
        }
    }

    public class ProjectStateEventArgs : EventArgs {
        public IO.Swagger.Model.ProjectState Data {
            get; set;
        }

        public ProjectStateEventArgs(IO.Swagger.Model.ProjectState data) {
            Data = data;
        }
    }

    public class GameManager : Singleton<GameManager> {

        public delegate void StringEventHandler(object sender, StringEventArgs args);
        public delegate void GameStateEventHandler(object sender, GameStateEventArgs args);
        public delegate void ProjectStateEventHandler(object sender, ProjectStateEventArgs args);

        public event EventHandler OnSaveProject;
        public event EventHandler OnLoadProject;
        public event EventHandler OnLoadScene;
        public event EventHandler OnRunProject;
        public event EventHandler OnStopProject;
        public event EventHandler OnPauseProject;
        public event EventHandler OnResumeProject;
        public event EventHandler OnCloseProject;
        public event EventHandler OnProjectsListChanged;
        public event EventHandler OnSceneListChanged;
        public event StringEventHandler OnConnectedToServer;
        public event StringEventHandler OnConnectingToServer;
        public event EventHandler OnDisconnectedFromServer;
        public event EventHandler OnSceneChanged;
        public event EventHandler OnActionObjectsChanged;
        public event EventHandler OnServicesChanged;
        public event GameStateEventHandler OnGameStateChanged;
        public event ProjectStateEventHandler OnProjectStateChanged;
        public event EventHandler OnOpenProjectEditor;
        public event EventHandler OnOpenSceneEditor;
        public event EventHandler OnOpenMainScreen;


        private GameStateEnum gameState;

        public GameObject LoadingScreen;
        public GameObject ButtonPrefab;
        public GameObject Tooltip;
        public TMPro.TextMeshProUGUI Text;
        private string loadedScene;

        
        private IO.Swagger.Model.Project newProject;
        public IO.Swagger.Model.Project CurrentProject = null;
        private IO.Swagger.Model.Scene newScene;
        private bool sceneReady;
        private IO.Swagger.Model.ProjectState projectState = null;

        public const string ApiVersion = "0.6.0";

        public List<IO.Swagger.Model.ListProjectsResponseData> Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
        public List<IO.Swagger.Model.IdDesc> Scenes = new List<IO.Swagger.Model.IdDesc>();

        public TMPro.TMP_Text ConnectionInfo, MessageBox, EditorInfo;

        public Image GizmoOverlay;

        public bool SceneInteractable {
            get => !MenuManager.Instance.IsAnyMenuOpened();
        }

        public enum ConnectionStatusEnum {
            Connected, Disconnected, Connecting
        }

        public enum GameStateEnum {
            Disconnected,
            MainScreen,
            SceneEditor,
            ProjectEditor,
            ProjectRunning
        }

        private ConnectionStatusEnum connectionStatus;

        public ConnectionStatusEnum ConnectionStatus {
            get => connectionStatus; set {
                if (connectionStatus != value) {
                    connectionStatus = value;
                    OnConnectionStatusChanged(connectionStatus);
                }
            }
        }

        public GameStateEnum GetGameState() {
            return gameState;
        }

        public void SetGameState(GameStateEnum value) {
            gameState = value;
            OnGameStateChanged?.Invoke(this, new GameStateEventArgs(gameState));
        }

        public void SetProjectState(IO.Swagger.Model.ProjectState state) {
            projectState = state;
            OnProjectStateChanged?.Invoke(this, new ProjectStateEventArgs(state));
        }

        public IO.Swagger.Model.ProjectState GetProjectState() {
            return projectState;
        }

        private void Awake() {
            loadedScene = "";
            sceneReady = false;
            ConnectionStatus = ConnectionStatusEnum.Disconnected;
            OpenDisconnectedScreen();
        }

        private void Start() {
            Scene.Instance.gameObject.SetActive(false);
            ActionsManager.Instance.OnActionsLoaded += OnActionsLoaded;
            OnLoadProject += ProjectLoaded;
            OnLoadScene += SceneLoaded;
            EndLoading(); // GameManager is executed after all other scripts, set in Edit | Project Settings | Script Execution Order
        }



        private async void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
            switch (newState) {
                case ConnectionStatusEnum.Connected:
                    
                    if (!await CheckApiVersion()) {
                        DisconnectFromSever();
                        EndLoading();
                        return;
                    }
                    
                    ConnectionInfo.text = WebsocketManager.Instance.APIDomainWS;
                    MenuManager.Instance.DisableAllMenus();
                    StartLoading();
                    Scene.Instance.gameObject.SetActive(true);
                    OnConnectedToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.APIDomainWS));
                    await UpdateActionObjects();
                    await UpdateServices();
                    try {
                        await Task.Run(() => ActionsManager.Instance.WaitUntilActionsReady(15000));
                    } catch (TimeoutException e) {
                        Notifications.Instance.ShowNotification("Connection failed", "Some actions were not loaded within timeout");
                        DisconnectFromSever();
                        ActionsManager.Instance.Init();
                        return;
                    }
                    if (newScene != null) {
                        SceneUpdated(newScene);
                    }
                    if (sceneReady && CurrentProject == null) {
                        OnLoadScene?.Invoke(this, EventArgs.Empty);
                    } else if (sceneReady && CurrentProject != null && GetProjectState().State == IO.Swagger.Model.ProjectState.StateEnum.Stopped) {
                        OnLoadProject?.Invoke(this, EventArgs.Empty);
                    } else if (!sceneReady && CurrentProject == null) {
                        await OpenMainScreen();
                    } else {
                        OpenProjectRunningScreen();
                    }
                    EndLoading();
                    break;
                case ConnectionStatusEnum.Disconnected:
                    OpenDisconnectedScreen();
                    ConnectionInfo.text = "Not connected";
                    OnDisconnectedFromServer?.Invoke(this, EventArgs.Empty);
                    Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
                    Scenes = new List<IO.Swagger.Model.IdDesc>();

                    CurrentProject = null;
                    loadedScene = "";
                    ProjectUpdated(null);
                    SceneUpdated(null);
                    Scene.Instance.gameObject.SetActive(false);
                    Init();
                    break;
            }
        }

        public void StartLoading() {
            Debug.Assert(LoadingScreen != null);
            LoadingScreen.SetActive(true);
        }

        public void EndLoading() {
            Debug.Assert(LoadingScreen != null);
            LoadingScreen.SetActive(false);
        }

        private void Init() {

        }

        public async void ConnectToSever(string domain, int port) {
            StartLoading();
            OnConnectingToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.GetWSURI(domain, port)));
            if (await WebsocketManager.Instance.ConnectToServer(domain, port)) {
                try {
                    await Task.Run(() => WebsocketManager.Instance.WaitForInitData(5000));
                } catch (TimeoutException e) {
                    Notifications.Instance.ShowNotification("Connection failed", "Connected but failed to fetch required data (scene, project, projectstate)");
                    WebsocketManager.Instance.DisconnectFromSever();
                    EndLoading();
                }                
                ConnectionStatus = GameManager.ConnectionStatusEnum.Connected;
            } else {
                ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
                Notifications.Instance.ShowNotification("Connection failed", "Failed to connect to remote server. Is it running?");
            }

        }

        public void DisconnectFromSever() {
            WebsocketManager.Instance.DisconnectFromSever();
        }

        private void OnActionsLoaded(object sender, EventArgs e) {
            EndLoading();
            MenuManager.Instance.EnableAllWindows();
        }

        public async Task UpdateActionObjects(string highlighteObject = null) {
            List<IO.Swagger.Model.ObjectTypeMeta> objectTypeMetas = await WebsocketManager.Instance.GetObjectTypes();
            await ActionsManager.Instance.UpdateObjects(objectTypeMetas, highlighteObject);
        }

        public async Task UpdateServices() {
            await ActionsManager.Instance.UpdateServicesMetadata(await WebsocketManager.Instance.GetServices());
        }

        /// <summary>
        /// Sends request to the server to create a new Action Object of user specified type and id. Id has to be generated here in the client.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<IO.Swagger.Model.AddObjectToSceneResponse> AddObjectToScene(string type, string name) {
            StartLoading();
            IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(new Vector3(0, 0, 0)), orientation: new IO.Swagger.Model.Orientation(1, 0, 0, 0));
            EndLoading();
            return await WebsocketManager.Instance.AddObjectToScene(name, type, pose);
        }

        public async Task<IO.Swagger.Model.AutoAddObjectToSceneResponse> AutoAddObjectToScene(string type) {
            return await WebsocketManager.Instance.AutoAddObjectToScene(type);
        }

        public async void AddServiceToScene(string type, string configId = "") {
            StartLoading();
            IO.Swagger.Model.SceneService sceneService = new IO.Swagger.Model.SceneService(type: type, configurationId: configId);
            try {
                await WebsocketManager.Instance.AddServiceToScene(sceneService: sceneService);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Add service failed", e.Message);
            } finally {
                EndLoading();
            }            
            
        }

        public void SceneAdded(IO.Swagger.Model.Scene scene) {
            SceneUpdated(scene);
        }

        
        public async void SceneBaseUpdated(IO.Swagger.Model.Scene scene) {
            if (GetGameState() == GameStateEnum.SceneEditor)
                Scene.Instance.SceneBaseUpdated(scene);
            else if (GetGameState() == GameStateEnum.MainScreen) {
                await LoadScenes();
            }
        }

        // SceneUpdated is called from server, when another GUI makes some change.
        public void SceneUpdated(IO.Swagger.Model.Scene scene) {
            StartLoading();
            sceneReady = false;
            newScene = null;
            if (scene == null) {
                Scene.Instance.RemoveActionObjects();
                EndLoading();
                return;
            }
            if (!ActionsManager.Instance.ActionsReady) {
                newScene = scene;
                return;
            }

            // Set current loaded swagger scene
            if (Scene.Instance.Data == null) {
                Scene.Instance.Data = scene;
                OpenSceneEditor();
            } else {
                Scene.Instance.Data = scene;
            }
            
            

            // if another scene was loaded, remove everything from current scene
            if (loadedScene != scene.Id) {
                Scene.Instance.RemoveActionObjects();
                loadedScene = scene.Id;
                if (loadedScene != null) {
                    Scene.Instance.ActionObjectsVisible = LoadBool("scene/" + loadedScene + "/AOVisibility", true);
                    Scene.Instance.ActionObjectsInteractive = LoadBool("scene/" + loadedScene + "/AOInteractivity", true);
                }
            }

            Scene.Instance.UpdateActionObjects();
            Scene.Instance.UpdateServices();

            sceneReady = true;
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
            
            if (newProject != null) {
                ProjectUpdated(newProject);
            }
            EndLoading();
        }

      

        public void ActionUpdated(IO.Swagger.Model.Action projectAction) {
            Base.Action action = Scene.Instance.GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdate(projectAction);
        }

        public void ActionBaseUpdated(IO.Swagger.Model.Action projectAction) {
            Base.Action action = Scene.Instance.GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdateBaseData(projectAction);
        }

        public async void ActionAdded(IO.Swagger.Model.Action projectAction, string parentId) {
            ActionPoint actionPoint = Scene.Instance.GetActionPoint(parentId);
            IActionProvider actionProvider = Scene.Instance.GetActionProvider(Action.ParseActionType(projectAction.Type).Item1);
            Base.Action action = await Scene.Instance.SpawnAction(projectAction.Id, projectAction.Name, Action.ParseActionType(projectAction.Type).Item2, actionPoint, actionProvider);
            // updates name of the action
            action.ActionUpdateBaseData(projectAction);
            // updates parameters of the action
            action.ActionUpdate(projectAction);
        }


        public void ActionRemoved(IO.Swagger.Model.Action action) {
            Scene.Instance.RemoveAction(action.Id);
        }


        public async void ActionPointUpdated(ProjectActionPoint projectActionPoint) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPoint(projectActionPoint.Id);
                await actionPoint.UpdateActions(projectActionPoint);
                // TODO - update orientations, joints etc.
            } catch (KeyNotFoundException ex) {
                Debug.Log("Action point " + projectActionPoint.Id + " not found!");
                Notifications.Instance.ShowNotification("", "Action point " + projectActionPoint.Id + " not found!");
                return;
            }
        }

        public void ActionPointBaseUpdated(ProjectActionPoint projectActionPoint) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPoint(projectActionPoint.Id);
                actionPoint.ActionPointBaseUpdate(projectActionPoint);
            } catch (KeyNotFoundException ex) {
                Debug.Log("Action point " + projectActionPoint.Id + " not found!");
                Notifications.Instance.ShowNotification("", "Action point " + projectActionPoint.Id + " not found!");
                return;
            }
        }

        public void ActionPointAdded(ProjectActionPoint projectActionPoint) {
            IActionPointParent actionPointParent = Scene.Instance.GetActionPointParent(projectActionPoint.Parent);
            if (actionPointParent != null) {
                Scene.Instance.SpawnActionPoint(projectActionPoint, actionPointParent);
            } else {
                Debug.LogError("Parent " + projectActionPoint.Name + "(" + projectActionPoint.Id + ") not found");
            }
        }


        public void ActionPointRemoved(ProjectActionPoint projectActionPoint) {
            Scene.Instance.RemoveActionPoint(projectActionPoint.Id);
        }


        public void SceneObjectUpdated(SceneObject sceneObject) {
            ActionObject actionObject = Scene.Instance.GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                actionObject.ActionObjectUpdate(sceneObject, Scene.Instance.ActionObjectsVisible, Scene.Instance.ActionObjectsInteractive);
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
        }

        public void SceneObjectBaseUpdated(SceneObject sceneObject) {
            ActionObject actionObject = Scene.Instance.GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
        }

        public void SceneObjectAdded(SceneObject sceneObject) {
            ActionObject actionObject = Scene.Instance.SpawnActionObject(sceneObject.Id, sceneObject.Type, false, sceneObject.Name);
            actionObject.ActionObjectUpdate(sceneObject, Scene.Instance.ActionObjectsVisible, Scene.Instance.ActionObjectsInteractive);
        }


        public void SceneObjectRemoved(SceneObject sceneObject) {
            ActionObject actionObject = Scene.Instance.GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                Scene.Instance.ActionObjects.Remove(sceneObject.Id);
                Destroy(actionObject.gameObject);
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }            
        }


        public void ActionPointOrientationUpdated(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPointWithOrientation(orientation.Id);
                actionPoint.UpdateOrientation(orientation); 
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointOrientationBaseUpdated(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPointWithOrientation(orientation.Id);
                actionPoint.BaseUpdateOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointOrientationAdded(NamedOrientation orientation, string actionPointIt) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPoint(actionPointIt);
                actionPoint.AddOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointOrientationRemoved(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPointWithOrientation(orientation.Id);
                actionPoint.RemoveOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to remove action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointJointsUpdated(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPointWithJoints(joints.Id);
                actionPoint.UpdateJoints(joints); 
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point joints", ex.Message);
                return;
            }
        }

        public void ActionPointJointsBaseUpdated(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPointWithJoints(joints.Id);
                actionPoint.BaseUpdateJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point joints", ex.Message);
                return;
            }
        }

        public void ActionPointJointsAdded(ProjectRobotJoints joints, string actionPointIt) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPoint(actionPointIt);
                actionPoint.AddJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add action point joints", ex.Message);
                return;
            }
        }


        public void ActionPointJointsRemoved(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = Scene.Instance.GetActionPointWithJoints(joints.Id);
                actionPoint.RemoveJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to remove action point joints", ex.Message);
                return;
            }
        }


        // ProjectUpdated is called from server, when another GUI makes some changes
        public void ProjectUpdated(IO.Swagger.Model.Project project) {
            StartLoading();
            if (project == null) {
                CurrentProject = null;
                Scene.Instance.RemoveActionPoints();
                EndLoading();
                return;
            }

            if (project.SceneId != loadedScene || !sceneReady) {
                newProject = project;
                return;
            }

            newProject = null;

            CurrentProject = project;

            Scene.Instance.UpdateActionPoints(CurrentProject);
            
            EndLoading();
        }

        

        public string GetSceneId(string name) {
            foreach (IdDesc scene in Scenes) {
                if (name == scene.Name)
                    return scene.Id;
            }
            throw new RequestFailedException("No scene with name: " + name);
        }



        // UpdateProject updates opened project on the server.
        public void UpdateProject() {
            /*if (CurrentProject == null)
                return;
            CurrentProject.Objects.Clear();
            CurrentProject.SceneId = Scene.Instance.Data.Id;
            foreach (ActionObject actionObject in Scene.Instance.ActionObjects.Values) {
                IO.Swagger.Model.ProjectObject projectObject = DataHelper.SceneObjectToProjectObject(actionObject.Data);
                foreach (ActionPoint actionPoint in actionObject.ActionPoints.Values) {
                    actionPoint.UpdatePositionsOfPucks();
                    IO.Swagger.Model.ProjectActionPoint projectActionPoint = actionPoint.Data;
                    projectActionPoint.Actions.Clear();
                    foreach (Action action in actionPoint.Actions.Values) {
                        IO.Swagger.Model.Action projectAction = action.Data;
                        projectAction.Parameters = new List<IO.Swagger.Model.ActionParameter>();
                        foreach (ActionParameter parameter in action.Parameters.Values) {
                            IO.Swagger.Model.ActionParameter projectParameter = parameter;
                            projectAction.Parameters.Add(projectParameter);
                        }

                        // TODO Discuss and solve multiple inputs/outputs possibility in Action (currently only 1 input and 1 output)
                        projectAction.Inputs = new List<IO.Swagger.Model.ActionIO>();
                        projectAction.Outputs = new List<IO.Swagger.Model.ActionIO>();

                        projectAction.Inputs.Add(action.Input.Data);
                        projectAction.Outputs.Add(action.Output.Data);

                        projectActionPoint.Actions.Add(projectAction);
                    }
                    projectObject.ActionPoints.Add(projectActionPoint);
                }
                CurrentProject.Objects.Add(projectObject);
            }

            WebsocketManager.Instance.UpdateProject(CurrentProject);*/
        }

        public async Task LoadScenes() {
            Scenes = await WebsocketManager.Instance.LoadScenes();
            OnSceneListChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task LoadProjects() {
            Projects = await WebsocketManager.Instance.LoadProjects();
            OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<IO.Swagger.Model.SaveSceneResponse> SaveScene() {
            IO.Swagger.Model.SaveSceneResponse response = await WebsocketManager.Instance.SaveScene();
            return response;
        }

        public async Task<IO.Swagger.Model.SaveProjectResponse> SaveProject() {
            IO.Swagger.Model.SaveProjectResponse response = await WebsocketManager.Instance.SaveProject();
            OnSaveProject?.Invoke(this, EventArgs.Empty);
            return response;
        }

        public async void OpenProject(string id) {
            StartLoading();
            try {
                await WebsocketManager.Instance.OpenProject(id);
                await Task.Run(() => WaitForProjectReady(5000));
                OnLoadProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to open project", ex.Message);
            } catch (TimeoutException e) {
                Notifications.Instance.ShowNotification("Open project failed", "Failed to load project");
            } finally {
                EndLoading();
            }
        }

        public async Task OpenScene(string id) {
            try {
                await WebsocketManager.Instance.OpenScene(id);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Open scene failed", e.Message);
                return;
            }    
            try {
                await Task.Run(() => WaitForSceneReady(2000));
                OnLoadScene?.Invoke(this, EventArgs.Empty);
                return;
            } catch (TimeoutException e) {
                EndLoading();
                Notifications.Instance.ShowNotification("Open scene failed", "Failed to open selected scene");
            }
           
        }

        public async void RunProject() {
            if (CurrentProject == null)
                return;
            try {
                await WebsocketManager.Instance.BuildProject(CurrentProject.Id);
                await WebsocketManager.Instance.RunProject(CurrentProject.Id);
                OpenProjectRunningScreen();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to run project", ex.Message);
            }
        }

        public async void StopProject() {
            try {
                await WebsocketManager.Instance.StopProject();
                OnStopProject?.Invoke(this, EventArgs.Empty);
                OpenProjectEditor();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to stop project", ex.Message);
            }
        }

        public async void PauseProject() {
            try {
                await WebsocketManager.Instance.PauseProject();
                OnPauseProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to pause project", ex.Message);
            }
        }


        public async void ResumeProject() {
            try {
                await WebsocketManager.Instance.ResumeProject();
                OnResumeProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to resume project", ex.Message);
            }
        }


        public async Task<bool> CreateNewObjectType(IO.Swagger.Model.ObjectTypeMeta objectType) {
            try {
                await WebsocketManager.Instance.CreateNewObjectType(objectType);
                await UpdateActionObjects(objectType.Type);
                return true;
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to create new object type", ex.Message);
                return false;
            }
        }

        public void ExitApp() => Application.Quit();

        public async void UpdateActionPointPositionUsingRobot(string actionPointId, string robotId, string endEffectorId) {

            try {
                await WebsocketManager.Instance.UpdateActionPointUsingRobot(actionPointId, robotId, endEffectorId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }

        public async void UpdateActionPointOrientationUsingRobot(string actionPointId, string robotId, string endEffectorId, string orientationId) {

            try {
                await WebsocketManager.Instance.UpdateActionPointOrientationUsingRobot(actionPointId, robotId, endEffectorId, orientationId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }

        public async void UpdateActionPointJoints(string robotId, string jointsId) {

            try {
                await WebsocketManager.Instance.UpdateActionPointJoints(robotId, jointsId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }

        public async void UpdateActionObjectPosition(string actionObjectId, string robotId, string endEffectorId) {

            try {
                await WebsocketManager.Instance.UpdateActionObjectPosition(actionObjectId, robotId, endEffectorId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action object", ex.Message);
            }
        }

        
        public async Task StartObjectFocusing(string objectId, string robotId, string endEffector) {
            await WebsocketManager.Instance.StartObjectFocusing(objectId, robotId, endEffector);
        }

        public async Task SavePosition(string objectId, int pointIdx) {
            await WebsocketManager.Instance.SavePosition(objectId, pointIdx);
        }

        public async Task FocusObjectDone(string objectId) {
            await WebsocketManager.Instance.FocusObjectDone(objectId);
        }

        public async Task NewProject(string name, string sceneId, bool generateLogic) {
            Debug.Assert(sceneId != null && sceneId != "");
            Debug.Assert(name != null && name != "");
            StartLoading();
            
            try {
                await WebsocketManager.Instance.OpenScene(sceneId);
            } catch (RequestFailedException e) {
                EndLoading();
                Notifications.Instance.ShowNotification("Open scene failed", e.Message);
                return;
            }           
            
            try {
                await Task.Run(() => WaitForSceneReady(5000));
            } catch (TimeoutException e) {
                EndLoading();
                Notifications.Instance.ShowNotification("Open scene failed", "Scene " + sceneId + " could not be loaded (unknown reason).");
                return;
            }
            //IO.Swagger.Model.Project project = new IO.Swagger.Model.Project(id: Guid.NewGuid().ToString(), name: name, objects: new List<IO.Swagger.Model.ProjectObject>(), sceneId: sceneId, hasLogic: generateLogic);
            //WebsocketManager.Instance.UpdateProject(project);
            //ProjectUpdated(project);
            try {
                await WebsocketManager.Instance.CreateProject(name, sceneId, "");
                OnLoadProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to create project", e.Message);
            } finally {
                EndLoading();
            }
            
        }


        public async Task<bool> NewScene(string name) {
            if (name == "") {
                Notifications.Instance.ShowNotification("Failed to create new scene", "Scane name to defined");
                return false;
            }
            try {
                await WebsocketManager.Instance.CreateScene(name, "");
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to create new scene", e.Message);
            }
            //OnLoadScene?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public async Task<RemoveFromSceneResponse> RemoveFromScene(string id) {
            return await WebsocketManager.Instance.RemoveFromScene(id, false);
        }

        public async Task<bool> CloseScene(bool force) {
            loadedScene = "";
            bool success = await WebsocketManager.Instance.CloseScene(force);
            if (success) {
                OpenMainScreen();
                Scene.Instance.Data = null;
            }                
            return success;
        }

        public async Task<bool> CloseProject(bool force) {
            loadedScene = "";
            bool success = await WebsocketManager.Instance.CloseProject(force);
            if (success) {
                OpenMainScreen();
                OnCloseProject?.Invoke(this, EventArgs.Empty);
                Scene.Instance.Data = null;
            }
            return success;
            //ProjectUpdated(null);
            //CloseScene();
            
        }

        public async Task<List<ObjectAction>> GetActions(string name) {
            try {
                return await WebsocketManager.Instance.GetActions(name);
            } catch (RequestFailedException e) {
                Debug.LogError("Failed to load action for object/service " + name);
                Notifications.Instance.ShowNotification("Failed to laod actions", "Failed to load action for object/service " + name);
                return null;
            }

        }

        public async Task<List<string>> GetActionParamValues(string actionProviderId, string param_id, List<IO.Swagger.Model.IdValue> parent_params) {
            return await WebsocketManager.Instance.GetActionParamValues(actionProviderId, param_id, parent_params);
        }

        public async Task<bool> ExecuteAction(string actionId) {
            try {
                await WebsocketManager.Instance.ExecuteAction(actionId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to execute action", ex.Message);
                return false;
            }
            return true;
        }

        public int GetMajorVersion(string versionString) {
            return int.Parse(SplitVersionString(versionString)[0]);
        }

        public int GetMinorVersion(string versionString) {
            return int.Parse(SplitVersionString(versionString)[1]);
        }

        public int GetPatchVersion(string versionString) {
            return int.Parse(SplitVersionString(versionString)[2]);
        }

        public List<string> SplitVersionString(string versionString) {
            List<string> version = versionString.Split('.').ToList<string>();
            Debug.Assert(version.Count == 3, versionString);
            return version;
        }

        public async Task<bool> CheckApiVersion() {
            IO.Swagger.Model.SystemInfoData systemInfo = await WebsocketManager.Instance.GetSystemInfo();
            if (systemInfo.ApiVersion == ApiVersion)
                return true;
            if (GetMajorVersion(systemInfo.ApiVersion) != GetMajorVersion(ApiVersion) || GetMinorVersion(systemInfo.ApiVersion) != GetMinorVersion(ApiVersion)) {
                Notifications.Instance.ShowNotification("Incompatibile api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion);
                return false;
            }
            if (GetPatchVersion(systemInfo.ApiVersion) != GetPatchVersion(ApiVersion)) {
                Notifications.Instance.ShowNotification("Different api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion + ". It can casuse problems, you have been warned.");
                return true;
            }
            
            return false;
        }

        public void WaitForSceneReady(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (!sceneReady) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(100);
            }
            return;
        }

        public void WaitForProjectReady(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (CurrentProject == null) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(100);
            }
            return;
        }

        public async Task OpenMainScreen() {
            StartLoading();
            await LoadScenes();
            await LoadProjects();
            OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
            SetGameState(GameStateEnum.MainScreen);
            OnOpenMainScreen?.Invoke(this, EventArgs.Empty);
            EditorInfo.text = "";
            EndLoading();
        }

        public void OpenSceneEditor() {
            EditorInfo.text = "Scene: " + Scene.Instance.Data.Name;
            SetGameState(GameStateEnum.SceneEditor);
            OnOpenSceneEditor?.Invoke(this, EventArgs.Empty);
        }

        public void OpenProjectEditor() {
            EditorInfo.text = "Project: " + CurrentProject.Name;
            SetGameState(GameStateEnum.ProjectEditor);
            OnOpenProjectEditor?.Invoke(this, EventArgs.Empty);
        }

        public void OpenProjectRunningScreen() {
            EditorInfo.text = "Running: " + CurrentProject.Id;
            SetGameState(GameStateEnum.ProjectRunning);
            OnRunProject?.Invoke(this, EventArgs.Empty);            
        }

        public void OpenDisconnectedScreen() {
            SetGameState(GameStateEnum.Disconnected);
            EditorInfo.text = "";
        }

        public void ProjectLoaded(object sender, EventArgs eventArgs) {
            OpenProjectEditor();
        }

        public void SceneLoaded(object sender, EventArgs eventArgs) {
            OpenSceneEditor();
        }

        public void ActivateGizmoOverlay(bool activate) {
            GizmoOverlay.raycastTarget = activate;
        }

        public void SaveFloat(string key, float value) {
            PlayerPrefs.SetFloat(key, value);
        }

        public float LoadFloat(string key, float defaultValue) {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SaveBool(string key, bool value) {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public bool LoadBool(string key, bool defaultValue) {
            int value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
            return value == 1 ? true : false;
        }

        public Button CreateButton(Transform parent, string label) {
            GameObject btnGO = Instantiate(Base.GameManager.Instance.ButtonPrefab, parent);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = label;
            return btn;
        }

        public async Task<bool> RenameActionObject(string id, string newUserId) {
            try {
                await WebsocketManager.Instance.RenameObject(id, newUserId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to rename object", e.Message);
                return false;
            }
        }
         public async Task<bool> RenameScene(string id, string newUserId) {
            try {
                await WebsocketManager.Instance.RenameScene(id, newUserId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to rename scene", e.Message);
                return false;
            }
        }
        public async Task<bool> RenameProject(string id, string newUserId) {
            try {
                await WebsocketManager.Instance.RenameProject(id, newUserId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to rename project", e.Message);
                return false;
            }
        }

        public async Task<bool> AddActionPoint(string name, string parent, Position position) {
            try {
                await WebsocketManager.Instance.AddActionPoint(name, parent, position);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> RenameActionPoint(ActionPoint actionPoint, string newUserId) {
            try {
                await WebsocketManager.Instance.UpdateActionPoint(actionPoint.Data.Id, null, null, newUserId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }
        public async Task<bool> UpdateActionPointParent(ActionPoint actionPoint, string parentId) {
            try {
                await WebsocketManager.Instance.UpdateActionPoint(actionPoint.Data.Id, parentId, null, null);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateActionPointPosition(ActionPoint actionPoint, Position newPosition) {
            try {
                await WebsocketManager.Instance.UpdateActionPoint(actionPoint.Data.Id, null, newPosition, null);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> AddActionPointOrientation(ActionPoint actionPoint, string orientationId) {
            try {
                await WebsocketManager.Instance.AddActionPointOrientation(actionPoint.Data.Id, new Orientation(), orientationId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }


        public async Task<bool> AddActionPointJoints(ActionPoint actionPoint, string jointsId, string robotId) {
            try {
                await WebsocketManager.Instance.AddActionPointJoints(actionPoint.Data.Id, robotId, jointsId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }
        
        public async Task<bool> AddAction(string actionPointId, List<IO.Swagger.Model.ActionParameter> actionParameters, string type, string name) {
            try {
                await WebsocketManager.Instance.AddAction(actionPointId, actionParameters, type, name);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action", e.Message);
                return false;
            }
        }

        public async Task<bool> RenameAction(string actionId, string newName) {
            try {
                await WebsocketManager.Instance.RenameAction(actionId, newName);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to rename action", e.Message);
                return false;
            }
        }


        public async Task<bool> RemoveAction(string actionId) {
            try {
                await WebsocketManager.Instance.RemoveAction(actionId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> RemoveActionPoint(string actionPointId) {
            try {
                await WebsocketManager.Instance.RemoveActionPoint(actionPointId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        

    }    

}
