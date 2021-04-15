using System;
using Base;
using UnityEngine;

public class LogicItem 
{
    public IO.Swagger.Model.LogicItem Data;

    private ConnectionLine connection;

    private PuckInput input;
    private PuckOutput output;

    

    public LogicItem(IO.Swagger.Model.LogicItem logicItem) {
        Data = logicItem;
        UpdateConnection(logicItem);
    }

    public void Remove() {
        if (input.LineToConnection != null)
            GameObject.Destroy(input.LineToConnection);
        if (output.LineToConnection != null)
            GameObject.Destroy(output.LineToConnection);
        input.RemoveLogicItem(Data.Id);
        output.RemoveLogicItem(Data.Id);
        UnityEngine.Object.Destroy(connection.gameObject);
        connection = null;
    }

    public void UpdateConnection(IO.Swagger.Model.LogicItem logicItem) {
        if (connection != null) {
            Remove();
        }
        input = ProjectManager.Instance.GetAction(logicItem.End).Input;
        output = ProjectManager.Instance.GetAction(logicItem.Start).Output;
        input.AddLogicItem(Data.Id);
        output.AddLogicItem(Data.Id);

        connection = ConnectionManagerArcoro.Instance.CreateConnection(input.gameObject, output.gameObject);

        connection.InitConnection(Data.Id, output.Action.GetName() + " => " + input.Action.GetName());
        

        input.LineToConnection = GameObject.Instantiate(ConnectionManagerArcoro.Instance.ConnectionPrefab).GetComponent<ConnectionLine>();
        input.LineToConnection.SetTargets(input.transform.GetComponent<RectTransform>(), input.Action.Center);
        output.LineToConnection = GameObject.Instantiate(ConnectionManagerArcoro.Instance.ConnectionPrefab).GetComponent<ConnectionLine>();
        output.LineToConnection.SetTargets(output.Action.Center, output.transform.GetComponent<RectTransform>());

        SelectorMenu.Instance.CreateSelectorItem(connection);
        connection.UpdateConnection();
    }



    public ConnectionLine GetConnection() {
        return connection;
    }

    

}
