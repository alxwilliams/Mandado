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
        for (int i = 0; i < 6; i++)
        {
            if (_diceRolls[i] > 0)
            {
                if (_diceRolls[i] == 1)
                {
                    DealWithAction(_currentPlayerCharacters[i].actionSet.rollOneActions);
                }
                else if (_diceRolls[i] == 2)
                {
                    DealWithAction(_currentPlayerCharacters[i].actionSet.rollTwoActions);
                }
                else if (_diceRolls[i] == 3)
                {
                    DealWithAction(_currentPlayerCharacters[i].actionSet.rollThreeActions);
                }
                else if (_diceRolls[i] == 4)
                {
                    DealWithAction(_currentPlayerCharacters[i].actionSet.rollFourActions);
                }
                else if (_diceRolls[i] == 5)
                {
                    DealWithAction(_currentPlayerCharacters[i].actionSet.rollFiveActions);
                }
            }
            else
            {
                //add focus point to character
            }
        }
        ShowCurrentHealth();
    }

    private void DealWithAction(List<CharacterAction> actions)
    {
        foreach (var action in actions)
        {
            if (action.type == ActionType.Damage)
            {
                DealDamageToEnemy(action.value);
            }
        }
    }

    private void DealDamageToEnemy(float num)
    {
        _currentEnemyCharacters[Random.Range(0, _currentEnemyCharacters.Count)].currentHealth -= num;
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
    
    
    
}
