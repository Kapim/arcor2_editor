using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.EventSystems;
using RuntimeInspectorNamespace;

[RequireComponent(typeof(TooltipContent))]
public class ManualTooltip : MonoBehaviour {
    [SerializeField]
    private TooltipContent tooltipContent;
    [SerializeField]
    public string Description, DescriptionAlternative;
    [SerializeField]
    private bool displayAlternativeDescription = false;

    public bool DisplayAlternativeDescription {
        get => displayAlternativeDescription;
        set {
            displayAlternativeDescription = value;
            if (displayAlternativeDescription) {
                ShowAlternativeDescription();
            } else {
                ShowDefaultDescription();
            }
        }
    }

    private void Awake() {
        if (tooltipContent.tooltipRect == null || tooltipContent.descriptionText == null) {
            tooltipContent.tooltipRect = TooltipRef.Instance.Tooltip;
            tooltipContent.descriptionText = TooltipRef.Instance.Text;
        }
        tooltipContent.delay = AREditorResources.TooltipDelay;
    }

    private void Start() {
        Debug.Assert(tooltipContent != null);
        if (string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(DescriptionAlternative)) {
            tooltipContent.enabled = false;
            return;
        }
        

        if (DisplayAlternativeDescription) {
            ShowAlternativeDescription();
        } else {
            ShowDefaultDescription();
        }
    }

    private void ShowDefaultDescription() {
        if (tooltipContent == null)
            return; // tooltip was destroyed in the meantime
        if (string.IsNullOrEmpty(Description)) {
            tooltipContent.enabled = false;
        } else {
            tooltipContent.description = Description;
            tooltipContent.enabled = true;
        }
    }

    private void ShowAlternativeDescription() {
        if (tooltipContent == null)
            return; // tooltip was destroyed in the meantime
        if (string.IsNullOrEmpty(DescriptionAlternative)) {
            tooltipContent.enabled = false;
        } else {
            tooltipContent.description = DescriptionAlternative;
            tooltipContent.enabled = true;
        }
    }

    private void OnDisable() {
        if (tooltipContent != null && tooltipContent.tooltipAnimator != null)
            tooltipContent.OnPointerExit(null);
    }

    public void DisableTooltip() {
        tooltipContent.enabled = false;
    }

}
