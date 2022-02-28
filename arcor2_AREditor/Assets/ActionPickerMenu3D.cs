using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPickerMenu3D : MonoBehaviour
{
    [SerializeField]
    List<GameObject> MagicBelt;

    public void Show() {
        bool showMagicBelt = false;
        foreach (Base.ActionObject obj in Base.SceneManager.Instance.ActionObjects.Values) {
            if (obj.GetName().Contains("magic") || obj.GetName().Contains("onvey")) {
                showMagicBelt = true;
            }
        }
        foreach (GameObject obj in MagicBelt) {
            obj.SetActive(showMagicBelt);
        }
        gameObject.SetActive(true);
    }
} 
