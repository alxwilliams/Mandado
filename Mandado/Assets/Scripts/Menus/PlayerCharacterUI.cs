using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterUI : CharacterUI
{
    [SerializeField] private Animator _trayAnimator;
    
    [Header("Dice Images/Sprites")] 
    
    [SerializeField] private Image _firstDice;
    [SerializeField] private Image _secondDice;
    [SerializeField] private Image _thirdDice;
    [SerializeField] private Image _fourDice;
    [SerializeField] private Image _fiveDice;

    [Header("Dice Buttons")]
    [SerializeField] private Button _firstButtons;
    [SerializeField] private Button _secondButtons;
    [SerializeField] private Button _thirdButtons;
    [SerializeField] private Button _fourButtons;
    [SerializeField] private Button _fiveButtons;
    
    
    
    private int _diceNumber;
    private float _amountOfActiveDice = 0;

    private Action<int> DiceButtonPressAction;

    public void Initialize(int diceNum, Action<int> diceButtonPress)
    {
        _diceNumber = diceNum;
        DiceButtonPressAction = diceButtonPress;
        
        _firstButtons.onClick.AddListener(OnDiceButtonPress);
        _secondButtons.onClick.AddListener(OnDiceButtonPress);
        _thirdButtons.onClick.AddListener(OnDiceButtonPress);
        _fourButtons.onClick.AddListener(OnDiceButtonPress);
        _fiveButtons.onClick.AddListener(OnDiceButtonPress);
    }

    private void OnDiceButtonPress()
    {
        _amountOfActiveDice--;
        SetActiveDice(_amountOfActiveDice);
        DiceButtonPressAction?.Invoke(_diceNumber);
    }

    public void SetAnimationSpeed(float animatorSpeed)
    {
        _trayAnimator.speed = animatorSpeed;
    }
    
    public void OpenTray()
    {
        _trayAnimator.SetBool("Opened", true);
    }

    public void CloseTray()
    {
        _trayAnimator.SetBool("Opened", false);
    }

    public void IncreaseActiveDice()
    {
        _amountOfActiveDice++;
        SetActiveDice(_amountOfActiveDice);
    }

    public void ResetDice()
    {
        SetActiveDice(0);
    }

    public void SetActiveDice(float amount)
    {
        if (amount > 5 || amount < 0)
        {
            Debug.LogError($"amount is too low or too high, I don't care which one something is wrong: {amount}");
        }
        else
        {
            _amountOfActiveDice = amount;
            _firstDice.gameObject.SetActive(amount >= 1);
            _secondDice.gameObject.SetActive(amount >= 2);
            _thirdDice.gameObject.SetActive(amount >= 3);
            _fourDice.gameObject.SetActive(amount >= 4);
            _fiveDice.gameObject.SetActive(amount >= 5);
        }
    }
    
}
