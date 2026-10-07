using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceTrayDiceButton : MonoBehaviour
{
    [SerializeField] private Button _mainButton;
    [SerializeField] private Button _leftOrderButton;
    [SerializeField] private Button _rightOrderButton;

    public Button MainButton => _mainButton;

    public void SetOrderButtons(bool active)
    {
        _leftOrderButton.gameObject.SetActive(active);
        _rightOrderButton.gameObject.SetActive(active);
    }
}
