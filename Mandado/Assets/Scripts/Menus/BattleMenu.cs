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
    [SerializeField] private Button _goButton;
    [SerializeField] private List<PlayerCharacterUI> _characterUIs = new List<PlayerCharacterUI>();
    [SerializeField] private float _trayOpeningWaitBetween = .1f;
    [SerializeField] private List<Button> _diceTrayButtons = new List<Button>();
    [SerializeField] private TMP_Text _rerollText;

    
    [Header("Dice Sprites")] 
    [SerializeField] private Sprite _diceOne;
    [SerializeField] private Sprite _diceTwo;
    [SerializeField] private Sprite _diceThree;
    [SerializeField] private Sprite _diceFour;
    [SerializeField] private Sprite _diceFive;
    [SerializeField] private Sprite _diceSix;
    
    private Dictionary<Button, int> _diceTrayDictionary = new Dictionary<Button, int>();
    
    private Action _rollDiceAction;
    private Action _attackAction;
    private Action<int> _increaseDiceRollsAction;
    private Action<int> _decreaseDiceRollsAction;

    private Coroutine _trayOpeningRoutine;
    
    
    
    public void CloseTrays()
    {
        if (_trayOpeningRoutine != null)
        {
            StopCoroutine(_trayOpeningRoutine);
        }

        _trayOpeningRoutine = StartCoroutine(TrayRoutine(false));
    }

    public void OpenTrays()
    {
        if (_trayOpeningRoutine != null)
        {
            StopCoroutine(_trayOpeningRoutine);
        }

        _trayOpeningRoutine = StartCoroutine(TrayRoutine(true));
    }

    public void UpdateCharacterGuardUI(int index, float guard)
    {
        _characterUIs[index].UpdateGuardUI(guard);
    }

    private IEnumerator TrayRoutine(bool opening)
    {
        int i = 1;
        foreach (var UI in _characterUIs)
        {
            if (i == _characterUIs.Count)
            {
                yield return new WaitForSeconds(_trayOpeningWaitBetween * .65f);
            }
            else if (i == _characterUIs.Count - 1)
            {
                yield return new WaitForSeconds(_trayOpeningWaitBetween * .85f);
            }
            else
            {
                yield return new WaitForSeconds(_trayOpeningWaitBetween);
            }

            if(opening)
            {
                UI.OpenTray();
            }
            else
            {
                UI.CloseTray();
            }

            i++;
        }

        _trayOpeningRoutine = null;
    }
    
    public virtual void Initialize(MenuSystem menuSystem, Action rollDice, Action attack, Action<int> increaseDiceRolls, Action<int> decreaseDiceRolls)
    {
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
            Button button = _diceTrayButtons[i];
            
            _diceTrayButtons[i].onClick.AddListener(() => OnDiceButtonClicked(index, button));
            _diceTrayDictionary.Add(_diceTrayButtons[i],0);
        }
        
        base.Initialize(menuSystem);
    }

    public void SetRerollNumber(int num)
    {
        _rerollText.text = $"{num}";
    }
    
    private void OnDiceButtonClicked(int index, Button button)
    {
        button.gameObject.SetActive(false);
        SetDiceInCharacterUI(index, _diceTrayDictionary[button]);
    }

    private void Reroll()
    {
        _rollDiceAction?.Invoke();
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

    private void InitializeTrayAnimationSpeeds(int amount)
    {
        if (amount > 1)
        {
            for (int i = 0; i < amount; i++)
            {
                if (i == amount - 1)
                {
                    _characterUIs[i].SetAnimationSpeed(2.2f);
                }else if (i == amount -2)
                {
                    _characterUIs[i].SetAnimationSpeed(1.8f);
                }
                else
                {
                    _characterUIs[i].SetAnimationSpeed(1);
                }
            }
            
        }
        else
        {
            _characterUIs[0].SetAnimationSpeed(1);
        }
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

            InitializeTrayAnimationSpeeds(amount);
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

    public void SetDiceInTrayUI(int index, int num)
    {
        Button button = _diceTrayButtons[index];
        
        if (num != -1)
        {
            button.gameObject.SetActive(true);

            if (num == 1)
            {
                button.image.sprite = _diceOne;
            }
            else if (num == 2)
            {
                button.image.sprite = _diceTwo;
            }
            else if (num == 3)
            {
                button.image.sprite = _diceThree;
            }
            else if (num == 4)
            {
                button.image.sprite = _diceFour;
            }
            else if (num == 5)
            {
                button.image.sprite = _diceFive;
            }
            else if (num == 6)
            {
                button.image.sprite = _diceSix;
            }

            _diceTrayDictionary[button] = num;
        }
        else
        {
            button.gameObject.SetActive(false);
        }
    }
    
    public void UpdatePlayerCharacters(List<PlayerCharacterData> characters)
    {
        for(int i = 0; i < characters.Count; i++)
        {
            _characterUIs[i].UpdateHealth(characters[i].currentHealth / characters[i].maxHealth);
            _characterUIs[i].SetFocus(characters[i].currentFocus);
        }
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
