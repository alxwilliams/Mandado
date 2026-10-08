using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : BaseMenu
{

    [SerializeField] private Button _rollDiceButton;
    [SerializeField] private Button _orderButton;
    [SerializeField] private Button _goButton;
    [SerializeField] private EnemyCharacterUI _enemyCharacterUI;
    [SerializeField] private List<PlayerCharacterUI> _characterUIs = new List<PlayerCharacterUI>();
    [SerializeField] private float _trayOpeningWaitBetween = .1f;
    [SerializeField] private List<DiceTrayDiceButton> _diceTrayButtons = new List<DiceTrayDiceButton>();
    [SerializeField] private TMP_Text _rerollText;
    [SerializeField] private TMP_Text _orderTokenText;
    [SerializeField] private TMP_Text _enemyIntentDebugText;

    
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

    private bool _orderActivated = false;
    

    public void UpdatePlayerCharacterGuardUI(int index, float guard)
    {
        _characterUIs[index].UpdateGuardUI(guard);
    }

    public void UpdatePlayerCharacterBleedUI(int index, float newValue)
    {
        _characterUIs[index].UpdateBleedUI(newValue);
    }
    
    public void UpdatePlayerCharacterEmpowerUI(int index, float newValue)
    {
        _characterUIs[index].UpdateEmpowerUI(newValue);
    }

    public void UpdateOrderTokenText(float num)
    {
        _orderTokenText.text = $"{num}";
    }

    public void UpdateEnemyBleedUI(float newValue)
    {
        _enemyCharacterUI.UpdateBleedUI(newValue);
    }
    
    public void UpdateEnemyEmpowerUI(float newValue)
    {
        _enemyCharacterUI.UpdateEmpowerUI(newValue);
    }

    public void UpdatePlayerCharacterStatusEffects(BattleSystemState state, int characterIndex)
    {
        if(state.playerCharacters[characterIndex].statusEffects.ContainsKey(StatusEffects.Bleed))
        {
            _characterUIs[characterIndex].UpdateBleedUI(state.playerCharacters[characterIndex].statusEffects[StatusEffects.Bleed]);
        }
        else
        {
            _characterUIs[characterIndex].UpdateBleedUI(0);
        }
            
        if(state.playerCharacters[characterIndex].statusEffects.ContainsKey(StatusEffects.Guard))
        {
            _characterUIs[characterIndex].UpdateGuardUI(state.playerCharacters[characterIndex].statusEffects[StatusEffects.Guard]);
        }
        else
        {
            _characterUIs[characterIndex].UpdateGuardUI(0);
        }
            
        if(state.playerCharacters[characterIndex].currentDamageMultiplier > 1)
        {
            _characterUIs[characterIndex].UpdateEmpowerUI((state.playerCharacters[characterIndex].currentDamageMultiplier - 1)* 100);
        }
        else
        {
            _characterUIs[characterIndex].UpdateEmpowerUI(0);
        }
    }

    public void LoadInStatusEffects(BattleSystemState state)
    {
        for (int i = 0; i < state.playerCharacters.Count; i++)
        {
            UpdatePlayerCharacterStatusEffects(state, i);
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
        _orderButton.onClick.AddListener(OnOrderButtonClicked);

        for (int i = 0; i < 6; i++)
        {
            _characterUIs[i].Initialize(i+1,ReturnDice,SwapCharacterUI);
        }

        for (int i = 0; i < _diceTrayButtons.Count; i++)
        {
            int index = i;
            DiceTrayDiceButton button = _diceTrayButtons[i];
            button.SetUpButton(DecreaseDiceValue,IncreaseDiceValue,DisableOrderScreen);
            
            _diceTrayButtons[i].MainButton.onClick.AddListener(() => OnDiceButtonClicked(index, button));
            _diceTrayDictionary.Add(_diceTrayButtons[i],0);
        }
        
        base.Initialize(menuSystem);
    }

    public void OnOrderButtonClicked()
    {
        if ((!_orderActivated && _battleSystem.IsOrderUsable) || _orderActivated)
        {
            _orderActivated = !_orderActivated;
            SetOrderButtons(_orderActivated);
        }
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

    public void ResetTargetIndicators()
    {
        foreach (var ui in _characterUIs)
        {
            ui.SetTargetIndicator(false);
        }
        _enemyIntentDebugText.text = "";
    }

    public void SetOrderButtons(bool active)
    {
        foreach (var button in _diceTrayButtons)
        {
            button.SetOrderButtonsActiveState(active);
        }

        foreach (var ui in _characterUIs)
        {
            ui.SetOrderButtons(active);
        }
    }

    public void KillPlayer(int index)
    {
        _characterUIs[index].SetDead();
    }

    public void ShowEnemyIntent(List<EnemyCharacterAction> actions)
    {
        _enemyIntentDebugText.text = "";
        
        foreach (var action in actions)
        {
            if (action.type == EnemyActionType.DamageRandom)
            {
                _enemyIntentDebugText.text += "Damaging Target ";
                _characterUIs[action.targetIndex].SetTargetIndicator(true);
            }
            
            if (action.type == EnemyActionType.BleedRandom)
            {
                _enemyIntentDebugText.text += "Bleeding Target ";
                _characterUIs[action.targetIndex].SetTargetIndicator(true);
            }

            if (action.type == EnemyActionType.Heal)
            {
                _enemyIntentDebugText.text += "Healing Self ";
            }

            if (action.type == EnemyActionType.BleedAll)
            {
                _enemyIntentDebugText.text += "Bleeding All ";
                foreach (var ui in _characterUIs)
                {
                    ui.SetTargetIndicator(true);
                }
            }

            if (action.type == EnemyActionType.Focus)
            {
                _enemyIntentDebugText.text += "Gaining Focus ";
            }
        }

    }

    public void LoadInCharacterUI(List<PlayerCharacterData> characters, EnemyCharacterData enemy)
    {
        for (int i = 0; i < 6; i++)
        {
            if (characters[i].classType != ClassType.Empty && characters[i].IsAlive)
            {
                _characterUIs[i].SetActive(true);
                _characterUIs[i].SetLabel(characters[i].characterSpriteData._characterLabel);
            }else if(!characters[i].IsAlive)
            {
                _characterUIs[i].SetDead();
            }
            else
            {
                _characterUIs[i].SetEmpty();
            }
            
            _characterUIs[i].ClassType = characters[i].classType;
        }
        
        _enemyCharacterUI.SetLabel(enemy.characterSpriteData._characterLabel);
    }

    private void SwapCharacterUI(int index1, int index2)
    {
        PlayerCharacterUI character1 = _characterUIs[index1];
        PlayerCharacterUI character2 = _characterUIs[index2];
        
        character1.SwapCharacter(character2);
        _battleSystem.SwapCharacters(index1,index2);
        
        DisableOrderScreen();
    }

    public void SetDiceInCharacterUI(int index, int num)
    {
        _increaseDiceRollsAction?.Invoke(num-1);
        _characterUIs[num-1].IncreaseActiveDice();
    }

    public void StartBattle()
    {
        SetOrderButtons(false);
        ResetDiceTrays();
    }

    public void UpdateEnemyUI(EnemyCharacterData data)
    {
        _enemyCharacterUI.UpdateHealth(data.currentHealth,data.maxHealth);
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
        _characterUIs[data.currentIndex].UpdateHealth(data.currentHealth,data.maxHealth);
    }

    public void UpdatePlayerCharacterFocus(PlayerCharacterData data)
    {
        if (data.classType == ClassType.Empty || !data.IsAlive)
        {
            return;
        }
        _characterUIs[data.currentIndex].SetFocus(data.currentFocus);
    }

    

    private void DisableOrderScreen()
    {
        _orderActivated = false;
        SetOrderButtons(_orderActivated);
    }

    private void RollDice()
    {
        _rollDiceAction?.Invoke();
    }
    
    private void Attack()
    {
        _attackAction?.Invoke();
    }

    #region Dice
    
    private void IncreaseDiceValue(int index, int value)
    {
        SetDiceInTrayUI(index, value + 1);
        _battleSystem.UseOrderToken();
    }

    private void DecreaseDiceValue(int index, int value)
    {
        SetDiceInTrayUI(index, value - 1);
        _battleSystem.UseOrderToken();
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

    public void ResetDice(int index)
    {
        _characterUIs[index].ResetDice();
    }
    

    public void DisableDice(int index)
    {
        var dice = _diceTrayButtons[index].gameObject;
        
        if(dice.activeSelf)
        {
            _diceTrayButtons[index].gameObject.SetActive(false);
        }
    }

    public void SetDiceInTrayUI(int index, int value)
    {
        DiceTrayDiceButton button = _diceTrayButtons[index];
        
        if (value != -1)
        {
            if(!button.gameObject.activeSelf)
            {
                button.gameObject.SetActive(true);
            }

            if (value == 1)
            {
                button.MainButton.image.sprite = _diceOne;
            }
            else if (value == 2)
            {
                button.MainButton.image.sprite = _diceTwo;
            }
            else if (value == 3)
            {
                button.MainButton.image.sprite = _diceThree;
            }
            else if (value == 4)
            {
                button.MainButton.image.sprite = _diceFour;
            }
            else if (value == 5)
            {
                button.MainButton.image.sprite = _diceFive;
            }
            else if (value == 6)
            {
                button.MainButton.image.sprite = _diceSix;
            }

            button.SetDiceValue(index,value);
            _diceTrayDictionary[button] = value;
        }
        else
        {
            button.gameObject.SetActive(false);
        }
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
    
    #endregion
}
