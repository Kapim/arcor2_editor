using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Base {
    public class Scene : Singleton<Scene> {

        public IO.Swagger.Model.Scene Data = null;
        
        // string == IO.Swagger.Model.Scene Data.Id
        public Dictionary<string, ActionObject> ActionObjects = new Dictionary<string, ActionObject>();
        public Dictionary<string, ActionPoint> ActionPoints = new Dictionary<string, ActionPoint>();
        public GameObject ActionObjectsSpawn, ActionPointsOrigin;

        public GameObject ConnectionPrefab, ActionPointPrefab, PuckPrefab;
        public GameObject RobotPrefab, TesterPrefab, BoxPrefab, WorkspacePrefab, UnknownPrefab;

        public GameObject CurrentlySelectedObject;

        private bool sceneActive = true;
        private bool projectActive = true;

        public bool ActionObjectsInteractive, ActionObjectsVisible;

        // Update is called once per frame
        private void Update() {
            // Activates scene if the AREditor is in SceneEditor mode and scene is interactable (no windows are openned).
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor && GameManager.Instance.SceneInteractable) {
                if (!sceneActive && (ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionObjectsForGizmo(true);
                    sceneActive = true;
                } else if (sceneActive && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionObjectsForGizmo(false);
                    sceneActive = false;
                }
            } else {
                if (sceneActive) {
                    ActivateActionObjectsForGizmo(false);
                    sceneActive = false;
                }
            }
            
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor && GameManager.Instance.SceneInteractable) {
                if (!projectActive && (ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionPointsForGizmo(true);
                    projectActive = true;
                } else if (projectActive && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionPointsForGizmo(false);
                    projectActive = false;
                }
            } else {
                if (projectActive) {
                    ActivateActionPointsForGizmo(false);
                    projectActive = false;
                }
            }
        }

        /// <summary>
        /// Deactivates or activates all action objects in scene for gizmo interaction.
        /// </summary>
        /// <param name="activate"></param>
        private void ActivateActionObjectsForGizmo(bool activate) {
            if (activate) {
                gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                foreach (ActionObject actionObject in ActionObjects.Values) {
                    actionObject.ActivateForGizmo("GizmoRuntime");
                }
            } else {
                gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (ActionObject actionObject in ActionObjects.Values) {
                    actionObject.ActivateForGizmo("Default");
                }
            }
        }

        /// <summary>
        /// Deactivates or activates all action points in scene for gizmo interaction.
        /// </summary>
        /// <param name="activate"></param>
        private void ActivateActionPointsForGizmo(bool activate) {
            if (activate) {
                gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                    actionPoint.ActivateForGizmo("GizmoRuntime");
                }
            } else {
                gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                    actionPoint.ActivateForGizmo("Default");
                }
            }
        }

        private string GetFreeIOName(string ioType) {
            int i = 1;
            bool hasFreeName;
            string freeName = ioType;
            do {
                hasFreeName = true;
                if (ActionObjects.ContainsKey(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = ioType + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        public void SetSelectedObject(GameObject obj) {
            if (CurrentlySelectedObject != null) {
                CurrentlySelectedObject.SendMessage("Deselect");
            }
            CurrentlySelectedObject = obj;
        }

        

        #region ACTION_OBJECTS

        public ActionObject SpawnActionObject(string id, string type, bool updateScene = true, string name = "") {
            if (!ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata aom)) {
                return null;
            }
            GameObject obj;
            if (aom.Robot) {
                obj = Instantiate(RobotPrefab, ActionObjectsSpawn.transform);
            } else {
                switch (type) {
                    case "Box":
                        obj = Instantiate(BoxPrefab, ActionObjectsSpawn.transform);
                        break;
                    case "Box2":
                        obj = Instantiate(BoxPrefab, ActionObjectsSpawn.transform);
                        break;
                    case "Tester":
                        obj = Instantiate(TesterPrefab, ActionObjectsSpawn.transform);
                        break;
                    case "Workspace":
                        obj = Instantiate(WorkspacePrefab, ActionObjectsSpawn.transform);
                        break;
                    default:
                        obj = Instantiate(UnknownPrefab, ActionObjectsSpawn.transform);
                        break;
                }
            }

            ActionObject actionObject = obj.GetComponentInChildren<ActionObject>();

            if (name == "")
                name = GetFreeIOName(type);
            
            actionObject.InitActionObject(id, type, obj.transform.localPosition, obj.transform.localRotation, id, aom);

            // Add the Action Object into scene reference
            ActionObjects.Add(id, actionObject);
            if (aom.Robot) {
                actionObject.LoadEndEffectors();
            }

            return actionObject;
        }

        /// <summary>
        /// Updates action GameObjects in ActionObjects dict based on the data present in IO.Swagger.Model.Scene Data.
        /// </summary>
        public void UpdateActionObjects() {
            List<string> currentAO = new List<string>();
            foreach (IO.Swagger.Model.SceneObject aoSwagger in Data.Objects) {
                ActionObject actionObject = SpawnActionObject(aoSwagger.Id, aoSwagger.Type, false, aoSwagger.Name);
                actionObject.ActionObjectUpdate(aoSwagger, ActionObjectsVisible, ActionObjectsInteractive);
                currentAO.Add(aoSwagger.Id);
            }

        }

        /// <summary>
        /// Updates all services from scene data.  
        /// Only called when whole scene arrived, i.e. when client is connected or scene is opened, so all service needs to be added.
        /// </summary>
        public void UpdateServices() {
            ActionsManager.Instance.ClearServices(); //just to be sure
            foreach (IO.Swagger.Model.SceneService service in Data.Services) {
                ActionsManager.Instance.AddService(service);
            }
        }

        public ActionObject GetNextActionObject(string aoId) {
            List<string> keys = ActionObjects.Keys.ToList();
            Debug.Assert(keys.Count > 0);
            int index = keys.IndexOf(aoId);
            string next;
            if (index + 1 < ActionObjects.Keys.Count)
                next = keys[index + 1];
            else
                next = keys[0];
            if (!ActionObjects.TryGetValue(next, out ActionObject actionObject)) {
                throw new ItemNotFoundException("This should never happen");
            }
            return actionObject;
        }

        public ActionObject GetPreviousActionObject(string aoId) {
            List<string> keys = ActionObjects.Keys.ToList();
            Debug.Assert(keys.Count > 0);
            int index = keys.IndexOf(aoId);
            string previous;
            if (index - 1 > -1)
                previous = keys[index - 1];
            else
                previous = keys[keys.Count - 1];
            if (!ActionObjects.TryGetValue(previous, out ActionObject actionObject)) {
                throw new ItemNotFoundException("This should never happen");
            }
            return actionObject;
        }

        public ActionObject GetFirstActionObject() {
            if (ActionObjects.Count == 0) {
                return null;
            }
            return ActionObjects.First().Value;
        }

        /// <summary>
        /// Shows action objects models
        /// </summary>
        public void ShowActionObjects() {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.Show();
            }
            GameManager.Instance.SaveBool("scene/" + Data.Id + "/AOVisibility", true);
        }

        /// <summary>
        /// Hides action objects models
        /// </summary>
        public void HideActionObjects() {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.Hide();
            }
            GameManager.Instance.SaveBool("scene/" + Data.Id + "/AOVisibility", false);
        }

         /// <summary>
        /// Sets whether action objects should react to user inputs (i.e. enables/disables colliders)
        /// </summary>
        public void SetActionObjectsInteractivity(bool interactivity) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.SetInteractivity(interactivity);
            }
            GameManager.Instance.SaveBool("scene/" + Data.Id + "/AOInteractivity", interactivity);
            Debug.LogError("Save to: " + "scene/" + Data.Id + "/AOInteractivity: " + interactivity.ToString());
        }


        /// <summary>
        /// Destroys and removes references to all action objects in the scene.
        /// </summary>
        public void RemoveActionObjects() {
            foreach (string actionObjectId in ActionObjects.Keys.ToList<string>()) {
                RemoveActionObject(actionObjectId);
            }
            // just to make sure that none reference left
            ActionObjects.Clear();
        }

        /// <summary>
        /// Destroys and removes references to action object of given Id.
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveActionObject(string Id) {
            try {
                ActionObjects[Id].DeleteActionObject();
            } catch (NullReferenceException e) {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Finds action object by user defined ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionObject FindActionObjectByID(string id) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.Data.Id == id) {
                    return actionObject;
                }
            }
            return null;
        }

        #endregion

        #region ACTION_POINTS

        public ActionPoint SpawnActionPoint(IO.Swagger.Model.ProjectActionPoint apData, IActionPointParent actionPointParent) {
            Debug.Assert(apData != null);
            GameObject AP;
            if (actionPointParent == null) {
                AP = Instantiate(ActionPointPrefab, ActionPointsOrigin.transform);
            } else {
                AP = Instantiate(ActionPointPrefab, actionPointParent.GetTransform());
            }

            AP.transform.localScale = new Vector3(1f, 1f, 1f);
            ActionPoint actionPoint = AP.GetComponent<ActionPoint>();
            actionPoint.InitAP(apData, actionPointParent);
            ActionPoints.Add(actionPoint.Data.Id, actionPoint);

            return actionPoint;
        }



        /// <summary>
        /// Updates action point GameObject in ActionObjects.ActionPoints dict based on the data present in IO.Swagger.Model.ActionPoint Data.
        /// </summary>
        /// <param name="project"></param>
        public async void UpdateActionPoints(IO.Swagger.Model.Project project) {
            List<string> currentAP = new List<string>();
            List<string> currentActions = new List<string>();
            Dictionary<string, string> connections = new Dictionary<string, string>();

            

            foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in project.ActionPoints) {
                // if action point exist, just update it
                if (ActionPoints.TryGetValue(projectActionPoint.Id, out ActionPoint actionPoint)) {
                    actionPoint.ActionPointUpdate(projectActionPoint);
                }
                // if action point doesn't exist, create new one
                else {
                    ActionObject actionObject = null;
                    if (projectActionPoint.Parent != null) {
                        ActionObjects.TryGetValue(projectActionPoint.Parent, out actionObject);
                    }
                    //TODO: update spawn action point to not need action object
                    actionPoint = SpawnActionPoint(projectActionPoint, actionObject);
                }

                // update actions in current action point 
                var updateActionsResult = await UpdateActions(projectActionPoint, actionPoint);
                currentActions.AddRange(updateActionsResult.Item1);
                // merge dictionaries
                connections = connections.Concat(updateActionsResult.Item2).GroupBy(i => i.Key).ToDictionary(i => i.Key, i => i.First().Value);

                actionPoint.UpdatePositionsOfPucks();

                currentAP.Add(actionPoint.Data.Id);
            }
               
            

            UpdateActionConnections(project.ActionPoints, connections);

            // Remove deleted actions
            foreach (string actionId in GetAllActionsDict().Keys.ToList<string>()) {
                if (!currentActions.Contains(actionId)) {
                    RemoveAction(actionId);
                }
            }

            // Remove deleted action points
            foreach (string actionPointId in GetAllActionPointsDict().Keys.ToList<string>()) {
                if (!currentAP.Contains(actionPointId)) {
                    RemoveActionPoint(actionPointId);
                }
            }
        }

        public void RemoveActionPoints() {
            List<ActionPoint> actionPoints = ActionPoints.Values.ToList();
            foreach (ActionPoint actionPoint in actionPoints) {
                actionPoint.DeleteAP();
            }
        }

        public IActionProvider GetActionProvider(string id) {
            try {
                return ActionsManager.Instance.GetService(id);
            } catch (KeyNotFoundException ex) {

            }

            if (ActionObjects.TryGetValue(id, out ActionObject actionObject)) {
                return actionObject;
            }
            throw new KeyNotFoundException("No action provider with id: " + id);
        }

        public IActionPointParent GetActionPointParent(string parentId) {
            if (parentId == null)
                return null;
            if (ActionObjects.TryGetValue(parentId, out ActionObject actionObject)) {
                return actionObject;
            }
            /*if (ActionPoints.TryGetValue(parentId, out ActionPoint actionPoint)) {
                return actionPoint;
            }*/
            return null;
        }

        /// <summary>
        /// Destroys and removes references to action point of given Id.
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveActionPoint(string Id) {
           // Call function in corresponding action point that will delete it and properly remove all references and connections.
            // We don't want to update project, because we are calling this method only upon received update from server.
            ActionPoints[Id].DeleteAP();
        }

        /// <summary>
        /// Returns action point of given Id.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionPoint GetActionPoint(string Id) {
            if (ActionPoints.TryGetValue(Id, out ActionPoint actionPoint)) {
                    return actionPoint;
            }
            
            throw new KeyNotFoundException("ActionPoint " + Id + " not found!");
        }

        /// <summary>
        /// Returns all action points in the scene in a list [ActionPoint_object]
        /// </summary>
        /// <returns></returns>
        public List<ActionPoint> GetAllActionPoints() {
            return ActionPoints.Values.ToList();
        }

        /// <summary>
        /// Returns all action points in the scene in a dictionary [action_point_Id, ActionPoint_object]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ActionPoint> GetAllActionPointsDict() {
            return ActionPoints;
        }

        #endregion

        #region ACTIONS

        public async Task<Action> SpawnPuck(string action_id, string action_user_id, ActionPoint ap, IActionProvider actionProvider, string puck_id = "") {
            string newId = puck_id;
            const string glyphs = "0123456789";
            if (newId == "") {
                newId = action_user_id;
                for (int j = 0; j < 4; j++) {
                    newId += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
                }
            } else {
                if (Base.Scene.Instance.GetActionById(puck_id) != null) {
                    Base.Notifications.Instance.ShowNotification("Failed to create action", "Action with name " + puck_id + " already exists");
                    return null;
                }
            }
            GameManager.Instance.StartLoading();
            ActionMetadata actionMetadata;

            try {
                actionMetadata = actionProvider.GetActionMetadata(action_user_id);
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                GameManager.Instance.EndLoading();
                return null; //TODO: throw exception
            }

            if (actionMetadata == null) {
                Debug.LogError("Actions not ready");
                GameManager.Instance.EndLoading();
                return null; //TODO: throw exception
            }

            GameObject puck = Instantiate(PuckPrefab, ap.ActionsSpawn.transform);
            puck.SetActive(false);
            

            

            if (action_id == "" || action_id == null) {
                action_id = Guid.NewGuid().ToString();
            } 

            await puck.GetComponent<Action>().Init(action_id, newId, actionMetadata, ap, actionProvider, false);

            puck.transform.localScale = new Vector3(1f, 1f, 1f);

            Action action = puck.GetComponent<Action>();

            // Add new action into scene reference
            ActionPoints[ap.Data.Id].Actions.Add(action_id, action);

            ap.UpdatePositionsOfPucks();
            puck.SetActive(true);
            
            GameManager.Instance.EndLoading();
            return action;
        }

        /// <summary>
        /// Updates actions of given ActionPoint and ProjectActionPoint received from server.
        /// </summary>
        /// <param name="projectActionPoint"></param>
        /// <param name="actionPoint"></param>
        /// <returns></returns>
        public async Task<(List<string>, Dictionary<string, string>)> UpdateActions(IO.Swagger.Model.ProjectActionPoint projectActionPoint, ActionPoint actionPoint) {
            List<string> currentA = new List<string>();
            // Connections between actions (action -> output --- input <- action2)
            Dictionary<string, string> connections = new Dictionary<string, string>();

            foreach (IO.Swagger.Model.Action projectAction in projectActionPoint.Actions) {
                string providerName = projectAction.Type.Split('/').First();
                string actionType = projectAction.Type.Split('/').Last();
                IActionProvider actionProvider;
                //if (ActionObjects.TryGetValue(providerName, out ActionObject originalActionObject)) {
                ActionObject originalActionObject = FindActionObjectByID(providerName);
                if (originalActionObject != null) {
                    actionProvider = originalActionObject;
                } else if (ActionsManager.Instance.ServicesData.TryGetValue(providerName, out Service originalService)) {
                    actionProvider = originalService;
                } else {
                    Debug.LogError("PROVIDER NOT FOUND EXCEPTION: " + providerName + " " + actionType);
                    continue; //TODO: throw exception
                }

                // if action exist, just update it
                if (actionPoint.Actions.TryGetValue(projectAction.Id, out Action action)) {
                    action.ActionUpdate(projectAction);
                }
                // if action doesn't exist, create new one
                else {
                    action = await SpawnPuck(projectAction.Id, actionType, actionPoint, actionProvider, projectAction.Id);
                    action.ActionUpdate(projectAction);
                }

                // Updates (or creates new) parameters of current action
                foreach (IO.Swagger.Model.ActionParameter projectActionParameter in projectAction.Parameters) {
                    try {
                        // If action parameter exist in action dictionary, then just update that parameter value (it's metadata will always be unchanged)
                        if (action.Parameters.TryGetValue(projectActionParameter.Id, out ActionParameter actionParameter)) {
                            actionParameter.UpdateActionParameter(projectActionParameter);
                        }
                        // Otherwise create a new action parameter, load metadata for it and add it to the dictionary of action
                        else {
                            // Loads metadata of specified action parameter - projectActionParameter. Action.Metadata is created when creating Action.
                            IO.Swagger.Model.ActionParameterMeta actionParameterMetadata = action.Metadata.GetParamMetadata(projectActionParameter.Id);

                            actionParameter = new ActionParameter(actionParameterMetadata, action, projectActionParameter.Value);
                            action.Parameters.Add(actionParameter.Id, actionParameter);
                        }
                    } catch (ItemNotFoundException ex) {
                        Debug.LogError(ex);
                    }
                }

                // Add current connection from the server, we will only map the outputs
                foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Outputs) {
                    //if(!connections.ContainsKey(projectAction.Id))
                    connections.Add(projectAction.Id, actionIO.Default);
                }

                // local list of all actions for current action point
                currentA.Add(projectAction.Id);
            }
            
            return (currentA, connections);
        }

        /// <summary>
        /// Updates connections between actions in the scene.
        /// </summary>
        /// <param name="projectObjects"></param>
        /// <param name="connections"></param>
        public void UpdateActionConnections(List<IO.Swagger.Model.ProjectActionPoint> actionPoints, Dictionary<string, string> connections) {
            Dictionary<string, Action> actionsToActualize = new Dictionary<string, Action>();

            // traverse through all actions (even freshly created)
            foreach (Action action in GetAllActions()) {
                // get connection from dictionary [actionID,outputAction]
                if (connections.TryGetValue(action.Data.Id, out string actionOutput)) {
                    // Check if action's output action is NOT the same as actionOutput from newly received data from server,
                    // then connection changed and we have to delete actual connection of current action and create new one
                    Action refAction = null;
                    // Find corresponding action defined by ID
                    if (actionOutput != "start" && actionOutput != "end") {
                        refAction = GetActionById(actionOutput);
                        if (refAction != null) {
                            actionOutput = refAction.Data.Id;
                        } else {
                            actionOutput = "";
                        }
                    }
                    if (action.Output.Data.Default != actionOutput) {
                        // Destroy old connection if there was some
                        if (action.Output.Connection != null) {
                            ConnectionManagerArcoro.Instance.Connections.Remove(action.Output.Connection);
                            Destroy(action.Output.Connection.gameObject);
                        }

                        // Create new connection only if connected action exists (it is not start nor end)
                        if (refAction != null) {
                            // Create new one
                            //PuckInput input = GetAction(actionOutput).Input;
                            PuckInput input = refAction.Input;
                            PuckOutput output = action.Output;

                            GameObject c = Instantiate(ConnectionPrefab);
                            c.transform.SetParent(ConnectionManager.instance.transform);
                            Connection newConnection = c.GetComponent<Connection>();
                            // We are always connecting output to input.
                            newConnection.target[0] = output.gameObject.GetComponent<RectTransform>();
                            newConnection.target[1] = input.gameObject.GetComponent<RectTransform>();

                            input.Connection = newConnection;
                            output.Connection = newConnection;
                            ConnectionManagerArcoro.Instance.Connections.Add(newConnection);
                        }
                    }
                    actionsToActualize.Add(action.Data.Id, action);
                }
            }

            // Set action inputs and outputs for updated connections
            foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in actionPoints) {
                foreach (IO.Swagger.Model.Action projectAction in projectActionPoint.Actions) {                        
                    if (actionsToActualize.TryGetValue(projectAction.Id, out Action action)) {
                        // Sets action inputs (currently each action has only 1 input)
                        foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Inputs) {
                            action.Input.Data = actionIO;
                        }

                        // Sets action outputs (currently each action has only 1 output)
                        foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Outputs) {
                            action.Output.Data = actionIO;
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Destroys and removes references to action of given Id.
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveAction(string Id) {
            Action aToRemove = GetAction(Id);
            string apIdToRemove = aToRemove.ActionPoint.Data.Id;
            // Call function in corresponding action that will delete it and properly remove all references and connections.
            // We don't want to update project, because we are calling this method only upon received update from server.
            ActionPoints[apIdToRemove].Actions[Id].DeleteAction(false);
        }

        /// <summary>
        /// Returns action of given Id.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Action GetAction(string Id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                if (actionPoint.Actions.TryGetValue(Id, out Action action)) {
                    return action;
                }
            }
            
            //Debug.LogError("Action " + Id + " not found!");
            return null;
        }

        /// <summary>
        /// Returns action of given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Action GetActionById(string id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                foreach (Action action in actionPoint.Actions.Values) {
                    if (action.Data.Id == id) {
                        return action;
                    }
                }
            }
            
            //Debug.LogError("Action " + id + " not found!");
            return null;
        }

        /// <summary>
        /// Returns all actions in the scene in a list [Action_object]
        /// </summary>
        /// <returns></returns>
        public List<Action> GetAllActions() {
            List<Action> actions = new List<Action>();
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                foreach (Action action in actionPoint.Actions.Values) {
                    actions.Add(action);
                }
            }
            
            return actions;
        }

        /// <summary>
        /// Returns all actions in the scene in a dictionary [action_Id, Action_object]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Action> GetAllActionsDict() {
            Dictionary<string, Action> actions = new Dictionary<string, Action>();
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                foreach (Action action in actionPoint.Actions.Values) {
                    actions.Add(action.Data.Id, action);
                }
            }
           
            return actions;
        }

        #endregion


        //// Deactivates or activates scene and all objects in scene to ignore raycasting (clicking)
        //private void ActivateSceneForEditing(bool activate, string tagToActivate) {
        //    //Transform[] allChildren = Helper.FindComponentsInChildrenWithTag<Transform>(gameObject, tagToActivate);
        //    //if (activate) {
        //    //    gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //    //    foreach (Transform child in allChildren) {
        //    //        child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //    //    }
        //    //} else {
        //    //    gameObject.layer = LayerMask.NameToLayer("Default");
        //    //    foreach (Transform child in allChildren) {
        //    //        child.gameObject.layer = LayerMask.NameToLayer("Default");
        //    //    }
        //    //}

        //    if (activate) {
        //        foreach (GameObject actionObject in ActionObjects.Keys) {
        //            actionObject.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //            foreach (Transform child in actionObject.GetComponentsInChildren<Transform>()) {
        //                child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //            }
        //        }
        //    } else {
        //        foreach (GameObject actionObject in ActionObjects.Keys) {
        //            actionObject.gameObject.layer = LayerMask.NameToLayer("Default");
        //            foreach (Transform child in actionObject.GetComponentsInChildren<Transform>()) {
        //                child.gameObject.layer = LayerMask.NameToLayer("Default");
        //            }
        //        }
        //    }
        //}

        //private void ActivateProjectForEditing(bool activate, string tagToActivate) {
        //    if (activate) {
        //        foreach (List<GameObject> actionPoints in ActionObjects.Values) {
        //            foreach (GameObject aP in actionPoints) {
        //                aP.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //                foreach (Transform child in aP.GetComponentsInChildren<Transform>()) {
        //                    child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //                }
        //            }
        //        }
        //    } else {
        //        foreach (List<GameObject> actionPoints in ActionObjects.Values) {
        //            foreach (GameObject aP in actionPoints) {
        //                aP.gameObject.layer = LayerMask.NameToLayer("Default");
        //                foreach (Transform child in aP.GetComponentsInChildren<Transform>()) {
        //                    child.gameObject.layer = LayerMask.NameToLayer("Default");
        //                }
        //            }
        //        }
        //    }
        //}
    }
}

