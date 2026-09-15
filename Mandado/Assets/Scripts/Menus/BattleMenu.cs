using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : BaseMenu
{
    [SerializeField] private TMP_Text _debugEnemyText;
    
    [SerializeField] private TMP_Text _diceText;
    [SerializeField] private Button _rollDiceButton;
    [SerializeField] private Button _goButton;
    [SerializeField] private List<PlayerCharacterUI> _characterUIs = new List<PlayerCharacterUI>();

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

    public void SetPlayerAmount(int amount)
    {
        if (amount > 6 || amount < 0)
        {
            Debug.LogError($"amount is too low or too high, I don't care which one something is wrong: {amount}");
        }
        else
        {
            _characterUIs[0].SetActive(amount >= 1);
            _characterUIs[1].SetActive(amount >= 2);
            _characterUIs[2].SetActive(amount >= 3);
            _characterUIs[3].SetActive(amount >= 4);
            _characterUIs[4].SetActive(amount >= 5);
            _characterUIs[5].SetActive(amount >= 6);
        }
    }
    
    public void UpdatePlayerCharacters(List<PlayerCharacterData> characters)
    {
        for(int i = 0; i < characters.Count; i++)
        {
            _characterUIs[i].UpdateHealth(characters[i].currentHealth / characters[i].maxHealth);
            _characterUIs[i].UpdateFocus(characters[i].currentFocus);
        }
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
