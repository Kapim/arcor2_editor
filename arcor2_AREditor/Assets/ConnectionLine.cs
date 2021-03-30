using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionLine : InteractiveObject {
    public string LogicItemId = "", Name = "connection";
    public void InitConnection(string logicItemId, string name, Connection connection) {
        LogicItemId = logicItemId;
        Name = name;
    }

    private void Update() {
        /*
                for (int j = 0; j < lineRenderer.GetPositions().Count; j++) {
                    Vector2 distanceBetweenPoints = bezierPoints[j - 1] - bezierPoints[j];
                    Vector3 crossProduct = Vector3.Cross(distanceBetweenPoints, Vector3.forward);

                    Vector2 up = (wireWidth / 2) * new Vector2(crossProduct.normalized.x, crossProduct.normalized.y) + bezierPoints[j - 1];
                    Vector2 down = -(wireWidth / 2) * new Vector2(crossProduct.normalized.x, crossProduct.normalized.y) + bezierPoints[j - 1];

                    edgePoints.Insert(0, down);
                    edgePoints.Add(up);

                    if (j == bezierPoints.Count - 1) {
                        // Compute the values for the last point on the Bezier curve
                        up = (wireWidth / 2) * new Vector2(crossProduct.normalized.x, crossProduct.normalized.y) + bezierPoints[j];
                        down = -(wireWidth / 2) * new Vector2(crossProduct.normalized.x, crossProduct.normalized.y) + bezierPoints[j];

                        edgePoints.Insert(0, down);
                        edgePoints.Add(up);
                    }
                }

                collider.points = edgePoints.ToArray();*/

        /*float x;
        float y;
        float z = 0f;
        int segments;

        float angle = 0f;

        for (int i = 0; i < (segments + 1) - (segments / gap); i++) {
            float halfWidth = lineRenderer.startWidth / 2f;
            Vector2 rightPoint = colliderPoints[i];
            Vector2 leftPoint = colliderPoints[i];
            rightPoint.x -= halfWidth;
            leftPoint.x += halfWidth;
            colliderPoints2.Add(rightPoint);
            colliderPoints2.Add(leftPoint));*/
    

        
    }

    public override string GetId() {
        return LogicItemId;
    }

    public override string GetName() {
        return Name;
    }

    public override bool HasMenu() {
        return false;
    }

    public override bool Movable() {
        return false;
    }

    public override void OnClick(Click type) {
        throw new System.NotImplementedException();
    }

    public override void OnHoverEnd() {
        
    }

    public override void OnHoverStart() {
        
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public override bool Removable() {
        return true;
    }

    public async override void Remove() {
        await Base.WebsocketManager.Instance.RemoveLogicItem(LogicItemId);
    }

    public override void Rename(string newName) {
        throw new System.NotImplementedException();
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }
}
