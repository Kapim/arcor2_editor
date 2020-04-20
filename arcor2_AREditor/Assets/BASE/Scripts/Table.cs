using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField]
    private GameObject Desk, Leg1, Leg2, Leg3, Leg4, Visual;

    public void SetDeskDimensions(float width, float height) {
        Desk.transform.localScale = new Vector3(width, 0.1f, height);
        Leg1.transform.localPosition = new Vector3(-width / 2 + 0.01f, -0.5f, -height / 2 + 0.01f);
        Leg2.transform.localPosition = new Vector3(width / 2 + 0.01f, -0.5f, -height / 2 + 0.01f);
        Leg3.transform.localPosition = new Vector3(-width / 2 + 0.01f, -0.5f, height / 2 + 0.01f);
        Leg4.transform.localPosition = new Vector3(width / 2 + 0.01f, -0.5f, height / 2 + 0.01f);
    }

    public void SetTableHeight(float height) {
        Visual.transform.localPosition = new Vector3(0, -1 - 0.05f + height, 0);
    }
}
