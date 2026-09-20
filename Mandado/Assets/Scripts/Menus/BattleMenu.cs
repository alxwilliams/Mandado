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
    [SerializeField] private float _trayOpeningWaitBetween = .1f;

    private Action _rollDiceAction;
    private Action _attackAction;

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
    
    public virtual void Initialize(MenuSystem menuSystem, Action rollDice, Action attack)
    {
        _attackAction = attack;
        _rollDiceAction = rollDice;
        _goButton.onClick.AddListener(Attack);
        _rollDiceButton.onClick.AddListener(RollDice);
        base.Initialize(menuSystem);
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
        _characterUIs[index].SetActiveDice(num);
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
