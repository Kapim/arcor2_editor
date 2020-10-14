using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class Sight : Singleton<Sight> {
        public GameObject CurrentObject;

        public System.DateTime HoverStartTime;

        private bool endingHover = false;

        public APHeadUp APHeadUp;

        private void Update() {
            if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.TransformingAP)
                return;
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, Mathf.Infinity)) {
                try {
                    if (CurrentObject == null) {
                        HoverStart(hit);
                    } else {
                        if (!GameObject.ReferenceEquals(hit.collider.transform.gameObject, CurrentObject)) {
                            CurrentObject.SendMessage("OnHoverEnd");
                            HideHeadupMenu();
                            if (endingHover) {
                                StopAllCoroutines();
                                endingHover = false;
                            }
                            HoverStart(hit);
                        } else {

                            if (endingHover) {
                                StopAllCoroutines();
                                endingHover = false;
                                HoverStartTime = System.DateTime.UtcNow;
                            }
                        }
                    }         
                } catch (Exception e) {
                    Debug.LogError(e);
                }
            } else {
                if (CurrentObject != null) {
                    if (!endingHover)
                        StartCoroutine(HoverEnd());
                }
            }
        }

        private IEnumerator HoverEnd() {
            endingHover = true;
            yield return new WaitForSeconds((float) (0.5d - (System.DateTime.UtcNow - HoverStartTime).TotalSeconds));
            if (CurrentObject != null) {
                HideHeadupMenu();
                CurrentObject.SendMessage("OnHoverEnd");
                CurrentObject = null;
            }
            endingHover = false;
        }

        private void HideHeadupMenu() {
            ActionPoint actionPoint = CurrentObject.GetComponentInParent<ActionPoint>();
            if (actionPoint != null) {
                APHeadUp.HideMenu();
            }
        }

        private void HoverStart(RaycastHit hit) {
            hit.collider.transform.gameObject.SendMessage("OnHoverStart");
            HoverStartTime = System.DateTime.UtcNow;
            CurrentObject = hit.collider.transform.gameObject;

            ActionPoint actionPoint = hit.collider.transform.gameObject.GetComponent<OnClickCollider>()?.Target.GetComponent<ActionPoint>();
            if (actionPoint != null) {
                APHeadUp.ShowMenu(actionPoint);
            }
        }

        public void Touch() {
            if (CurrentObject == null)
                return;
            Clickable clickable = CurrentObject.GetComponent<Clickable>();
            if (clickable == null)
                return;
            clickable.OnClick(Clickable.Click.TOUCH);
        }

        public void LongTouch() {
            if (CurrentObject == null)
                return;
            Clickable clickable = CurrentObject.GetComponent<Clickable>();
            if (clickable == null)
                return;
            clickable.OnClick(Clickable.Click.LONG_TOUCH);
        }
    }
}

