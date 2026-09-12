using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleSystem : BaseSystem
{
    [SerializeField] private int _amountOfDiceRolled = 5;
    [SerializeField] private List<BaseCharacter> _fakePlayerData = new List<BaseCharacter>();
    [SerializeField] private List<BaseCharacter> _fakeEnemyData = new List<BaseCharacter>();
    [SerializeField] private FieldController _fieldController;
    [SerializeField] private BattleMenu _battleMenu;
    
    private List<CharacterData> _currentPlayerCharacters;
    private List<CharacterData> _currentEnemyCharacters;

    private Coroutine _attackRoutine;

    private int[] _diceRolls = new int[] {0,0,0,0,0,0};

    public override void Initialize(GameManager gameManager)
    {
        _battleMenu.Initialize(gameManager.MenuSystem, RollDice, PlayerAttack);
        base.Initialize(gameManager);
    }
    private void Start()
    {
        LoadCharacters(_fakePlayerData, _fakeEnemyData);
    }

    public void LoadCharacters(List<BaseCharacter> playerCharacters, List<BaseCharacter> enemyCharacters)
    {
        List<CharacterData> playerData = new List<CharacterData>(); 
        List<CharacterData> enemyData = new List<CharacterData>(); 
        
        _fieldController.WipeCharacterDictionary();
        
        foreach (var character in playerCharacters)
        {
            playerData.Add(character.GetFullHealthCharacterData());
        }
        
        _fieldController.LoadPlayerCharacters(playerData);
        _currentPlayerCharacters = playerData;
        
        foreach (var character in enemyCharacters)
        {
            enemyData.Add(character.GetFullHealthCharacterData());
        }
        
        _fieldController.LoadEnemyCharacters(enemyData);
        _currentEnemyCharacters = enemyData;
        ShowCurrentHealth();
    }

    [ContextMenu("Show health")]
    public void ShowCurrentHealth()
    {
        string testString = "";


        foreach (var character in _currentPlayerCharacters)
        {
            testString += character.name + ": " + character.currentHealth + "\n";
        }
        
        foreach (var character in _currentEnemyCharacters)
        {
            testString += character.name + ": " + character.currentHealth + "\n";
        }
        
        _battleMenu.UpdateDebugText(testString);
    }

    private void PlayerAttack()
    {
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
        }

        _attackRoutine = StartCoroutine(PlayerAttackRoutine());
    }
    
    private IEnumerator PlayerAttackRoutine()
    {
        for (int i = 0; i < 6; i++)
        {
            if (_diceRolls[i] > 0)
            {
                if (_diceRolls[i] == 1)
                {
                    yield return DealWithAction(_currentPlayerCharacters[i].actionSet.rollOneActions, _currentPlayerCharacters[i]);
                }
                else if (_diceRolls[i] == 2)
                {
                    yield return DealWithAction(_currentPlayerCharacters[i].actionSet.rollTwoActions, _currentPlayerCharacters[i]);
                }
                else if (_diceRolls[i] == 3)
                {
                    yield return DealWithAction(_currentPlayerCharacters[i].actionSet.rollThreeActions, _currentPlayerCharacters[i]);
                }
                else if (_diceRolls[i] == 4)
                {
                    yield return DealWithAction(_currentPlayerCharacters[i].actionSet.rollFourActions, _currentPlayerCharacters[i]);
                }
                else if (_diceRolls[i] == 5)
                {
                    yield return DealWithAction(_currentPlayerCharacters[i].actionSet.rollFiveActions, _currentPlayerCharacters[i], true);
                }

                ShowCurrentHealth();
                yield return new WaitForSeconds(.25f);
            }
            else
            {
                //add focus point to character
            }
        }
        ShowCurrentHealth();
    }

    private IEnumerator DealWithAction(List<CharacterAction> actions, CharacterData data, bool maxRoll = false)
    {
        foreach (var action in actions)
        {
            if (action.type == ActionType.Damage)
            {
                if(!maxRoll)
                {
                    yield return new WaitForSeconds(_fieldController.CharacterAttack(data));
                    DealDamageToEnemy(action.value);
                }
                else
                {
                    yield return new WaitForSeconds(_fieldController.CharacterBigAttack(data));
                    DealDamageToEnemy(action.value);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void DealDamageToEnemy(float num)
    {
        int enemyIndex = Random.Range(0, _currentEnemyCharacters.Count);
        _currentEnemyCharacters[enemyIndex].currentHealth -= num;
        _fieldController.CharacterTakeDamage(_currentEnemyCharacters[enemyIndex], num);
    }
    
    private void RollDice()
    {
        int[] dice = new int[_amountOfDiceRolled];
        _diceRolls = new[] { 0, 0, 0, 0, 0, 0};
        string debugString = "";

        //only rolling 5 dice
        for(int i =0; i < _amountOfDiceRolled; i++)
        {
            dice[i] = Random.Range(1, 6);
            _diceRolls[dice[i] - 1]++;
            debugString += $"Dice{i + 1}: {dice[i]}\n";
        }
        
        _battleMenu.UpdateDiceText(debugString);
    }

    public void UpdateBattleMenuText(string text)
    {
        _battleMenu.UpdateDebugText(text);
    }

    public void ShowBattleMenu()
    {
        _battleMenu.Show(true);
    }

    private void OnDestroy()
    {
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
        }
    }
}
