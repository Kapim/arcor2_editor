using System;
using Base;
using UnityEngine;

public class LogicItem 
{
    public IO.Swagger.Model.LogicItem Data;

    private Connection connection;

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

        ConnectionLine line = connection.GetComponent<ConnectionLine>();
        line.InitConnection(Data.Id, output.Action.GetName() + " => " + input.Action.GetName(), connection);
        input.transform.position = input.Action.ClosestPointOnCircle(output.Action.transform.position);
        output.transform.position = output.Action.ClosestPointOnCircle(input.Action.transform.position);
        input.LineToConnection = GameObject.Instantiate(ConnectionManagerArcoro.Instance.ConnectionNarrowPrefab).GetComponent<Connection>();
        input.LineToConnection.SetTargets(input.transform.GetComponent<RectTransform>(), input.Action.Center);
        output.LineToConnection = GameObject.Instantiate(ConnectionManagerArcoro.Instance.ConnectionNarrowPrefab).GetComponent<Connection>();
        output.LineToConnection.SetTargets(output.Action.Center, output.transform.GetComponent<RectTransform>());
        
    }

    public Connection GetConnection() {
        return connection;
    }

    

}
