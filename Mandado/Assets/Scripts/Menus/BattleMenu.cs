using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : BaseMenu
{
    [SerializeField] private TMP_Text _debugText;
    [SerializeField] private TMP_Text _diceText;
    [SerializeField] private Button _rollDiceButton;
    [SerializeField] private Button _goButton;

    private Action _rollDiceAction;
    private Action _attackAction;
    
    public virtual void Initialize(MenuSystem menuSystem, Action rollDice, Action attack)
    {
        _attackAction = attack;
        _rollDiceAction = rollDice;
        _goButton.onClick.AddListener(Attack);
        _rollDiceButton.onClick.AddListener(RollDice);
        base.Initialize(menuSystem);
    }
    
    public void UpdateDebugText(string text)
    {
        _debugText.text = text;
    }

    public void UpdateDiceText(string text)
    {
        _diceText.text = text;
    }

    private void RollDice()
    {
        _rollDiceAction?.Invoke();
    }
    
    private void Attack()
    {
        _attackAction?.Invoke();
    }
}
