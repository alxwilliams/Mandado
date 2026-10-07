using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterUI : CharacterUI
{
    [SerializeField] private Animator _trayAnimator;
    
    [SerializeField] private Button _orderLeftButton;
    [SerializeField] private Button _orderRightButton;
    
    
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

    private ClassType _classType;
    private Action<int,int> _swapCharacterAction;
    
    private int _diceNumber;
    private float _amountOfActiveDice = 0;

    private Action<int> DiceButtonPressAction;

    public ClassType ClassType
    {
        get => _classType;
        set => _classType = value;
    }

    public float AmountOfActiveDice
    {
        get => _amountOfActiveDice;
        set => _amountOfActiveDice = value;
    }


    public void Initialize(int diceNum, Action<int> diceButtonPress, Action<int,int> swapCharacters)
    {
        _diceNumber = diceNum;
        DiceButtonPressAction = diceButtonPress;
        
        _firstButtons.onClick.AddListener(OnDiceButtonPress);
        _secondButtons.onClick.AddListener(OnDiceButtonPress);
        _thirdButtons.onClick.AddListener(OnDiceButtonPress);
        _fourButtons.onClick.AddListener(OnDiceButtonPress);
        _fiveButtons.onClick.AddListener(OnDiceButtonPress);
        
        _swapCharacterAction = swapCharacters;
        
        _orderLeftButton.onClick.AddListener(OnLeftOrderButtonClicked);
        _orderRightButton.onClick.AddListener(OnRightOrderButtonClicked);
    }

    private void OnDiceButtonPress()
    {
        _amountOfActiveDice--;
        SetActiveDice(_amountOfActiveDice);
        DiceButtonPressAction?.Invoke(_diceNumber);
    }

    public void SetOrderButtons(bool active)
    {
        _orderLeftButton.gameObject.SetActive(active && _diceNumber > 1);
        _orderRightButton.gameObject.SetActive(active && _diceNumber < 6);
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

    public void SwapCharacter(PlayerCharacterUI character2)
    {
        bool character2Empty = character2.ClassType == ClassType.Empty;
        
        if (character2Empty)
        {
            character2.UIParent.SetActive(true);
            UIParent.SetActive(false);
        }
        
        (character2.LabelImage.sprite, _labelImage.sprite) = (_labelImage.sprite, character2.LabelImage.sprite);
        (character2.AmountOfActiveDice, _amountOfActiveDice) = (_amountOfActiveDice, character2.AmountOfActiveDice);
        (character2.ClassType, _classType) = (_classType, character2.ClassType);
        
        SetActiveDice(_amountOfActiveDice);
        character2.SetActiveDice(character2.AmountOfActiveDice);

        
    }

    private void SetActiveDice(float amount)
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

    private void OnLeftOrderButtonClicked()
    {
        _swapCharacterAction(_diceNumber-1, _diceNumber - 2);
    }
    
    private void OnRightOrderButtonClicked()
    {
        _swapCharacterAction(_diceNumber-1, _diceNumber);
    }
    
}
