using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectedChildButton : MonoBehaviour
{
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _unselectedColor;

    private Button[] _buttons;

    private void Start()
    {
        _buttons = GetComponentsInChildren<Button>();

        foreach (Button button in _buttons)
            button.onClick.AddListener(() => selectButton(button));

        selectButton(_buttons[0]);
    }

    private void selectButton(Button pButton)
    {
        foreach (Button button in _buttons)
            button.GetComponent<Image>().color = _unselectedColor;

        pButton.GetComponent<Image>().color = _selectedColor;
    }
}
