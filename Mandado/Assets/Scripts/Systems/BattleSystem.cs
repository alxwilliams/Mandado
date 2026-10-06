using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    private CameraSystem _cameraSystem;

    private Coroutine _attackRoutine;

    private EnemyAttackSet _currentEnemyAttackSet;
    private int _currentEnemyAttackIndex;

    private BattleSystemState _currentSaveState;

    public override void Initialize(GameManager gameManager)
    {
        _cameraSystem = gameManager.CameraSystem;
        _battleMenu.Initialize(gameManager.MenuSystem, RollDice, PlayerAttack,IncreaseActiveDiceRolls, DecreaseActiveDiceRolls);
        base.Initialize(gameManager);
    }
    
    public void LoadSavedBattle(BattleSystemState state)
    {
        _currentSaveState = state;
        List<PlayerCharacterData> playerData = new List<PlayerCharacterData>(); 
        List<EnemyCharacterData> enemyData = new List<EnemyCharacterData>();

        _fieldController.DisableAllCurrentCharacters();
        _fieldController.WipeCharacterDictionary();

        int i = 0;
        foreach (var data in state.playerCharacters)
        {
            data.statusEffects = data.statusEffectsSerialized.ToDictionary();
            data.currentIndex = i;
            playerData.Add(data);

            i++;
        }
        
        _fieldController.LoadPlayerCharacters(playerData);
        _battleMenu.LoadInCharacterUI(playerData);
        
        _currentSaveState.playerCharacters = playerData;
        
        foreach (var data in state.enemyCharacters)
        {
            data.statusEffects = data.statusEffectsSerialized.ToDictionary();
            enemyData.Add(data);
        }
        
        _fieldController.LoadEnemyCharacters(enemyData);
        _currentSaveState.enemyCharacters = enemyData;
        LoadInBattleState(state);
        
        UpdateUI();
    }

    public bool IsIndexCharacterEmpty(int index)
    {
        return _currentSaveState.playerCharacters[index].classType == ClassType.Empty;
    }

    public void LoadInBattleState(BattleSystemState state)
    {
        //_battleMenu.OpenTrays();
        _battleMenu.UpdateOrderTokenText(_currentSaveState.currentOrderTokens);
        _battleMenu.SetRerollNumber(_currentSaveState.amountOfRerolls);
        _battleMenu.LoadInStatusEffects(state);
    }

    public void StartNewBattle()
    {
        BattleSystemState newState = new BattleSystemState();
        LoadNewBattleCharacters(newState, _fakePlayerData, _fakeEnemyData);
        LoadInBattleState(newState);
    }

    public void LoadNewBattleCharacters(BattleSystemState state, List<PlayerCharacter> playerCharacters, List<EnemyCharacter> enemyCharacters)
    {
        _currentSaveState = state;
        
        List<PlayerCharacterData> playerData = new List<PlayerCharacterData>(); 
        List<EnemyCharacterData> enemyData = new List<EnemyCharacterData>(); 
        
        _fieldController.DisableAllCurrentCharacters();
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
        _battleMenu.LoadInCharacterUI(playerData);
        _currentSaveState.playerCharacters = playerData;
        
        foreach (var character in enemyCharacters)
        {
            enemyData.Add(character.GetFullHealthCharacterData());
        }
        
        _fieldController.LoadEnemyCharacters(enemyData);
        _currentSaveState.enemyCharacters = enemyData;
        UpdateUI();
        
    }

    public BattleSystemState GetBattleSystemState()
    {
        _currentSaveState.playerCharacters = _currentSaveState.playerCharacters;
        _currentSaveState.enemyCharacters = _currentSaveState.enemyCharacters;
        _currentSaveState.turnNumber = _currentSaveState.turnNumber;

        foreach (var data in _currentSaveState.playerCharacters)
        {
            data.statusEffectsSerialized = new SerializableStatusDictionary(data.statusEffects);
            
        }
        foreach (var data in _currentSaveState.enemyCharacters)
        {
            data.statusEffectsSerialized = new SerializableStatusDictionary(data.statusEffects);
        }
        
        return _currentSaveState;
    }

    public void UpdateUI()
    {
        _battleMenu.SetRerollNumber(_currentSaveState.amountOfRerolls);
        _battleMenu.UpdatePlayerCharacters(_currentSaveState.playerCharacters);
        _battleMenu.UpdateEnemyUI(_currentSaveState.enemyCharacters[0]);
    }

    private void PlayerAttack()
    {
        if (!_currentSaveState.canAttack)
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
        _currentSaveState.activeDiceRolls = new int[6];

        //enemy attack
        EnemyRollDice();
        
        _cameraSystem.SwitchToEnemyView();
        yield return new WaitForSeconds(_timeBeforeEnemyAttacks);
        
        yield return EnemyAttackActions();
        
        EndEnemyTurn();
        StartPlayerTurn();
    }

    private void EndEnemyTurn()
    {
        DealWithEnemyBleedDamage();
        _currentSaveState.turnNumber++;
    }

    private void StartPlayerTurn()
    {
        _currentSaveState.amountOfRerolls = 3;
        _currentSaveState.canAttack = false;
        ResetPlayerGuard();
        ResetPlayerDiceTrays();
        UpdateUI();
        
        //_battleMenu.OpenTrays();
        _cameraSystem.SwitchToPlayerView();
    }

    private void ResetPlayerGuard()
    {
        for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
        {
            if (_currentSaveState.playerCharacters[i].statusEffects.ContainsKey(StatusEffects.Guard) && _currentSaveState.playerCharacters[i].statusEffects[StatusEffects.Guard] != 0 )
            {
                _currentSaveState.playerCharacters[i].statusEffects[StatusEffects.Guard] = 0;
                _battleMenu.UpdatePlayerCharacterGuardUI(i,0);
            }
        }
    }

    private void DealWithEnemyBleedDamage()
    {
        foreach (var character in _currentSaveState.enemyCharacters)
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
        foreach (var character in _currentSaveState.playerCharacters)
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
        
        //_battleMenu.CloseTrays();
    }

    private IEnumerator PlayerDiceActions()
    {
        for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
        {
            if (_currentSaveState.activeDiceRolls[i] > 0 && _currentSaveState.playerCharacters[i].IsAlive && _currentSaveState.playerCharacters[i].classType != ClassType.Empty)
            {
                if (_currentSaveState.activeDiceRolls[i] == 1)
                {
                    yield return DealWithPlayerAction(_currentSaveState.playerCharacters[i].actionSet.rollOneActions,
                        _currentSaveState.playerCharacters[i],i);
                }
                else if (_currentSaveState.activeDiceRolls[i] == 2)
                {
                    yield return DealWithPlayerAction(_currentSaveState.playerCharacters[i].actionSet.rollTwoActions,
                        _currentSaveState.playerCharacters[i],i);
                }
                else if (_currentSaveState.activeDiceRolls[i] == 3)
                {
                    yield return DealWithPlayerAction(_currentSaveState.playerCharacters[i].actionSet.rollThreeActions,
                        _currentSaveState.playerCharacters[i],i);
                }
                else if (_currentSaveState.activeDiceRolls[i] == 4)
                {
                    yield return DealWithPlayerAction(_currentSaveState.playerCharacters[i].actionSet.rollFourActions,
                        _currentSaveState.playerCharacters[i],i);
                }
                else if (_currentSaveState.activeDiceRolls[i] == 5)
                {
                    yield return DealWithPlayerAction(_currentSaveState.playerCharacters[i].actionSet.rollFiveActions,
                        _currentSaveState.playerCharacters[i],i, true);
                }

                ResetIndexedPlayerFocus(i);
                
                UpdateUI();
                
                yield return new WaitForSeconds(_waitTimeBetweenAttacks);
            }
            else if(_currentSaveState.playerCharacters[i].currentFocus < 3 && _currentSaveState.playerCharacters[i].currentHealth > 0)
            {
                _currentSaveState.playerCharacters[i].currentFocus++;
                UpdateUI();
            }
        }
    }

    private void ResetIndexedPlayerFocus(int i)
    {
        if (_currentSaveState.playerCharacters[i].currentFocus > 0)
        {
            _currentSaveState.playerCharacters[i].currentFocus = 0;
        }
    }

    private void IncreaseActiveDiceRolls(int num)
    {
        _currentSaveState.activeDiceRolls[num]++;
        _currentSaveState.diceInCharacterTrays++;
    }

    private void DecreaseActiveDiceRolls(int num)
    {
        _currentSaveState.activeDiceRolls[num]--;
        _currentSaveState.diceInCharacterTrays--;
    }

    private List<EnemyAttackSet> GetPotentialEnemyAttacks()
    {
        var currentEnemy = _currentSaveState.enemyCharacters[0];
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

                if (condition.condition == EnemyAttackCondition.TurnCountEqualTo && _currentSaveState.turnNumber != condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.TurnCountGreaterThan && _currentSaveState.turnNumber > condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.TurnCountLessThan && _currentSaveState.turnNumber < condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.PlayerHasHealed && !_currentSaveState.playerHasHealed)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.PlayerHasNotHealed && _currentSaveState.playerHasHealed)
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

        if (_currentEnemyAttackSet == null ||
            _currentEnemyAttackSet.sequencedAttacks == null
            || _currentEnemyAttackIndex >= _currentEnemyAttackSet.sequencedAttacks.Count)
        {
            List<EnemyAttackSet> listOfAttacks = GetPotentialEnemyAttacks();

            _currentEnemyAttackIndex = 0;
            _currentEnemyAttackSet = listOfAttacks[_gameManager.GetNewMainRandom(0, listOfAttacks.Count)];
        }

        yield return DealWithEnemyAction(_currentEnemyAttackSet.sequencedAttacks[_currentEnemyAttackIndex].actionSet,
            _currentSaveState.enemyCharacters[0]);
        
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
                int playerIndex = GetNewPlayerIndex();
                ApplyPlayerBleedRandom(playerIndex, action.value);
            }

            if (action.type == EnemyActionType.BleedAll)
            {
                for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
                {
                    ApplyPlayerBleedRandom(i,action.value);
                }
            }

            if (action.type == EnemyActionType.Focus)
            {
                if (data.currentFocus < 3)
                {
                    data.currentFocus+=action.value;

                    if (data.currentFocus > 3)
                    {
                        data.currentFocus = 3;
                    }
                    
                    _battleMenu.UpdateEnemyUI(data);
                }
            }

            yield return new WaitForSeconds(_waitTimeBetweenAttacks);
        }
    }
    

    private IEnumerator DealWithPlayerAction(List<PlayerCharacterAction> actions, PlayerCharacterData data, int castingUnitIndex, bool maxRoll = false)
    {
        yield return new WaitForSeconds(_fieldController.PlayerCharacterMoveForward(data));
        
        foreach (var action in actions)
        {
            
            if (action.type == PlayerActionType.Damage)
            {
                
                if(!maxRoll)
                {
                    PlayerAttackEnemy(action.value * data.currentDamageMultiplier);
                }
                else
                {
                    PlayerAttackEnemy(action.value * data.currentDamageMultiplier);
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

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentSaveState.playerCharacters.Count)
                {
                    HealPlayerUnit(castingUnitIndex + 1, action.value);
                }

                _currentSaveState.playerHasHealed = true;
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

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentSaveState.playerCharacters.Count)
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

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentSaveState.playerCharacters.Count)
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

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentSaveState.playerCharacters.Count)
                {
                    EmpowerPlayerUnit(castingUnitIndex + 1, action.value);
                }
            }

            if (action.type == PlayerActionType.Inspire)
            {
                _currentSaveState.playerCharacters[castingUnitIndex].currentFocus++;
                _battleMenu.UpdatePlayerCharacterFocus(_currentSaveState.playerCharacters[castingUnitIndex]);
                
                if (castingUnitIndex > 0)
                {
                    _currentSaveState.playerCharacters[castingUnitIndex - 1].currentFocus++;
                    _battleMenu.UpdatePlayerCharacterFocus(_currentSaveState.playerCharacters[castingUnitIndex-1]);
                }

                if (castingUnitIndex < 5 && castingUnitIndex + 1 < _currentSaveState.playerCharacters.Count)
                {
                    _currentSaveState.playerCharacters[castingUnitIndex + 1].currentFocus++;
                    _battleMenu.UpdatePlayerCharacterFocus(_currentSaveState.playerCharacters[castingUnitIndex+1]);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void EmpowerPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty)
        {
            return;
        }
        
        _currentSaveState.playerCharacters[unitIndex].currentDamageMultiplier += (amount / 100);
    }

    private void ResetPlayerEmpower(int unitIndex)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty)
        {
            return;
        }
        
        _currentSaveState.playerCharacters[unitIndex].currentDamageMultiplier = 1;
    }

    private void UpdateOrder(float newValue)
    {
        _currentSaveState.currentOrderTokens += newValue;
        _battleMenu.UpdateOrderTokenText(_currentSaveState.currentOrderTokens);
    }

    private void GuardPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty)
        {
            return;
        }
        
        if (_currentSaveState.playerCharacters[unitIndex].statusEffects.ContainsKey(StatusEffects.Guard))
        {
            _currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Guard] += amount;
        }
        else
        {
            _currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Guard] = amount;
        }
        
        _battleMenu.UpdatePlayerCharacterGuardUI(unitIndex,_currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Guard]);
    }

    private void ApplyPlayerBleedRandom(int index, float amount)
    {
        if (_currentSaveState.playerCharacters[index].classType == ClassType.Empty)
        {
            return;
        }
        
        if(_currentSaveState.playerCharacters[index].statusEffects.ContainsKey(StatusEffects.Bleed))
        {
            _currentSaveState.playerCharacters[index].statusEffects[StatusEffects.Bleed] += amount;
        }
        else
        {
            _currentSaveState.playerCharacters[index].statusEffects.Add(StatusEffects.Bleed,amount);
        }
        
        _battleMenu.UpdatePlayerCharacterBleedUI(index,_currentSaveState.playerCharacters[index].statusEffects[StatusEffects.Bleed]);
    }
    
    private void ApplyEnemyBleed(float amount)
    {
        if (_currentSaveState.enemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Bleed))
        {
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Bleed] += amount;
        }
        else
        {
            _currentSaveState.enemyCharacters[0].statusEffects.Add(StatusEffects.Bleed,amount);
        }
        
        _battleMenu.UpdateEnemyBleedUI(_currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Bleed]);
    }

    private void HealPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty)
        {
            return;
        }
        
        if (_currentSaveState.playerCharacters[unitIndex].currentHealth + amount >
            _currentSaveState.playerCharacters[unitIndex].maxHealth)
        {
            amount = _currentSaveState.playerCharacters[unitIndex].maxHealth -
                     _currentSaveState.playerCharacters[unitIndex].currentHealth;
        }
        
        _currentSaveState.playerCharacters[unitIndex].currentHealth += amount;
        _fieldController.PlayerCharacterGetHealed(_currentSaveState.playerCharacters[unitIndex], amount);
    }

    private void HealEnemyUnit(float amount)
    {
        if (_currentSaveState.enemyCharacters[0].currentHealth + amount >
            _currentSaveState.enemyCharacters[0].maxHealth)
        {
            amount = _currentSaveState.enemyCharacters[0].maxHealth -
                     _currentSaveState.enemyCharacters[0].currentHealth;
        }
        
        _currentSaveState.enemyCharacters[0].currentHealth += amount;
        _fieldController.EnemyCharacterGetHealed(_currentSaveState.enemyCharacters[0],amount);
    }

    private void PlayerAttackEnemy(float num)
    {
        int enemyIndex = _gameManager.GetNewTargetRandom(0, _currentSaveState.enemyCharacters.Count);
        num = CheckStatusEffectsForGuard(ref _currentSaveState.enemyCharacters[enemyIndex].statusEffects, num);
        
        EnemyTakeDamage(num);
    }

    private int GetNewPlayerIndex()
    {
        int playerIndex = -1;
        bool foundNotEmpty = false;
        bool allEmpty = true;

        foreach (var characterData in _currentSaveState.playerCharacters)
        {
            if (characterData.classType != ClassType.Empty)
            {
                allEmpty = false;
                break;
            }
        }

        if (allEmpty)
        {
            Debug.LogError("SOmething is wrong. All are empty");
            return -1;
        }

        while (!foundNotEmpty)
        {
            playerIndex = _gameManager.GetNewTargetRandom(0, _currentSaveState.playerCharacters.Count);

            if (_currentSaveState.playerCharacters[playerIndex].classType != ClassType.Empty)
            {
                foundNotEmpty = true;
                break;
            }
        }

        return playerIndex;
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
            playerIndex = GetNewPlayerIndex();
        }

        if (_currentSaveState.playerCharacters[playerIndex].classType == ClassType.Warrior &&
            _currentSaveState.playerCharacters[playerIndex].currentFocus > 0)
        {
            FocusWarriorCheckCounterDamage(_currentSaveState.playerCharacters[playerIndex]);
        }

        float newDamageNum = CheckStatusEffectsForGuard(ref _currentSaveState.playerCharacters[playerIndex].statusEffects, num);

        if (num != newDamageNum)
        {
            _battleMenu.UpdatePlayerCharacterGuardUI(playerIndex,_currentSaveState.playerCharacters[playerIndex].statusEffects[StatusEffects.Guard]);
        }
        
        PlayerTakeDamage(playerIndex, num);
    }

    private void EnemyTakeDamage(float num)
    {
        _currentSaveState.enemyCharacters[0].currentHealth -= num;
        _fieldController.EnemyTakeDamage(_currentSaveState.enemyCharacters[0], num);
        _battleMenu.UpdateEnemyUI(_currentSaveState.enemyCharacters[0]);
    }
    
    private void PlayerTakeDamage(int index, float num)
    {
        _currentSaveState.playerCharacters[index].currentHealth -= num;
        _fieldController.PlayerTakeDamage(_currentSaveState.playerCharacters[index], num);
        _battleMenu.UpdatePlayerCharacterHealth(_currentSaveState.playerCharacters[index]);
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
        return (_currentSaveState.amountOfRerolls > 0 && _amountOfDiceRolledPerTurn - _currentSaveState.diceInCharacterTrays > 0);
    }
    
    private void RollDice()
    {
        if (!CheckDiceButtonClickable())
        {
            return;
        }

        int diceRolled = _amountOfDiceRolledPerTurn - _currentSaveState.diceInCharacterTrays;

        int[] dice = new int[diceRolled];
        //_diceRolls = new[] { 0, 0, 0, 0, 0, 0};
        //string debugString = "";

        //only rolling 5 dice
        for(int i =0; i < diceRolled; i++)
        {
            dice[i] = _gameManager.GetNewMainRandom(1, 6);
        }

        for(int i = 0; i < _amountOfDiceRolledPerTurn; i++)
        {
            if(i < diceRolled)
            {
                _battleMenu.SetDiceInTrayUI(i, dice[i]);
            }
            else
            {
                _battleMenu.DisableDice(i);
            }
        }
        
        _currentSaveState.canAttack = true;
        
        _currentSaveState.amountOfRerolls--;
        _battleMenu.SetRerollNumber(_currentSaveState.amountOfRerolls);
        
    }
    
    private void EnemyRollDice()
    {
        
        int[] dice = new int[_amountOfDiceRolledPerTurn];
        _currentSaveState.activeDiceRolls = new[] { 0, 0, 0, 0, 0, 0};
        string debugString = "Enemy Dice:\n";

        //only rolling 5 dice
        for(int i =0; i < _amountOfDiceRolledPerTurn; i++)
        {
            dice[i] = _gameManager.GetNewMainRandom(1, 6);
            _currentSaveState.activeDiceRolls[dice[i] - 1]++;
            debugString += $"{i + 1}: {dice[i]}\n";
        }
        
        //_battleMenu.UpdateDiceText(debugString);
    }

    private void ResetPlayerDiceTrays()
    {
        _currentSaveState.diceInCharacterTrays = 0;
        _currentSaveState.activeDiceRolls = new[] { 0, 0, 0, 0, 0, 0};
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
