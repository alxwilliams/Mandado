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
    [SerializeField] private int _maxFocusPoints = 3;

    [Header("Wait Times")] 
    [SerializeField] private float _waitTimeBetweenAttacks = .25f;
    [SerializeField] private float _timeBeforeEnemyAttacks = .5f;

    private List<PlayerCharacterData> _currentPlayerCharacters;
    private List<EnemyCharacterData> _currentEnemyCharacters;

    private CameraSystem _cameraSystem;

    private Coroutine _attackRoutine;
    private bool _canRollDice = true;
    private bool _canAttack = false;

    private int[] _activeDiceRolls = new int[] {0,0,0,0,0,0};

    private int _diceInCharacterTrays = 0;


    public override void Initialize(GameManager gameManager)
    {
        _cameraSystem = gameManager.CameraSystem;
        _battleMenu.Initialize(gameManager.MenuSystem, RollDice, PlayerAttack,IncreaseActiveDiceRolls, DecreaseActiveDiceRolls);
        base.Initialize(gameManager);
    }
    private void Start()
    {
        LoadCharacters(_fakePlayerData, _fakeEnemyData);
        _battleMenu.OpenTrays();
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
        _battleMenu.SetPlayerAmount(playerCharacters.Count);
        _currentPlayerCharacters = playerData;
        
        foreach (var character in enemyCharacters)
        {
            enemyData.Add(character.GetFullHealthCharacterData());
        }
        
        _fieldController.LoadEnemyCharacters(enemyData);
        _currentEnemyCharacters = enemyData;
        UpdateUI();
        
        _canRollDice = true;
        _canAttack = false;
    }

    public void UpdateUI()
    {
        string testString = "";
        
        _battleMenu.UpdatePlayerCharacters(_currentPlayerCharacters);

        foreach (var character in _currentEnemyCharacters)
        {
            testString += $"{character.name}: {character.currentHealth}\n";
        }
        
        _battleMenu.UpdateEnemyDebugText(testString);
    }

    private void PlayerAttack()
    {
        if (!_canAttack)
        {
            return;
        }
        
        EndPlayerTurn();
        
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
        }

        _attackRoutine = StartCoroutine(AttackRoutine());
    }
    
    private IEnumerator AttackRoutine()
    {
        yield return PlayerDiceActions();
        _activeDiceRolls = new int[5];

        //enemy attack
        EnemyRollDice();
        yield return new WaitForSeconds(_timeBeforeEnemyAttacks);
        yield return EnemyDiceActions();
        
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        UpdateUI();
        _canAttack = false;
        _canRollDice = true;
        ResetPlayerGuard();
        ResetPlayerDiceTrays();
        
        _battleMenu.OpenTrays();
        _cameraSystem.SwitchToPlayerView();
    }

    private void ResetPlayerGuard()
    {
        for (int i = 0; i < _currentPlayerCharacters.Count; i++)
        {
            if (_currentPlayerCharacters[i].statusEffects.ContainsKey(StatusEffects.Guard) && _currentPlayerCharacters[i].statusEffects[StatusEffects.Guard] != 0 )
            {
                _currentPlayerCharacters[i].statusEffects[StatusEffects.Guard] = 0;
                _battleMenu.UpdateCharacterGuardUI(i,0);
            }
        }
    }

    private void EndPlayerTurn()
    {
        _battleMenu.CloseTrays();
        _cameraSystem.SwitchToEnemyView();
    }

    private IEnumerator PlayerDiceActions()
    {
        for (int i = 0; i < _currentPlayerCharacters.Count; i++)
        {
            if (_activeDiceRolls[i] > 0 && _currentPlayerCharacters[i].currentHealth > 0)
            {
                if (_activeDiceRolls[i] == 1)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollOneActions,
                        _currentPlayerCharacters[i],i);
                }
                else if (_activeDiceRolls[i] == 2)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollTwoActions,
                        _currentPlayerCharacters[i],i);
                }
                else if (_activeDiceRolls[i] == 3)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollThreeActions,
                        _currentPlayerCharacters[i],i);
                }
                else if (_activeDiceRolls[i] == 4)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollFourActions,
                        _currentPlayerCharacters[i],i);
                }
                else if (_activeDiceRolls[i] == 5)
                {
                    yield return DealWithPlayerAction(_currentPlayerCharacters[i].actionSet.rollFiveActions,
                        _currentPlayerCharacters[i],i, true);
                }

                UpdateUI();
                yield return new WaitForSeconds(_waitTimeBetweenAttacks);
            }
            else if(_currentPlayerCharacters[i].currentFocus < 3 && _currentPlayerCharacters[i].currentHealth > 0)
            {
                _currentPlayerCharacters[i].currentFocus++;
                UpdateUI();
            }
        }
    }

    private void IncreaseActiveDiceRolls(int num)
    {
        _activeDiceRolls[num]++;
        _diceInCharacterTrays++;
    }

    private void DecreaseActiveDiceRolls(int num)
    {
        _activeDiceRolls[num]--;
        _diceInCharacterTrays--;
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
                workingDiceTotal += _activeDiceRolls[currentDiceNumber];

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
            yield return new WaitForSeconds(_fieldController.EnemyCharacterMoveForward(data));

            if (action.type == ActionType.Damage)
            {
                if(!maxRoll)
                {
                    DealDamageToPlayer(action.value);
                }
                else
                {
                    //yield return new WaitForSeconds(_fieldController.CharacterBigAttack(data));
                    DealDamageToPlayer(action.value);
                }
            }

            if (action.type == ActionType.HealSelf)
            {
                HealEnemyUnit(action.value);
            }

            yield return new WaitForSeconds(_waitTimeBetweenAttacks);
        }
    }
    

    private IEnumerator DealWithPlayerAction(List<CharacterAction> actions, PlayerCharacterData data, int castingUnitIndex, bool maxRoll = false)
    {
        foreach (var action in actions)
        {
            yield return new WaitForSeconds(_fieldController.PlayerCharacterMoveForward(data));
            
            if (action.type == ActionType.Damage)
            {
                
                if(!maxRoll)
                {
                    DealDamageToEnemy(action.value);
                }
                else
                {
                    DealDamageToEnemy(action.value);
                }
            }
            
            if (action.type == ActionType.HealSelf)
            {
                HealPlayerUnit(castingUnitIndex, action.value);
            }
            
            if (action.type == ActionType.HealNearby)
            {
                if (castingUnitIndex > 0)
                {
                    HealPlayerUnit(castingUnitIndex -1, action.value);
                }

                if (castingUnitIndex < 5)
                {
                    HealPlayerUnit(castingUnitIndex + 1, action.value);
                }
            }

            if (action.type == ActionType.GuardSelf)
            {
                GuardPlayerUnit(castingUnitIndex, action.value);
            }
            
            

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void GuardPlayerUnit(int unitIndex, float amount)
    {
        if (_currentPlayerCharacters[unitIndex].statusEffects.ContainsKey(StatusEffects.Guard))
        {
            _currentPlayerCharacters[unitIndex].statusEffects[StatusEffects.Guard] += amount;
        }
        else
        {
            _currentPlayerCharacters[unitIndex].statusEffects[StatusEffects.Guard] = amount;
        }
        
        _battleMenu.UpdateCharacterGuardUI(unitIndex,_currentPlayerCharacters[unitIndex].statusEffects[StatusEffects.Guard]);
    }

    private void HealPlayerUnit(int unitIndex, float amount)
    {
        _currentPlayerCharacters[unitIndex].currentHealth += amount;
        _fieldController.PlayerCharacterGetHealed(_currentPlayerCharacters[unitIndex], amount);
    }

    private void HealEnemyUnit(float amount)
    {
        _currentEnemyCharacters[0].currentHealth += amount;
        _fieldController.EnemyCharacterGetHealed(_currentEnemyCharacters[0],amount);
    }

    private void DealDamageToEnemy(float num)
    {
        int enemyIndex = Random.Range(0, _currentEnemyCharacters.Count);

        num = CheckStatusEffectsForGuard(ref _currentEnemyCharacters[enemyIndex].statusEffects, num);
        
        _currentEnemyCharacters[enemyIndex].currentHealth -= num;
        _fieldController.EnemyTakeDamage(_currentEnemyCharacters[enemyIndex], num);
    }
    
    private void DealDamageToPlayer(float num)
    {
        int playerIndex = Random.Range(0, _currentPlayerCharacters.Count);
        
        
        float newDamageNum = CheckStatusEffectsForGuard(ref _currentPlayerCharacters[playerIndex].statusEffects, num);

        if (num != newDamageNum)
        {
            _battleMenu.UpdateCharacterGuardUI(playerIndex,_currentPlayerCharacters[playerIndex].statusEffects[StatusEffects.Guard]);
        }
        
        _currentPlayerCharacters[playerIndex].currentHealth -= num;
        _fieldController.PlayerTakeDamage(_currentPlayerCharacters[playerIndex], num);
    }

    private float CheckStatusEffectsForGuard(ref Dictionary<StatusEffects,float> effects, float damage)
    {
        if (effects.ContainsKey(StatusEffects.Guard))
        {
            effects[StatusEffects.Guard] -= damage;

            if (effects[StatusEffects.Guard] >= 0)
            {
                return 0;
            }
            else
            {
                float newDamage = effects[StatusEffects.Guard];
                effects[StatusEffects.Guard] = 0;
                
                return newDamage;
            }
        }
        else
        {
            return damage;
        }
    }
    
    private void RollDice()
    {
        if (!_canRollDice)
        {
            return;
        }

        int[] dice = new int[_amountOfDiceRolled-_diceInCharacterTrays];
        //_diceRolls = new[] { 0, 0, 0, 0, 0, 0};
        //string debugString = "";

        //only rolling 5 dice
        for(int i =0; i < _amountOfDiceRolled-_diceInCharacterTrays; i++)
        {
            dice[i] = Random.Range(1, 6);
        }

        for(int i = 0; i < dice.Length; i++)
        {
            _battleMenu.SetDiceInTrayUI(i, dice[i]);
        }
        
        _canAttack = true;
        _canRollDice = false;
    }
    
    private void EnemyRollDice()
    {
        
        int[] dice = new int[_amountOfDiceRolled];
        _activeDiceRolls = new[] { 0, 0, 0, 0, 0, 0};
        string debugString = "Enemy Dice:\n";

        //only rolling 5 dice
        for(int i =0; i < _amountOfDiceRolled; i++)
        {
            dice[i] = Random.Range(1, 6);
            _activeDiceRolls[dice[i] - 1]++;
            debugString += $"{i + 1}: {dice[i]}\n";
        }
        
        //_battleMenu.UpdateDiceText(debugString);
    }

    private void ResetPlayerDiceTrays()
    {
        _diceInCharacterTrays = 0;
        _activeDiceRolls = new[] { 0, 0, 0, 0, 0, 0};
        _battleMenu.ResetDiceTrays();
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
