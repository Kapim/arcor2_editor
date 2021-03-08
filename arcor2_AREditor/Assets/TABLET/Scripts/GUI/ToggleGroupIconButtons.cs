using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class ToggleGroupIconButtons : Singleton<ToggleGroupIconButtons> {
    public List<IconButton> Buttons = new List<IconButton>();
    public IconButton Default;

    private Button selected;

    public Color SelectedColor;

    private void Start() {
        foreach (IconButton btn in Buttons) {
            btn.Background.color = Color.clear;
            btn.Button.onClick.AddListener(() => SelectButton(btn));
        }
        Default.Background.color = SelectedColor;
    }

    public void SelectButton(IconButton button) {
        foreach (IconButton btn in Buttons) {
            btn.Background.color = Color.clear;
        }
        button.Background.color = SelectedColor;
    }


}
