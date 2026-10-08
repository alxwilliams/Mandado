using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public partial class BattleSystem : BaseSystem
{
    [SerializeField] private int _amountOfDiceRolledPerTurn = 5;
    [SerializeField] private List<EnemyCharacter> _fakeEnemyData = new List<EnemyCharacter>();
    [SerializeField] private FieldController _fieldController;
    [SerializeField] private BattleMenu _battleMenu;
    [SerializeField] private int _maxFocusPoints = 3;
    
    [Header("Intent text Details")] 
    [SerializeField] private List<IntentTextDetails> _intentTextDetails;

    [Header("Wait Times")]
    [SerializeField] private float _waitTimeBetweenAttacks = .25f;
    [SerializeField] private float _timeBeforeEnemyAttacks = .5f;
    [SerializeField] private float _timeAfterBleedDamage = .5f;
    [SerializeField] private float _timeAfterEnemyAttacks = .5f;

    
    private List<PlayerCharacter> _newGamePlayerCharacters = new List<PlayerCharacter>();
    private Dictionary<PlayerActionType, IntentTextDetails> _playerIntentDetailsDictionary = new Dictionary<PlayerActionType, IntentTextDetails>();
    private CameraSystem _cameraSystem;
    private Coroutine _attackRoutine;
    private Coroutine _loadCharactersRoutine;
    
    private EnemyAttackSet _currentEnemyAttackSet;
    private int _currentEnemyAttackIndex;

    private BattleSystemState _currentSaveState;

    public bool IsOrderUsable
    {
        get
        {
            return _currentSaveState.currentOrderTokens > 0;
        }
    }

    public override void Initialize(GameManager gameManager)
    {
        _cameraSystem = gameManager.CameraSystem;
        _battleMenu.Initialize(gameManager.MenuSystem, RollDice, PlayerAttack, IncreaseActiveDiceRolls, DecreaseActiveDiceRolls);

        foreach (var details in _intentTextDetails)
        {
            if (!_playerIntentDetailsDictionary.TryAdd(details.type, details))
            {
                Debug.LogError($"Something wrong with: {details.type} when adding to dictionary");
            }
        }
        _fieldController.Init(this,_currentSaveState);
        base.Initialize(gameManager);
    }

    public void SetNewGamePlayerCharacters(List<PlayerCharacter> list)
    {
        _newGamePlayerCharacters = list;
    }

    public IntentTextDetails GetIntentTextDetails(PlayerActionType type)
    {
        return _playerIntentDetailsDictionary[type];
    }

    public void LoadSavedBattle(BattleSystemState state)
    {
        _currentSaveState = state;
        if (_loadCharactersRoutine != null)
        {
            StopCoroutine(_loadCharactersRoutine);
        }

        _loadCharactersRoutine = StartCoroutine(LoadSavedBattle());
    }
    
    private IEnumerator LoadSavedBattle()
    {
        List<PlayerCharacterData> playerData = new List<PlayerCharacterData>(); 
        List<EnemyCharacterData> enemyData = new List<EnemyCharacterData>();

        _fieldController.DisableAllCurrentCharacters();
        _fieldController.WipeCharacterDictionary();

        int i = 0;
        foreach (var data in _currentSaveState.playerCharacters)
        {
            data.statusEffects = data.statusEffectsSerialized.ToDictionary();
            data.currentIndex = i;
            playerData.Add(data);

            i++;
        }
        yield return StartCoroutine(_fieldController.LoadPlayerCharactersCoroutine(playerData));
        _currentSaveState.playerCharacters = playerData;
        ChangePlayerIntentStates();
        
        foreach (var data in _currentSaveState.enemyCharacters)
        {
            data.statusEffects = data.statusEffectsSerialized.ToDictionary();
            enemyData.Add(data);
        }
        
        _battleMenu.LoadInCharacterUI(playerData,enemyData[0]);
        
        yield return StartCoroutine(_fieldController.LoadEnemyCharactersCoroutine(enemyData));
        
        _currentSaveState.enemyCharacters = enemyData;
        LoadInBattleState(_currentSaveState);
        UpdateUI();
        
        _loadCharactersRoutine = null;

    }

    public void UseOrderToken()
    {
        AddOrder(_currentSaveState.currentOrderTokens-1);
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
        _battleMenu.StartBattle();
        StartPlayerTurn(true);
    }

    public void StartNewBattle()
    {
        BattleSystemState newState = new BattleSystemState();

        if (_loadCharactersRoutine != null)
        {
            StopCoroutine(_loadCharactersRoutine);
        }

        _loadCharactersRoutine = StartCoroutine(LoadNewBattleCharacters(newState, _newGamePlayerCharacters, _fakeEnemyData));
    }

    public IEnumerator LoadNewBattleCharacters(BattleSystemState state, List<PlayerCharacter> playerCharacters, List<EnemyCharacter> enemyCharacters)
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
        
        yield return _fieldController.LoadPlayerCharactersCoroutine(playerData);
        _currentSaveState.playerCharacters = playerData;
        ChangePlayerIntentStates();
        
        foreach (var character in enemyCharacters)
        {
            enemyData.Add(character.GetFullHealthCharacterData());
        }
        
        _battleMenu.LoadInCharacterUI(playerData, enemyData[0]);
        
        yield return _fieldController.LoadEnemyCharactersCoroutine(enemyData);
        _currentSaveState.enemyCharacters = enemyData;
        UpdateUI();
        LoadInBattleState(_currentSaveState);
        _loadCharactersRoutine = null;

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
        bool playerBled = DealWithPlayerBleedDamage();

        if (playerBled)
        {
            yield return  new WaitForSeconds(_timeAfterBleedDamage);
        }
        
        yield return PlayerDiceActions();
        _currentSaveState.activeDiceRolls = new int[6];

        //enemy attack
        EnemyRollDice();

        _cameraSystem.SwitchToEnemyView();
        yield return new WaitForSeconds(_timeBeforeEnemyAttacks);
        
        yield return EnemyAttackActions();
        
        yield return new WaitForSeconds(_timeAfterEnemyAttacks);
        
        EndEnemyTurn();
        StartPlayerTurn();
    }

    private void EndEnemyTurn()
    {
        DealWithEnemyBleedDamage();
        _currentSaveState.turnNumber++;
    }

    private void StartPlayerTurn(bool init = false)
    {
        _currentSaveState.amountOfRerolls = 3;
        _currentSaveState.canAttack = false;
        ResetPlayerGuard();
        ResetPlayerDiceTrays();
        GenerateEnemyActions();
        
        UpdateUI();
        //calculate enemy actions
        //show enemy intent
        
        if(!init)
        {
            _cameraSystem.SwitchToPlayerView();
        }
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
    private bool DealWithPlayerBleedDamage()
    {
        bool playerBled = false;
        foreach (var character in _currentSaveState.playerCharacters)
        {
            if (character.statusEffects.ContainsKey(StatusEffects.Bleed) && character.statusEffects[StatusEffects.Bleed] > 0)
            {
                PlayerTakeDamage(character.currentIndex,character.statusEffects[StatusEffects.Bleed]);
                character.statusEffects[StatusEffects.Bleed]--;
                
                _battleMenu.UpdatePlayerCharacterBleedUI(character.currentIndex, character.statusEffects[StatusEffects.Bleed]);
                playerBled = true;
            }
        }

        return playerBled;
    }

    private void EndPlayerTurn()
    {
        FocusSentinelCheckForHeals();
        FocusPilgrimCheckForOrderTokens();
        
        UpdateUI();
        
    }

    private IEnumerator PlayerDiceActions()
    {
        for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
        {
            if (_currentSaveState.activeDiceRolls[i] > 0 && _currentSaveState.playerCharacters[i].IsAlive && _currentSaveState.playerCharacters[i].classType != ClassType.Empty)
            {
                yield return DealWithPlayerAction(_currentSaveState.playerCharacters[i].actionSet.GetActionSetFromRollNumber(_currentSaveState.activeDiceRolls[i]),
                    _currentSaveState.playerCharacters[i],i);

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
        ChangePlayerIntentStates();
    }

    private void DecreaseActiveDiceRolls(int num)
    {
        _currentSaveState.activeDiceRolls[num]--;
        _currentSaveState.diceInCharacterTrays--;
        ChangePlayerIntentStates();
    }

    private void ChangePlayerIntentStates()
    {
        _fieldController.UpdatePlayerStates(_currentSaveState);
    }

    public void SwapCharacters(int character1, int character2)
    {
        (_currentSaveState.activeDiceRolls[character1], _currentSaveState.activeDiceRolls[character2]) = (_currentSaveState.activeDiceRolls[character2], _currentSaveState.activeDiceRolls[character1]);
        (_currentSaveState.playerCharacters[character1], _currentSaveState.playerCharacters[character2]) = (_currentSaveState.playerCharacters[character2], _currentSaveState.playerCharacters[character1]);
        _fieldController.SwapCharacterPlaces(_currentSaveState.playerCharacters[character1],_currentSaveState.playerCharacters[character2]);
        
        _battleMenu.UpdatePlayerCharacterStatusEffects(_currentSaveState, character1);
        _battleMenu.UpdatePlayerCharacterStatusEffects(_currentSaveState, character2);
        
        UseOrderToken();
        ChangePlayerIntentStates();
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

    private void GenerateEnemyActions()
    {
        if (_currentEnemyAttackSet == null ||
            _currentEnemyAttackSet.sequencedAttacks == null
            || _currentEnemyAttackIndex >= _currentEnemyAttackSet.sequencedAttacks.Count)
        {
            List<EnemyAttackSet> listOfAttacks = GetPotentialEnemyAttacks();

            _currentEnemyAttackIndex = 0;
            _currentEnemyAttackSet = listOfAttacks[_gameManager.GetNewMainRandom(0,listOfAttacks.Count-1)];
        }

        foreach (var action in _currentEnemyAttackSet.sequencedAttacks[_currentEnemyAttackIndex].actionSet)
        {
            if (action.type == EnemyActionType.DamageRandom)
            {
                action.targetIndex = GetNewPlayerIndex();
            }

            if (action.type == EnemyActionType.BleedRandom)
            {
                action.targetIndex = GetNewPlayerIndex();
            }
        }
        
        _battleMenu.ShowEnemyIntent(_currentEnemyAttackSet.sequencedAttacks[_currentEnemyAttackIndex].actionSet);
    }
    
    private IEnumerator EnemyAttackActions()
    {
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
                    EnemyAttackPlayer(action.targetIndex,action.value);
                }
                else
                {
                    //yield return new WaitForSeconds(_fieldController.CharacterBigAttack(data));
                    EnemyAttackPlayer(action.targetIndex,action.value);
                }
            }

            if (action.type == EnemyActionType.Heal)
            {
                HealEnemyUnit(action.value);
            }
            
            if (action.type == EnemyActionType.BleedRandom)
            {
                ApplyPlayerBleedRandom(action.targetIndex, action.value);
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

        _battleMenu.ResetTargetIndicators();

        if (data.currentFocus >= 3)
        {
            if (data.enemyFocusAbility == EnemyFocusAbility.Tragos)
            {
                yield return new WaitForSeconds(_fieldController.EnemyCharacterMoveForward(data));
                TragosFocusAttack();
            }

            data.currentFocus = 0;
            _battleMenu.UpdateEnemyUI(data);
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
                AddOrder(action.value);
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

        _fieldController.WipePlayerCharacterIntent(data);
        _battleMenu.ResetDice(castingUnitIndex);
    }

    private void EmpowerPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty || !_currentSaveState.playerCharacters[unitIndex].IsAlive)
        {
            return;
        }
        
        _currentSaveState.playerCharacters[unitIndex].currentDamageMultiplier += (amount / 100);
        
        _battleMenu.UpdatePlayerCharacterEmpowerUI(unitIndex,
            (_currentSaveState.playerCharacters[unitIndex].currentDamageMultiplier - 1) * 100);
    }

    private void ResetPlayerEmpower(int unitIndex)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty || !_currentSaveState.playerCharacters[unitIndex].IsAlive)
        {
            return;
        }
        
        _currentSaveState.playerCharacters[unitIndex].currentDamageMultiplier = 1;
        
        _battleMenu.UpdatePlayerCharacterEmpowerUI(unitIndex,
            (_currentSaveState.playerCharacters[unitIndex].currentDamageMultiplier - 1) * 100);
    }

    private void AddOrder(float newValue)
    {
        _currentSaveState.currentOrderTokens = newValue;
        _battleMenu.UpdateOrderTokenText(_currentSaveState.currentOrderTokens);
    }

    private void GuardPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty || !_currentSaveState.playerCharacters[unitIndex].IsAlive)
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
        if (_currentSaveState.playerCharacters[index].classType == ClassType.Empty || !_currentSaveState.playerCharacters[index].IsAlive)
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
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty || !_currentSaveState.playerCharacters[unitIndex].IsAlive)
        {
            return;
        }
        
        if (_currentSaveState.playerCharacters[unitIndex].currentHealth + amount >=
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
            if (characterData.classType != ClassType.Empty && characterData.IsAlive)
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

            if (_currentSaveState.playerCharacters[playerIndex].classType != ClassType.Empty && _currentSaveState.playerCharacters[playerIndex].IsAlive)
            {
                foundNotEmpty = true;
            }
        }

        return playerIndex;
    }
    
    private void EnemyAttackPlayer(int playerIndex, float num)
    {
        int sentinelValue = FocusSentinelCheckForFullPoints();

        if (sentinelValue != -1)
        {
            playerIndex = sentinelValue;
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
        
        PlayerTakeDamage(playerIndex, newDamageNum);
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

        if (!_currentSaveState.playerCharacters[index].IsAlive)
        {
            PlayerDead(index);
        }
        
    }

    private void PlayerDead(int unitIndex)
    {
        _battleMenu.KillPlayer(unitIndex);
        _fieldController.KillPlayer(_currentSaveState.playerCharacters[unitIndex]);
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

    private void OnDestroy()
    {
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
        }
    }
}

[Serializable]
public class IntentTextDetails
{
    public PlayerActionType type;
    public string spriteIndex;
    public Color textColor;
}
