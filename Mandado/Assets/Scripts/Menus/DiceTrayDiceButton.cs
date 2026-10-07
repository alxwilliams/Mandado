using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceTrayDiceButton : MonoBehaviour
{
    [SerializeField] private Button _mainButton;
    [SerializeField] private Button _leftOrderButton;
    [SerializeField] private Button _rightOrderButton;

    private Action<int,int> _leftButtonAction;
    private Action<int,int> _rightButtonAction;
    private Action _disableOrderScreen;

    private int _indexValue;
    private int _value;
    
    public Button MainButton => _mainButton;

    public void SetUpButton(Action<int,int> leftButton, Action<int,int> rightButton, Action disableOrderScreen)
    {
        _leftButtonAction = leftButton;
        _rightButtonAction = rightButton;
        _disableOrderScreen = disableOrderScreen;
        
        _leftOrderButton.onClick.AddListener(OnLeftOrderButtonClicked);
        _rightOrderButton.onClick.AddListener(OnRightOrderButtonClicked);
    }

    public void SetDiceValue(int indexVal, int val)
    {
        _indexValue = indexVal;
        _value = val;
    }
    
    public void SetOrderButtonsActiveState(bool active)
    {
        _leftOrderButton.gameObject.SetActive(active && _value > 1);
        _rightOrderButton.gameObject.SetActive(active && _value < 6);
    }

    private void OnLeftOrderButtonClicked()
    {
        _leftButtonAction?.Invoke(_indexValue,_value);
        _disableOrderScreen?.Invoke();
    }
    
    private void OnRightOrderButtonClicked()
    {
        _rightButtonAction?.Invoke(_indexValue,_value);
        _disableOrderScreen?.Invoke();
    }
    
    
}
