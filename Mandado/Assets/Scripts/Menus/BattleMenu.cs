using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : BaseMenu
{
    [SerializeField] private TMP_Text _debugPlayerText;
    [SerializeField] private TMP_Text _debugEnemyText;
    
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
    
    public void UpdatePlayerDebugText(string text)
    {
        _debugPlayerText.text = text;
    }
    
    public void UpdateEnemyDebugText(string text)
    {
        _debugEnemyText.text = text;
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
