using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class BattleSystem : BaseSystem
{
    [SerializeField] private int _amountOfDiceRolledPerTurn = 5;
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
    private bool _firstRoll = true;
    private bool _canAttack = false;

    private int[] _activeDiceRolls = new int[] {0,0,0,0,0,0};
    private int _amountOfRerolls = 0;

    private int _diceInCharacterTrays = 0;
    private float _currentOrderTokens = 0;

    private int _turnCount = 0;
    private bool _playerHasHealed = false;

    private EnemyAttackSet _currentEnemyAttackSet;
    private int _currentEnemyAttackIndex;


    public override void Initialize(GameManager gameManager)
    {
        _cameraSystem = gameManager.CameraSystem;
        _battleMenu.Initialize(gameManager.MenuSystem, RollDice, PlayerAttack,IncreaseActiveDiceRolls, DecreaseActiveDiceRolls);
        base.Initialize(gameManager);
    }
    private void Start()
    {
        StartNewBattle();
    }

    private void StartNewBattle()
    {
        LoadCharacters(_fakePlayerData, _fakeEnemyData);
        _battleMenu.OpenTrays();
        _diceInCharacterTrays = 0;
        _currentOrderTokens = 0;
        _amountOfRerolls = 3;
        _turnCount = 0;
        _playerHasHealed = false;
        
        _battleMenu.UpdateOrderTokenText(_currentOrderTokens);
        _battleMenu.SetRerollNumber(_amountOfRerolls);
        _battleMenu.ResetAllStatusEffects();
    }

    public void LoadCharacters(List<PlayerCharacter> playerCharacters, List<EnemyCharacter> enemyCharacters)
    {
        List<PlayerCharacterData> playerData = new List<PlayerCharacterData>(); 
        List<EnemyCharacterData> enemyData = new List<EnemyCharacterData>(); 
        
        _fieldController.WipeCharacterDictionary();
        _fieldController.WipeCharacterDictionary();

        int i = 0;
        foreach (var character in playerCharacters)
        {
            var data = character.GetFullHealthCharacterData();
            data.currentIndex = i;
            playerData.Add(data);

            i++;
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
        
        _firstRoll = true;
        _canAttack = false;
    }

    public void UpdateUI()
    {
        
        _battleMenu.UpdatePlayerCharacters(_currentPlayerCharacters);
        _battleMenu.UpdateEnemyUI(_currentEnemyCharacters[0]);
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
        
        yield return EnemyAttackActions();
        
        EndEnemyTurn();
        StartPlayerTurn();
    }

    private void EndEnemyTurn()
    {
        DealWithEnemyBleedDamage();
        _turnCount++;
    }

    private void StartPlayerTurn()
    {
        UpdateUI();
        _canAttack = false;
        _firstRoll = true;
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
                _battleMenu.UpdatePlayerCharacterGuardUI(i,0);
            }
        }
    }

    private void DealWithEnemyBleedDamage()
    {
        foreach (var character in _currentEnemyCharacters)
        {
            if (character.statusEffects.ContainsKey(StatusEffects.Bleed) && character.statusEffects[StatusEffects.Bleed] > 0)
            {
                EnemyTakeDamage(character.statusEffects[StatusEffects.Bleed]);
                character.statusEffects[StatusEffects.Bleed]--;
                
                _battleMenu.UpdateEnemyBleedUI(character.statusEffects[StatusEffects.Bleed]);
            }
        }
        
    }
    private void DealWithPlayerBleedDamage()
    {
        foreach (var character in _currentPlayerCharacters)
        {
            if (character.statusEffects.ContainsKey(StatusEffects.Bleed) && character.statusEffects[StatusEffects.Bleed] > 0)
            {
                PlayerTakeDamage(character.currentIndex,character.statusEffects[StatusEffects.Bleed]);
                character.statusEffects[StatusEffects.Bleed]--;
                
                _battleMenu.UpdatePlayerCharacterBleedUI(character.currentIndex, character.statusEffects[StatusEffects.Bleed]);
            }
        }
    }

    private void EndPlayerTurn()
    {
        FocusSentinelCheckForHeals();
        FocusPilgrimCheckForOrderTokens();
        DealWithPlayerBleedDamage();
        
        UpdateUI();
        
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

                ResetIndexedPlayerFocus(i);
                
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

    private void ResetIndexedPlayerFocus(int i)
    {
        if (_currentPlayerCharacters[i].currentFocus > 0)
        {
            _currentPlayerCharacters[i].currentFocus = 0;
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

    private List<EnemyAttackSet> GetPotentialEnemyAttacks()
    {
        var currentEnemy = _currentEnemyCharacters[0];
        List<EnemyAttackSet> _potentialAttackSets = new List<EnemyAttackSet>();

        foreach (var attackSet in currentEnemy.enemyActions)
        {
            bool conditionPass = true;
            
            foreach (var condition in attackSet.conditionList)
            {
                if (condition.condition == EnemyAttackCondition.HealthLowerThan && currentEnemy.currentHealth < condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.HealthGreaterThan && currentEnemy.currentHealth > condition.value)
                {
                    conditionPass = false;
                    break;
                }

                if (condition.condition == EnemyAttackCondition.TurnCountEqualTo && _turnCount != condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.TurnCountGreaterThan && _turnCount > condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.TurnCountLessThan && _turnCount < condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.PlayerHasHealed && !_playerHasHealed)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.PlayerHasNotHealed && _playerHasHealed)
                {
                    conditionPass = false;
                    break;
                }
            }

            if (conditionPass)
            {
                _potentialAttackSets.Add(attackSet);
            }
        }

        return _potentialAttackSets;
    }
    
    private IEnumerator EnemyAttackActions()
    {

        yield return null;

        if (_currentEnemyAttackSet == null || _currentEnemyAttackIndex >= _currentEnemyAttackSet.sequencedAttacks.Count)
        {
            List<EnemyAttackSet> listOfAttacks = GetPotentialEnemyAttacks();

            _currentEnemyAttackIndex = 0;
            _currentEnemyAttackSet = listOfAttacks[Random.Range(0, listOfAttacks.Count)];
        }

        yield return DealWithEnemyAction(_currentEnemyAttackSet.sequencedAttacks[_currentEnemyAttackIndex].actionSet,
            _currentEnemyCharacters[0]);
        
        _currentEnemyAttackIndex++;
    }

    private IEnumerator DealWithEnemyAction(List<EnemyCharacterAction> actions, EnemyCharacterData data, bool maxRoll = false)
    {
        foreach (var action in actions)
        {                    
            yield return new WaitForSeconds(_fieldController.EnemyCharacterMoveForward(data));

            if (action.type == EnemyActionType.DamageRandom)
            {
                if(!maxRoll)
                {
                    EnemyAttackPlayer(action.value);
                }
                else
                {
                    //yield return new WaitForSeconds(_fieldController.CharacterBigAttack(data));
                    EnemyAttackPlayer(action.value);
                }
            }

            if (action.type == EnemyActionType.Heal)
            {
                HealEnemyUnit(action.value);
            }
            
            if (action.type == EnemyActionType.BleedRandom)
            {
                int playerIndex = Random.Range(0, _currentPlayerCharacters.Count);
                ApplyPlayerBleedRandom(playerIndex, action.value);
            }

            if (action.type == EnemyActionType.BleedAll)
            {
                for (int i = 0; i < _currentPlayerCharacters.Count; i++)
                {
                    ApplyPlayerBleedRandom(i,action.value);
                }
            }

            if (action.type == EnemyActionType.Focus)
            {
                if (data.currentFocus < 3)
                {
                    data.currentFocus+=action.value;
                    _battleMenu.UpdateEnemyUI(data);
                }
            }

            yield return new WaitForSeconds(_waitTimeBetweenAttacks);
        }
    }
    

    private IEnumerator DealWithPlayerAction(List<PlayerCharacterAction> actions, PlayerCharacterData data, int castingUnitIndex, bool maxRoll = false)
    {
        foreach (var action in actions)
        {
            yield return new WaitForSeconds(_fieldController.PlayerCharacterMoveForward(data));
            
            if (action.type == PlayerActionType.Damage)
            {
                
                if(!maxRoll)
                {
                    PlayerAttackEnemy(action.value);
                }
                else
                {
                    PlayerAttackEnemy(action.value);
                }
                ResetPlayerEmpower(data.currentIndex);
            }

            if (action.type == PlayerActionType.Bleed)
            {
                ApplyEnemyBleed(action.value);
            }


            if (action.type == PlayerActionType.Heal)
            {
                HealPlayerUnit(castingUnitIndex, action.value);
                
                if (castingUnitIndex > 0)
                {
                    HealPlayerUnit(castingUnitIndex -1, action.value);
                }

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentPlayerCharacters.Count)
                {
                    HealPlayerUnit(castingUnitIndex + 1, action.value);
                }

                _playerHasHealed = true;
            }
            
            if (action.type == PlayerActionType.HealSelf)
            {
                HealPlayerUnit(castingUnitIndex, action.value);
            }
            
            if (action.type == PlayerActionType.HealNearby)
            {
                if (castingUnitIndex > 0)
                {
                    HealPlayerUnit(castingUnitIndex -1, action.value);
                }

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentPlayerCharacters.Count)
                {
                    HealPlayerUnit(castingUnitIndex + 1, action.value);
                }
            }

            if (action.type == PlayerActionType.Guard)
            {
                GuardPlayerUnit(castingUnitIndex, action.value);
                
                if (castingUnitIndex > 0)
                {
                    GuardPlayerUnit(castingUnitIndex - 1, action.value);
                }

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentPlayerCharacters.Count)
                {
                    GuardPlayerUnit(castingUnitIndex + 1, action.value);
                }
            }

            if (action.type == PlayerActionType.Order)
            {
                UpdateOrder(action.value);
            }

            if (action.type == PlayerActionType.Empower)
            {
                EmpowerPlayerUnit(castingUnitIndex, action.value);
                
                if (castingUnitIndex > 0)
                {
                    EmpowerPlayerUnit(castingUnitIndex -1, action.value);
                }

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentPlayerCharacters.Count)
                {
                    EmpowerPlayerUnit(castingUnitIndex + 1, action.value);
                }
            }

            if (action.type == PlayerActionType.Inspire)
            {
                _currentPlayerCharacters[castingUnitIndex].currentFocus++;
                _battleMenu.UpdatePlayerCharacterFocus(_currentPlayerCharacters[castingUnitIndex]);
                
                if (castingUnitIndex > 0)
                {
                    _currentPlayerCharacters[castingUnitIndex - 1].currentFocus++;
                    _battleMenu.UpdatePlayerCharacterFocus(_currentPlayerCharacters[castingUnitIndex-1]);
                }

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentPlayerCharacters.Count)
                {
                    _currentPlayerCharacters[castingUnitIndex + 1].currentFocus++;
                    _battleMenu.UpdatePlayerCharacterFocus(_currentPlayerCharacters[castingUnitIndex+1]);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void EmpowerPlayerUnit(int unitIndex, float amount)
    {
        _currentPlayerCharacters[unitIndex].currentDamageMultiplier += (amount / 100);
    }

    private void ResetPlayerEmpower(int unitIndex)
    {
        _currentPlayerCharacters[unitIndex].currentDamageMultiplier = 1;
    }

    private void UpdateOrder(float newValue)
    {
        _currentOrderTokens += newValue;
        _battleMenu.UpdateOrderTokenText(_currentOrderTokens);
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
        
        _battleMenu.UpdatePlayerCharacterGuardUI(unitIndex,_currentPlayerCharacters[unitIndex].statusEffects[StatusEffects.Guard]);
    }

    private void ApplyPlayerBleedRandom(int index, float amount)
    {
        
        if(_currentPlayerCharacters[index].statusEffects.ContainsKey(StatusEffects.Bleed))
        {
            _currentPlayerCharacters[index].statusEffects[StatusEffects.Bleed] += amount;
        }
        else
        {
            _currentPlayerCharacters[index].statusEffects.Add(StatusEffects.Bleed,amount);
        }
        
        _battleMenu.UpdatePlayerCharacterBleedUI(index,_currentPlayerCharacters[index].statusEffects[StatusEffects.Bleed]);
    }
    
    private void ApplyEnemyBleed(float amount)
    {
        if (_currentEnemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Bleed))
        {
            _currentEnemyCharacters[0].statusEffects[StatusEffects.Bleed] += amount;
        }
        else
        {
            _currentEnemyCharacters[0].statusEffects.Add(StatusEffects.Bleed,amount);
        }
        
        _battleMenu.UpdateEnemyBleedUI(_currentEnemyCharacters[0].statusEffects[StatusEffects.Bleed]);
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

    private void PlayerAttackEnemy(float num)
    {
        int enemyIndex = Random.Range(0, _currentEnemyCharacters.Count);
        num = CheckStatusEffectsForGuard(ref _currentEnemyCharacters[enemyIndex].statusEffects, num);
        
        EnemyTakeDamage(num);
    }
    
    private void EnemyAttackPlayer(float num)
    {
        int playerIndex;
        int sentinelValue = FocusSentinelCheckForFullPoints();

        if (sentinelValue != -1)
        {
            playerIndex = sentinelValue;
        }
        else
        {
            playerIndex = Random.Range(0, _currentPlayerCharacters.Count);
        }

        if (_currentPlayerCharacters[playerIndex].classType == ClassType.Warrior &&
            _currentPlayerCharacters[playerIndex].currentFocus > 0)
        {
            FocusWarriorCheckCounterDamage(_currentPlayerCharacters[playerIndex]);
        }

        float newDamageNum = CheckStatusEffectsForGuard(ref _currentPlayerCharacters[playerIndex].statusEffects, num);

        if (num != newDamageNum)
        {
            _battleMenu.UpdatePlayerCharacterGuardUI(playerIndex,_currentPlayerCharacters[playerIndex].statusEffects[StatusEffects.Guard]);
        }
        
        PlayerTakeDamage(playerIndex, num);
    }

    private void EnemyTakeDamage(float num)
    {
        _currentEnemyCharacters[0].currentHealth -= num;
        _fieldController.EnemyTakeDamage(_currentEnemyCharacters[0], num);
        _battleMenu.UpdateEnemyUI(_currentEnemyCharacters[0]);
    }
    
    private void PlayerTakeDamage(int index, float num)
    {
        _currentPlayerCharacters[index].currentHealth -= num;
        _fieldController.PlayerTakeDamage(_currentPlayerCharacters[index], num);
        _battleMenu.UpdatePlayerCharacterHealth(_currentPlayerCharacters[index]);
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

    private bool CheckDiceButtonClickable()
    {
        return !(!_firstRoll && _amountOfRerolls <= 0 && _amountOfDiceRolledPerTurn - _diceInCharacterTrays <= 0);
    }
    
    private void RollDice()
    {
        if (!CheckDiceButtonClickable())
        {
            return;
        }

        int[] dice = new int[_amountOfDiceRolledPerTurn-_diceInCharacterTrays];
        //_diceRolls = new[] { 0, 0, 0, 0, 0, 0};
        //string debugString = "";

        //only rolling 5 dice
        for(int i =0; i < _amountOfDiceRolledPerTurn-_diceInCharacterTrays; i++)
        {
            dice[i] = Random.Range(1, 6);
        }

        for(int i = 0; i < dice.Length; i++)
        {
            _battleMenu.SetDiceInTrayUI(i, dice[i]);
        }
        
        _canAttack = true;
        
        if(_firstRoll)
        {
            _firstRoll = false;
        }
        else
        {
            _amountOfRerolls--;
            _battleMenu.SetRerollNumber(_amountOfRerolls);
        }
    }
    
    private void EnemyRollDice()
    {
        
        int[] dice = new int[_amountOfDiceRolledPerTurn];
        _activeDiceRolls = new[] { 0, 0, 0, 0, 0, 0};
        string debugString = "Enemy Dice:\n";

        //only rolling 5 dice
        for(int i =0; i < _amountOfDiceRolledPerTurn; i++)
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
