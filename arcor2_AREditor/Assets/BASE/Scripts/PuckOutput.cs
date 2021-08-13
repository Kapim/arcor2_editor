

using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class PuckOutput : InputOutput {
        public override string GetObjectTypeName() {
            return "Action output";
        }


        public override string GetId() {
            return Action.GetId() + "output";
        }
        public override void UpdateColor() {
            Renderer renderer = Action.OutputArrow.GetComponent<Renderer>();
            List<Material> materials = new List<Material>(renderer.materials);

            if (Enabled && !(IsLocked && !IsLockedByMe)) {
                foreach (var material in materials) {
                    if (Action.Data.Id == "START")
                        material.color = Color.green;
                    else if (Action.Data.Id == "END")
                        material.color = Color.red;
                    else
                        material.color = new Color(0.9f, 0.84f, 0.27f);
                }
            } else {
                foreach (var material in materials)
                    material.color = Color.gray;
            }
        }

        public override void CloseMenu() {
            throw new System.NotImplementedException();
        }
    }
}
