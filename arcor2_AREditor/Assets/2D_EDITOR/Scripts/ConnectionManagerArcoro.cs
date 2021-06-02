using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using UnityEngine;

public class ConnectionManagerArcoro : Base.Singleton<ConnectionManagerArcoro> {

    public GameObject ConnectionPrefab;
    public Material DefaultMat, SelectedMat;
    public List<ConnectionLine> Connections = new List<ConnectionLine>();
    private ConnectionLine virtualConnectionToMouse;
    private GameObject virtualPointer;
    [SerializeField]
    private Material EnabledMaterial, DisabledMaterial;
    public GameObject SegmentColliderPrefab;

    private void Start() {
        virtualPointer = VirtualConnectionOnTouch.Instance.VirtualPointer;

    }


    public ConnectionLine CreateConnection(GameObject o1, GameObject o2) {
        ConnectionLine c = Instantiate(ConnectionPrefab).GetComponent<ConnectionLine>();
        
        c.transform.SetParent(transform);
        // Set correct targets. Output has to be always at 0 index, because we are connecting output to input.
        // Output has direction to the east, while input has direction to the west.
        if (o1.GetComponent<Base.InputOutput>().GetType() == typeof(Base.PuckOutput)) {
            c.Target[0] = o1.GetComponent<RectTransform>();
            c.Target[1] = o2.GetComponent<RectTransform>();
        } else {
            c.Target[1] = o1.GetComponent<RectTransform>();
            c.Target[0] = o2.GetComponent<RectTransform>();
        }
        Connections.Add(c);
        if (!ControlBoxManager.Instance.ConnectionsToggle.isOn)
            c.gameObject.SetActive(false);
        
        return c;
    }

    private void FixedUpdate() {
        if (virtualConnectionToMouse != null)
            virtualConnectionToMouse.UpdateConnection();
    }

    public void CreateConnectionToPointer(GameObject o) {
        if (virtualConnectionToMouse != null)
            Destroy(virtualConnectionToMouse.gameObject);
        VirtualConnectionOnTouch.Instance.DrawVirtualConnection = true;
        virtualConnectionToMouse = CreateConnection(o, virtualPointer);
    }

    public void DestroyConnectionToMouse() {
        if (virtualConnectionToMouse != null) {
            Destroy(virtualConnectionToMouse.gameObject);
            Connections.Remove(virtualConnectionToMouse);
            VirtualConnectionOnTouch.Instance.DrawVirtualConnection = false;
        }
    }

    public bool IsConnecting() {
        return virtualConnectionToMouse != null;
    }

    public Base.Action GetActionConnectedToPointer() {
        Debug.Assert(virtualConnectionToMouse != null);
        GameObject obj = GetConnectedTo(virtualConnectionToMouse, virtualPointer);
        return obj.GetComponent<InputOutput>().Action;
    }

    public GameObject GetConnectedToPointer() {
        Debug.Assert(virtualConnectionToMouse != null);
        return GetConnectedTo(virtualConnectionToMouse, virtualPointer);
    }

     public Base.Action GetActionConnectedTo(ConnectionLine c, GameObject o) {        
        return GetConnectedTo(c, o).GetComponent<InputOutput>().Action;
    }

    private int GetIndexOf(ConnectionLine c, GameObject o) {
        if (c.Target[0] != null && c.Target[0].gameObject == o) {
            return 0;
        } else if (c.Target[1] != null && c.Target[1].gameObject == o) {
            return 1;
        } else {
            return -1;
        }
    }

    private int GetIndexByType(ConnectionLine c, System.Type type) {
        if (c.Target[0] != null && c.Target[0].gameObject.GetComponent<Base.InputOutput>() != null && c.Target[0].gameObject.GetComponent<Base.InputOutput>().GetType().IsSubclassOf(type))
            return 0;
        else if (c.Target[1] != null && c.Target[1].gameObject.GetComponent<Base.InputOutput>() != null && c.Target[1].gameObject.GetComponent<Base.InputOutput>().GetType().IsSubclassOf(type))
            return 1;
        else
            return -1;

    }

    public GameObject GetConnectedTo(ConnectionLine c, GameObject o) {
        if (c == null || o == null)
            return null;
        int i = GetIndexOf(c, o);
        if (i < 0)
            return null;
        return c.Target[1 - i].gameObject;
    }

    /**
     * Checks that there is input on one end of connection and output on the other side
     */
    public bool ValidateConnection(ConnectionLine c) {
        if (c == null)
            return false;
        int input = GetIndexByType(c, typeof(Base.InputOutput)), output = GetIndexByType(c, typeof(Base.PuckOutput));
        if (input < 0 || output < 0)
            return false;
        return input + output == 1;
    }

    public async Task<bool> ValidateConnection(InputOutput output, InputOutput input, IO.Swagger.Model.ProjectLogicIf condition) {
        string[] startEnd = new[] { "START", "END" };
        if (output.GetType() == input.GetType() ||
            output.Action.Data.Id.Equals(input.Action.Data.Id) ||
            (startEnd.Contains(output.Action.Data.Id) && startEnd.Contains(input.Action.Data.Id))) {
            return false;
        }
        try {
            // TODO: how to pass condition?
            await WebsocketManager.Instance.AddLogicItem(output.Action.Data.Id, input.Action.Data.Id, condition, true);
        } catch (RequestFailedException) {
            return false;
        }
        return true;
    }

    public void Clear() {
        foreach (ConnectionLine c in Connections) {
            if (c != null && c.gameObject != null) {
                Destroy(c.gameObject);
            }
        }
        Connections.Clear();
    }

    public void DisplayConnections(bool active) {
        foreach (ConnectionLine connection in Connections) {
            connection.gameObject.SetActive(active);
        }
    }

    public void DisableConnectionToMouse() {
        if (virtualConnectionToMouse != null)
            virtualConnectionToMouse.GetComponent<LineRenderer>().material = DisabledMaterial;
    }

    public void EnableConnectionToMouse() {
        if (virtualConnectionToMouse != null)
            virtualConnectionToMouse.GetComponent<LineRenderer>().material = EnabledMaterial;
    }

}
