using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SelectLanguage : MonoBehaviour
{
    #region Fields
    private Button[] _buttons;
    #endregion

    #region Methods
    private void Start()
    {
        _buttons = GetComponentsInChildren<Button>();

        foreach (Button button in _buttons)
            button.onClick.AddListener(() => selectLanguage(button));

        StartCoroutine(changeLanguage(_buttons[0], true));
    }

    private void selectLanguage(Button pButton)
    {
        foreach (Button button in _buttons)
        {
            if (button == pButton)
            {
                StartCoroutine(changeLanguage(button));
                return;
            }
        }
    }

    private IEnumerator changeLanguage(Button pButton, bool pIsInit = false)
    {
        yield return LocalizationSettings.InitializationOperation;

        if (!pIsInit)
            if (LocalizationSettings.SelectedLocale.LocaleName.Contains(pButton.name)) yield break;

        foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.LocaleName.Contains(pButton.name))
            {
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }

        foreach (Button button in _buttons)
        {
            Color oCol = button.transform.parent.GetComponent<Image>().color;
            float alpha = (button == pButton) ? 1 : 0;
            button.transform.parent.GetComponent<Image>().color = new Color(oCol.r, oCol.g, oCol.b, alpha);
            button.GetComponent<Image>().sprite = (button == pButton) ? button.GetComponent<LanguageIcon>().Selected : button.GetComponent<LanguageIcon>().Unselected;
        }
    }
    #endregion
}
