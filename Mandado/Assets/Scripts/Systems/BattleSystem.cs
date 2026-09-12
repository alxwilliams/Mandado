using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleSystem : BaseSystem
{
    [SerializeField] private int _amountOfDiceRolled = 5;
    [SerializeField] private List<PlayerCharacter> _fakePlayerData = new List<PlayerCharacter>();
    [SerializeField] private List<EnemyCharacter> _fakeEnemyData = new List<EnemyCharacter>();
    [SerializeField] private FieldController _fieldController;
    [SerializeField] private BattleMenu _battleMenu;

    [Header("Wait Times")] 
    [SerializeField] private float _waitTimeBetweenAttacks = .25f;
    [SerializeField] private float _timeBeforeEnemyAttacks = .5f;
    
    private List<PlayerCharacterData> _currentPlayerCharacters;
    private List<EnemyCharacterData> _currentEnemyCharacters;

    private Coroutine _attackRoutine;
    private bool _canRollDice = true;
    private bool _canAttack = false;

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

    public void LoadCharacters(List<PlayerCharacter> playerCharacters, List<EnemyCharacter> enemyCharacters)
    {
        List<PlayerCharacterData> playerData = new List<PlayerCharacterData>(); 
        List<EnemyCharacterData> enemyData = new List<EnemyCharacterData>(); 
        
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
        
        _canRollDice = true;
        _canAttack = false;
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
        if (!_canAttack)
        {
            return;
        }
        
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
        }

        _attackRoutine = StartCoroutine(AttackRoutine());
    }
    
    private IEnumerator AttackRoutine()
    {
        yield return PlayerDiceActions();
        _diceRolls = new int[5];
        _battleMenu.UpdateDiceText("");
        
        //enemy attack
        EnemyRollDice();
        yield return new WaitForSeconds(_timeBeforeEnemyAttacks);
        yield return EnemyDiceActions();
        
        ShowCurrentHealth();
        
        _canAttack = false;
        _canRollDice = true;

    }

    private IEnumerator PlayerDiceActions()
    {
        for (int i = 0; i < 6; i++)
        {
            if (_diceRolls[i] > 0)
            {
                if (_diceRolls[i] == 1)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollOneActions,
                        _currentPlayerCharacters[i]);
                }
                else if (_diceRolls[i] == 2)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollTwoActions,
                        _currentPlayerCharacters[i]);
                }
                else if (_diceRolls[i] == 3)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollThreeActions,
                        _currentPlayerCharacters[i]);
                }
                else if (_diceRolls[i] == 4)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollFourActions,
                        _currentPlayerCharacters[i]);
                }
                else if (_diceRolls[i] == 5)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollFiveActions,
                        _currentPlayerCharacters[i], true);
                }

                ShowCurrentHealth();
                yield return new WaitForSeconds(_waitTimeBetweenAttacks);
            }
            else
            {
                //add focus point to character
            }
        }
    }
    
    private IEnumerator EnemyDiceActions()
    {
        //if we make more enemies we will have to change this to account for each enemy instead of _currentEnemyCharacters[0]

        int currentDiceNumber = 0;
        
        for (int i = 0; i < _currentEnemyCharacters[0].actionSet.Count; i++)
        {
            int workingDiceTotal = 0;
            int iOriginalValue = i;
            int totalDiceSpan = _currentEnemyCharacters[0].actionSet[i].diceRollSpan;

            for (int j = 0; j < totalDiceSpan; j++)
            {
                workingDiceTotal += _diceRolls[currentDiceNumber];

                if (j + 1 < totalDiceSpan)
                {
                    currentDiceNumber++;
                }
            }

            if (workingDiceTotal == 1)
            {
                yield return DealWithEnemyAction(_currentEnemyCharacters[0].actionSet[iOriginalValue]._diceActionSet.rollOneActions,
                    _currentEnemyCharacters[0]);
            }
            else if (workingDiceTotal == 2)
            {
                yield return DealWithEnemyAction(_currentEnemyCharacters[0].actionSet[iOriginalValue]._diceActionSet.rollTwoActions,
                    _currentEnemyCharacters[0]);
            }
            else if (workingDiceTotal == 3)
            {
                yield return DealWithEnemyAction(_currentEnemyCharacters[0].actionSet[iOriginalValue]._diceActionSet.rollThreeActions,
                    _currentEnemyCharacters[0]);
            }
            else if (workingDiceTotal == 4)
            {
                yield return DealWithEnemyAction(_currentEnemyCharacters[0].actionSet[iOriginalValue]._diceActionSet.rollFourActions,
                    _currentEnemyCharacters[0]);
            }
            else if (workingDiceTotal == 5)
            {
                yield return DealWithEnemyAction(_currentEnemyCharacters[0].actionSet[iOriginalValue]._diceActionSet.rollFiveActions,
                    _currentEnemyCharacters[0], true);
            }

            currentDiceNumber++;
            
        }
    }

    private IEnumerator DealWithEnemyAction(List<CharacterAction> actions, EnemyCharacterData data, bool maxRoll = false)
    {
        foreach (var action in actions)
        {
            if (action.type == ActionType.Damage)
            {
                if(!maxRoll)
                {
                    yield return new WaitForSeconds(_fieldController.EnemyAttack(data));
                    DealDamageToPlayer(action.value);
                }
                else
                {
                    //yield return new WaitForSeconds(_fieldController.CharacterBigAttack(data));
                    yield return new WaitForSeconds(_fieldController.EnemyAttack(data));
                    DealDamageToPlayer(action.value);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
    

    private IEnumerator DealWithPlayerAction(List<CharacterAction> actions, PlayerCharacterData data, bool maxRoll = false)
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
        //_fieldController.CharacterTakeDamage(_currentEnemyCharacters[enemyIndex], num);
    }
    
    private void DealDamageToPlayer(float num)
    {
        int playerIndex = Random.Range(0, _currentPlayerCharacters.Count);
        _currentPlayerCharacters[playerIndex].currentHealth -= num;
        //_fieldController.CharacterTakeDamage(_currentEnemyCharacters[enemyIndex], num);
    }
    
    private void RollDice()
    {
        if (!_canRollDice)
        {
            return;
        }
        
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
        _canAttack = true;
        _canRollDice = false;
    }
    
    private void EnemyRollDice()
    {
        
        int[] dice = new int[_amountOfDiceRolled];
        _diceRolls = new[] { 0, 0, 0, 0, 0, 0};
        string debugString = "Enemy Dice:\n";

        //only rolling 5 dice
        for(int i =0; i < _amountOfDiceRolled; i++)
        {
            dice[i] = Random.Range(1, 6);
            _diceRolls[dice[i] - 1]++;
            debugString += $"{i + 1}: {dice[i]}\n";
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
