using System;
using Base;
using UnityEngine;

public class LogicItem 
{
    public IO.Swagger.Model.LogicItem Data;

    private ConnectionLine connection;

    public PuckInput Input {
        get;
        private set;
    }
    public PuckOutput Output {
        get;
        private set;
    }

    public LogicItem(IO.Swagger.Model.LogicItem logicItem) {
        Data = logicItem;
        UpdateConnection(logicItem);
    }

    public void Remove() {
        if (Input.LineToConnection != null)
            GameObject.Destroy(Input.LineToConnection);
        if (Output.LineToConnection != null)
            GameObject.Destroy(Output.LineToConnection);
        Input.RemoveLogicItem(Data.Id);
        Output.RemoveLogicItem(Data.Id);
        UnityEngine.Object.Destroy(connection.gameObject);
        connection = null;
    }

    public void UpdateConnection(IO.Swagger.Model.LogicItem logicItem) {
        if (connection != null) {
            Remove();
        }
        Input = ProjectManager.Instance.GetAction(logicItem.End).Input;
        Output = ProjectManager.Instance.GetAction(logicItem.Start).Output;
        Input.AddLogicItem(Data.Id);
        Output.AddLogicItem(Data.Id);

        connection = ConnectionManagerArcoro.Instance.CreateConnection(Input.gameObject, Output.gameObject);

        connection.InitConnection(Data.Id, Output.Action.GetName() + " => " + Input.Action.GetName());

        if (Input.LineToConnection != null)
            GameObject.Destroy(Input.LineToConnection.gameObject);
        if (Output.LineToConnection != null)
            GameObject.Destroy(Output.LineToConnection.gameObject);
        Input.LineToConnection = GameObject.Instantiate(ConnectionManagerArcoro.Instance.ConnectionPrefab).GetComponent<ConnectionLine>();
        Input.LineToConnection.SetTargets(Input.transform.GetComponent<RectTransform>(), Input.Action.Center);
        Output.LineToConnection = GameObject.Instantiate(ConnectionManagerArcoro.Instance.ConnectionPrefab).GetComponent<ConnectionLine>();
        Output.LineToConnection.SetTargets(Output.Action.Center, Output.transform.GetComponent<RectTransform>());

        SelectorMenu.Instance.CreateSelectorItem(connection);
        //connection.UpdateConnection();
    }



    public ConnectionLine GetConnection() {
        return connection;
    }

    

    

}
