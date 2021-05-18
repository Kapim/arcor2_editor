using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCameraAroundAP : MonoBehaviour
{
    public GameObject GameObj;
    private void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);

        Vector3 dir = Camera.main.transform.position - GameObj.transform.position;
        dir.Normalize();
           
        transform.localPosition = dir * 0.8f;
        
    }
}
