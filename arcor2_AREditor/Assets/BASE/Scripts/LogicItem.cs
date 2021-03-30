using System;
using Base;
using UnityEngine;

public class LogicItem 
{
    public IO.Swagger.Model.LogicItem Data;

    private Connection connection;

    private InputOutput input;
    private PuckOutput output;

    

    public LogicItem(IO.Swagger.Model.LogicItem logicItem) {
        Data = logicItem;
        UpdateConnection(logicItem);
    }

    public void Remove() {
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
        
    }

    public Connection GetConnection() {
        return connection;
    }

}
