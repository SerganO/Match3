using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OneButtonMessage : MonoBehaviour
{
    public TMP_Text TitleLabel;
    public TMP_Text MessageLabel;
    public TMP_Text ButtonTitle;
    public Button ActionButton;

    public void Setup(string title, string message, string buttonTitle, VoidFunc action)
    {
        TitleLabel.text = title;
        MessageLabel.text = message;
        ButtonTitle.text = buttonTitle;
        ActionButton.onClick.RemoveAllListeners();
        ActionButton.onClick.AddListener(() => { action(); });
    }
}
