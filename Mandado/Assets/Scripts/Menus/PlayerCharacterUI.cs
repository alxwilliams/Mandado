using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterUI : CharacterUI
{
    [SerializeField] private Animator _trayAnimator;
    [SerializeField] private GameObject _targetIndicator;
    
    [SerializeField] private Button _orderLeftButton;
    [SerializeField] private Button _orderRightButton;
    
    [Header("Dice")]
    [SerializeField] private PlayerCharacterDiceButton _firstDice;
    [SerializeField] private PlayerCharacterDiceButton _secondDice;
    [SerializeField] private PlayerCharacterDiceButton _thirdDice;
    [SerializeField] private PlayerCharacterDiceButton _fourDice;
    [SerializeField] private PlayerCharacterDiceButton _fiveDice;


    private ClassType _classType;
    private Action<int,int> _swapCharacterAction;
    private bool _isDead = false;
    
    private int _diceNumber;
    private float _amountOfActiveDice = 0;

    private Action<int> DiceButtonPressAction;

    public bool IsDead
    {
        get => _isDead;
        set => _isDead = value;
    }

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
        
        _firstDice.Button.onClick.AddListener(OnDiceButtonPress);
        _secondDice.Button.onClick.AddListener(OnDiceButtonPress);
        _thirdDice.Button.onClick.AddListener(OnDiceButtonPress);
        _fourDice.Button.onClick.AddListener(OnDiceButtonPress);
        _fiveDice.Button.onClick.AddListener(OnDiceButtonPress);
        
        _swapCharacterAction = swapCharacters;
        
        _orderLeftButton.onClick.AddListener(OnLeftOrderButtonClicked);
        _orderRightButton.onClick.AddListener(OnRightOrderButtonClicked);
        _targetIndicator.SetActive(false);
        Initialize();
    }

    public void SetEmpty()
    {
        _uiParent.SetActive(false);
    }
    
    
    public void SetTargetIndicator(bool active)
    {
        if (_targetIndicator.activeSelf != active)
        {
            _targetIndicator.SetActive(active);
        }
    }

    public void SetDead()
    {
        _isDead = true;
        _uiParent.SetActive(false);
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
        bool character2Empty = character2.ClassType == ClassType.Empty || character2.IsDead;
        
        if (character2Empty)
        {
            character2.UIParent.SetActive(true);
            UIParent.SetActive(false);
        }
        
        (character2.IsDead, _isDead) = (_isDead, character2.IsDead);
        (character2.LabelImage.sprite, _labelImage.sprite) = (_labelImage.sprite, character2.LabelImage.sprite);
        (character2.AmountOfActiveDice, _amountOfActiveDice) = (_amountOfActiveDice, character2.AmountOfActiveDice);
        (character2.ClassType, _classType) = (_classType, character2.ClassType);

        float num = character2.CurrentHealth;
        float num2 = character2.MaxHealth;
        
        character2.UpdateHealth(_currentHealth,_maxHealth);
        UpdateHealth(num, num2);

        num = character2.CurrentBleed;
        character2.UpdateBleedUI(_currentBleed);
        UpdateBleedUI(num);
        
        num = character2.CurrentEmpower;
        character2.UpdateEmpowerUI(_currentEmpower);
        UpdateEmpowerUI(num);
        
        num = character2.CurrentFocus;
        character2.UpdateBleedUI(_currentFocus);
        SetFocus(num);
        
        num = character2.CurrentGuard;
        character2.UpdateBleedUI(_currentGuard);
        UpdateGuardUI(num);

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

            if (!_firstDice.Image.enabled && amount >= 1)
            {
                _firstDice.Animator.SetTrigger("Pressed");
            }else if (_firstDice.Image.enabled && amount < 1)
            {
                _firstDice.Animator.SetTrigger("Gone");
            }
            
            if (!_secondDice.Image.isActiveAndEnabled && amount >= 2)
            {
                _secondDice.Animator.SetTrigger("Pressed");
            }
            else if (_secondDice.Image.enabled && amount < 2)
            {
                _secondDice.Animator.SetTrigger("Gone");
            }
            
            if (!_thirdDice.Image.isActiveAndEnabled && amount >= 3)
            {
                _thirdDice.Animator.SetTrigger("Pressed");
            }
            else if (_thirdDice.Image.enabled && amount < 3)
            {
                _thirdDice.Animator.SetTrigger("Gone");
            }
            
            if (!_fourDice.Image.isActiveAndEnabled && amount >= 4)
            {
                _fourDice.Animator.SetTrigger("Pressed");
            }
            else if (_fourDice.Image.enabled && amount < 4)
            {
                _fourDice.Animator.SetTrigger("Gone");
            }
            
            if (!_fiveDice.Image.isActiveAndEnabled && amount >= 5)
            {
                _fiveDice.Animator.SetTrigger("Pressed");
            }
            else if (_fiveDice.Image.enabled && amount < 5)
            {
                _fiveDice.Animator.SetTrigger("Gone");
            }
            
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

    private void OnDestroy()
    {
        _firstDice.Button.onClick.RemoveAllListeners();
        _secondDice.Button.onClick.RemoveAllListeners();
        _thirdDice.Button.onClick.RemoveAllListeners();
        _fourDice.Button.onClick.RemoveAllListeners();
        _fiveDice.Button.onClick.RemoveAllListeners();
    }
}
