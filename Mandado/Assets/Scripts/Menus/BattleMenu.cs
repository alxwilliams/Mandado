using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : BaseMenu
{
    [SerializeField] private TMP_Text _debugEnemyText;

    [SerializeField] private Button _rollDiceButton;
    [SerializeField] private Button _orderButton;
    [SerializeField] private Button _goButton;
    [SerializeField] private EnemyCharacterUI _enemyCharacterUI;
    [SerializeField] private List<PlayerCharacterUI> _characterUIs = new List<PlayerCharacterUI>();
    [SerializeField] private float _trayOpeningWaitBetween = .1f;
    [SerializeField] private List<DiceTrayDiceButton> _diceTrayButtons = new List<DiceTrayDiceButton>();
    [SerializeField] private TMP_Text _rerollText;
    [SerializeField] private TMP_Text _orderTokenText;

    
    [Header("Dice Sprites")] 
    [SerializeField] private Sprite _diceOne;
    [SerializeField] private Sprite _diceTwo;
    [SerializeField] private Sprite _diceThree;
    [SerializeField] private Sprite _diceFour;
    [SerializeField] private Sprite _diceFive;
    [SerializeField] private Sprite _diceSix;
    
    private Dictionary<DiceTrayDiceButton, int> _diceTrayDictionary = new Dictionary<DiceTrayDiceButton, int>();
    private BattleSystem _battleSystem;
    
    private Action _rollDiceAction;
    private Action _attackAction;
    private Action<int> _increaseDiceRollsAction;
    private Action<int> _decreaseDiceRollsAction;

    private Coroutine _trayOpeningRoutine;
    

    public void UpdatePlayerCharacterGuardUI(int index, float guard)
    {
        _characterUIs[index].UpdateGuardUI(guard);
    }

    public void UpdatePlayerCharacterBleedUI(int index, float newValue)
    {
        _characterUIs[index].UpdateBleedUI(newValue);
    }

    public void UpdateOrderTokenText(float num)
    {
        _orderTokenText.text = $"{num}";
    }

    public void UpdateEnemyBleedUI(float newValue)
    {
        _enemyCharacterUI.UpdateBleedUI(newValue);
    }

    public void LoadInStatusEffects(BattleSystemState state)
    {
        for (int i = 0; i < state.playerCharacters.Count; i++)
        {
            if(state.playerCharacters[i].statusEffects.ContainsKey(StatusEffects.Bleed))
            {
                _characterUIs[i].UpdateBleedUI(state.playerCharacters[i].statusEffects[StatusEffects.Bleed]);
            }
            else
            {
                _characterUIs[i].UpdateBleedUI(0);
            }
            
            if(state.playerCharacters[i].statusEffects.ContainsKey(StatusEffects.Guard))
            {
                _characterUIs[i].UpdateGuardUI(state.playerCharacters[i].statusEffects[StatusEffects.Guard]);
            }
            else
            {
                _characterUIs[i].UpdateGuardUI(0);
            }
        }

        if (state.enemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Bleed))
        {
            _enemyCharacterUI.UpdateBleedUI(state.enemyCharacters[0].statusEffects[StatusEffects.Bleed]);
        }
        else
        {
            _enemyCharacterUI.UpdateBleedUI(0);
        }
        
        if (state.enemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Guard))
        {
            _enemyCharacterUI.UpdateGuardUI(state.enemyCharacters[0].statusEffects[StatusEffects.Guard]);
        }
        else
        {
            _enemyCharacterUI.UpdateGuardUI(0);
        }
    }

    public virtual void Initialize(MenuSystem menuSystem, Action rollDice, Action attack, Action<int> increaseDiceRolls, Action<int> decreaseDiceRolls)
    {
        _battleSystem = GameManager.Instance.BattleSystem;
        _attackAction = attack;
        _rollDiceAction = rollDice;
        _increaseDiceRollsAction = increaseDiceRolls;
        _decreaseDiceRollsAction = decreaseDiceRolls;
        
        _goButton.onClick.AddListener(Attack);
        _rollDiceButton.onClick.AddListener(RollDice);

        for (int i = 0; i < 6; i++)
        {
            _characterUIs[i].Initialize(i+1,ReturnDice);
        }

        for (int i = 0; i < _diceTrayButtons.Count; i++)
        {
            int index = i;
            DiceTrayDiceButton button = _diceTrayButtons[i];
            
            _diceTrayButtons[i].MainButton.onClick.AddListener(() => OnDiceButtonClicked(index, button));
            _diceTrayDictionary.Add(_diceTrayButtons[i],0);
        }
        
        base.Initialize(menuSystem);
    }

    public void SetRerollNumber(int num)
    {
        _rerollText.text = $"{num}";
    }
    
    private void OnDiceButtonClicked(int index, DiceTrayDiceButton button)
    {
        if (_battleSystem.IsIndexCharacterEmpty(_diceTrayDictionary[button] - 1))
        {
            return;
        }
        
        button.gameObject.SetActive(false);
        SetDiceInCharacterUI(index, _diceTrayDictionary[button]);
    }

    private void ReturnDice(int diceNum)
    {
        for(int i = 0; i< _diceTrayButtons.Count;i++)
        {
            if (!_diceTrayButtons[i].gameObject.activeSelf)
            {
                SetDiceInTrayUI(i, diceNum);
                _decreaseDiceRollsAction?.Invoke(diceNum-1);
                break;
            }
        }
    }

    public void LoadInCharacterUI(List<PlayerCharacterData> characters)
    {
        for (int i = 0; i < 6; i++)
        {
            if (characters[i].classType != ClassType.Empty)
            {
                _characterUIs[i].SetActive(true);
                _characterUIs[i].SetLabel(characters[i].characterLabel);
            }
            else
            {
                _characterUIs[i].SetActive(false);
            }
        }
    }

    public void SetDiceInCharacterUI(int index, int num)
    {
        _increaseDiceRollsAction?.Invoke(num-1);
        _characterUIs[num-1].IncreaseActiveDice();
    }

    public void ResetDiceTrays()
    {
        foreach (var button in _diceTrayButtons)
        {
            button.gameObject.SetActive(false);
        }

        foreach (var characterUI in _characterUIs)
        {
            characterUI.ResetDice();
        }
    }

    public void DisableDice(int index)
    {
        var dice = _diceTrayButtons[index].gameObject;
        
        if(dice.activeSelf)
        {
            _diceTrayButtons[index].gameObject.SetActive(false);
        }
    }

    public void SetDiceInTrayUI(int index, int num)
    {
        DiceTrayDiceButton button = _diceTrayButtons[index];
        
        if (num != -1)
        {
            button.gameObject.SetActive(true);

            if (num == 1)
            {
                button.MainButton.image.sprite = _diceOne;
            }
            else if (num == 2)
            {
                button.MainButton.image.sprite = _diceTwo;
            }
            else if (num == 3)
            {
                button.MainButton.image.sprite = _diceThree;
            }
            else if (num == 4)
            {
                button.MainButton.image.sprite = _diceFour;
            }
            else if (num == 5)
            {
                button.MainButton.image.sprite = _diceFive;
            }
            else if (num == 6)
            {
                button.MainButton.image.sprite = _diceSix;
            }

            _diceTrayDictionary[button] = num;
        }
        else
        {
            button.gameObject.SetActive(false);
        }
    }

    public void UpdateEnemyUI(EnemyCharacterData data)
    {
        _enemyCharacterUI.UpdateHealth(data.currentHealth/data.maxHealth);
        _enemyCharacterUI.SetFocus(data.currentFocus);
    }
    
    public void UpdatePlayerCharacters(List<PlayerCharacterData> characters)
    {
        for(int i = 0; i < characters.Count; i++)
        {
            UpdatePlayerCharacterHealth(characters[i]);
            UpdatePlayerCharacterFocus(characters[i]);
        }
    }

    public void UpdatePlayerCharacterHealth(PlayerCharacterData data)
    {
        _characterUIs[data.currentIndex].UpdateHealth(data.currentHealth / data.maxHealth);
    }

    public void UpdatePlayerCharacterFocus(PlayerCharacterData data)
    {
        if (data.classType == ClassType.Empty)
        {
            return;
        }
        _characterUIs[data.currentIndex].SetFocus(data.currentFocus);
    } 

    public void UpdateEnemyDebugText(string text)
    {
        _debugEnemyText.text = text;
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
