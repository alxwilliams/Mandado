using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public partial class BattleSystem : BaseSystem
{
    [SerializeField] private int _amountOfDiceRolledPerTurn = 5;
    //[SerializeField] private List<EnemyCharacter> _fakeEnemyData = new List<EnemyCharacter>();
    [SerializeField] private List<EnemyCharacter> _enemyCharacters = new List<EnemyCharacter>();
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

    private bool _enemyDied = false;
    private List<PlayerCharacter> _newGamePlayerCharacters = new List<PlayerCharacter>();
    private Dictionary<PlayerActionType, IntentTextDetails> _playerIntentDetailsDictionary = new Dictionary<PlayerActionType, IntentTextDetails>();
    private CameraSystem _cameraSystem;
    private MenuSystem _menuSystem;
    private Coroutine _attackRoutine;
    private Coroutine _loadCharactersRoutine;
    
    private EnemyAttackSet _currentEnemyAttackSet;
    private int _currentEnemyAttackIndex;

    private BattleSystemState _currentSaveState;

    public Action GameLoaded;
    private Action SaveGame;

    public bool IsOrderUsable
    {
        get
        {
            return _currentSaveState.currentOrderTokens > 0;
        }
    }

    public void Initialize(GameManager gameManager, Action saveGame)
    {
        SaveGame = saveGame;
        _cameraSystem = gameManager.CameraSystem;
        _menuSystem = gameManager.MenuSystem;
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
        
        _enemyDied = false;
        GameLoaded?.Invoke();
        _loadCharactersRoutine = null;
    }

    public void UseOrderToken()
    {
        AddOrder(_currentSaveState.currentOrderTokens-1);
        SaveGame?.Invoke();
    }
    
    public bool IsIndexCharacterEmpty(int index)
    {
        return _currentSaveState.playerCharacters[index].classType == ClassType.Empty || !_currentSaveState.playerCharacters[index].IsAlive;
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

    public void StartNewBattle(int newEnemyIndex)
    {
        if (newEnemyIndex >= _enemyCharacters.Count)
        {
            //You Win!
            Debug.Log("YOU WIN!");
        }
        else
        {
            BattleSystemState newState = new BattleSystemState();
            
            newState.currentEnemyIndex = newEnemyIndex;
            
            if (_loadCharactersRoutine != null)
            {
                StopCoroutine(_loadCharactersRoutine);
            }
            
            _loadCharactersRoutine = StartCoroutine(LoadNewBattleCharacters(newState, _newGamePlayerCharacters, _enemyCharacters[newState.currentEnemyIndex]));
        }
    }


    public IEnumerator LoadNewBattleCharacters(BattleSystemState state, List<PlayerCharacter> playerCharacters, EnemyCharacter enemyCharacter)
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

        enemyData.Add(enemyCharacter.GetFullHealthCharacterData());

        _battleMenu.LoadInCharacterUI(playerData, enemyData[0]);
        
        yield return _fieldController.LoadEnemyCharactersCoroutine(enemyData);
        _currentSaveState.enemyCharacters = enemyData;
        UpdateUI();
        LoadInBattleState(_currentSaveState);
        
        GameLoaded?.Invoke();
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
        _battleMenu.UpdateCrippledState(_currentSaveState.amountOfCripple);
    }

    private bool CheckEnemyHealth()
    {
        if (!_currentSaveState.enemyCharacters[0].IsAlive)
        {
            _gameManager.ShowRewardMenu();
            return true;
        }

        return false;
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
        
        bool playerBled = DealWithPlayerBleedDamage();

        if (playerBled)
        {
            yield return  new WaitForSeconds(_timeAfterBleedDamage);
        }
        
        if(!_enemyDied)
        {
            _currentSaveState.activeDiceRolls = new int[6];

            //enemy attack
            EnemyRollDice();

            _cameraSystem.SwitchToEnemyView();
            yield return new WaitForSeconds(_timeBeforeEnemyAttacks);

            yield return EnemyAttackActions();

            yield return new WaitForSeconds(_timeAfterEnemyAttacks);

            EndEnemyTurn();
            
            if(!_enemyDied)
            {
                StartPlayerTurn();
            }
            else
            {
                _cameraSystem.SwitchToPlayerView();
            }
        }

        _attackRoutine = null;
    }

    private void EndEnemyTurn()
    {
        DealWithEnemyBleedDamage();
        
        if(!CheckEnemyHealth())
        {
            _currentSaveState.turnNumber++;
            SaveGame?.Invoke();
        }
        else
        {
            _currentSaveState.rewardData.enemyKilledOnTheirTurn = true;
            _enemyDied = true;
        }
    }

    private void StartPlayerTurn(bool init = false)
    {
        _currentSaveState.amountOfRerolls = 3;
        ResetPlayerGuard();
        ResetPlayerDiceTrays();
        GenerateEnemyActions();

        RollDice();
        UpdateUI();
        SaveGame?.Invoke();
        
        if(!init)
        {
            _cameraSystem.SwitchToPlayerView();
        }
    }

    private void ResetEnemyGuard()
    {
        if (_currentSaveState.enemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Guard) &&
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard] != 0)
        {
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard] = 0;
            _battleMenu.UpdateEnemyGuardUI(0);
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
                EnemyTakeDamage(character.statusEffects[StatusEffects.Bleed], StatusEffects.Bleed);
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
                PlayerTakeDamage(character.currentIndex,character.statusEffects[StatusEffects.Bleed], StatusEffects.Bleed);
                character.statusEffects[StatusEffects.Bleed]--;
                
                _battleMenu.UpdatePlayerCharacterBleedUI(character.currentIndex, character.statusEffects[StatusEffects.Bleed]);
                playerBled = true;
            }
        }

        return playerBled;
    }

    private void EndPlayerTurn()
    {
        if (_currentSaveState.amountOfRerolls == 3)
        {
            _currentSaveState.rewardData.noRerollCount++;
        }
        _currentSaveState.canAttack = false;
        _currentSaveState.amountOfCripple = 0;
        UpdateUI();
    }

    private IEnumerator PlayerDiceActions()
    {
        for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
        {
            if (_currentSaveState.activeDiceRolls[i] > 0 && _currentSaveState.playerCharacters[i].IsAlive && _currentSaveState.playerCharacters[i].classType != ClassType.Empty)
            {
                yield return DealWithPlayerAction(_currentSaveState.playerCharacters[i].actionSet.GetActionSetFromRollNumber(_currentSaveState.activeDiceRolls[i]),
                    _currentSaveState.playerCharacters[i],i,_currentSaveState.activeDiceRolls[i]);

                if (_enemyDied)
                {
                    break;
                }
                
                ResetIndexedPlayerFocus(i);
                
                UpdateUI();
                
                yield return new WaitForSeconds(_waitTimeBetweenAttacks);
            }
            else if(_currentSaveState.playerCharacters[i].currentFocus < 3 && _currentSaveState.playerCharacters[i].currentHealth > 0)
            {
                _currentSaveState.playerCharacters[i].currentFocus++;
                CheckForPassiveFocusAbilities(i);
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
        SaveGame?.Invoke();
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
                if (condition.condition == EnemyAttackCondition.HealthLowerThan && currentEnemy.currentHealth > condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.HealthGreaterThan && currentEnemy.currentHealth < condition.value)
                {
                    conditionPass = false;
                    break;
                }

                if (condition.condition == EnemyAttackCondition.TurnCountEqualTo && _currentSaveState.turnNumber != condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.TurnCountGreaterThan && _currentSaveState.turnNumber < condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.TurnCountLessThan && _currentSaveState.turnNumber > condition.value)
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
            _currentEnemyAttackSet = listOfAttacks[_gameManager.GetNewMainRandom(0,listOfAttacks.Count)];
        }

        foreach (var action in _currentEnemyAttackSet.sequencedAttacks[_currentEnemyAttackIndex].actionSet)
        {
            if (action.type == EnemyActionType.DamageRandom ||
                action.type == EnemyActionType.BleedRandom ||
                action.type == EnemyActionType.DamageAndDamageNearby)
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
            
            if (action.type == EnemyActionType.DamageAndDamageNearby)
            {
                if(!maxRoll)
                {
                    EnemyAttackPlayer(action.targetIndex,action.value);
                    EnemyAttackPlayer(action.targetIndex-1,action.value);
                    EnemyAttackPlayer(action.targetIndex+1,action.value);
                }
                else
                {
                    //yield return new WaitForSeconds(_fieldController.CharacterBigAttack(data));
                    EnemyAttackPlayer(action.targetIndex,action.value);
                    EnemyAttackPlayer(action.targetIndex-1,action.value);
                    EnemyAttackPlayer(action.targetIndex+1,action.value);
                }
            }

            if (action.type == EnemyActionType.Cripple)
            {
                PlayerGainCripple(action.value);
            }


            if (action.type == EnemyActionType.Guard)
            {
                GuardEnemyUnit(action.value);
            }

            if (action.type == EnemyActionType.Heal)
            {
                HealEnemyUnit(action.value);
            }
            
            if (action.type == EnemyActionType.BleedRandom)
            {
                ApplyPlayerBleed(action.targetIndex, action.value);
            }

            if (action.type == EnemyActionType.BleedAll)
            {
                for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
                {
                    ApplyPlayerBleed(i,action.value);
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

            if (data.enemyFocusAbility == EnemyFocusAbility.Cabra)
            {
                yield return new WaitForSeconds(_fieldController.EnemyCharacterMoveForward(data));
                CabraFocusAttack();
            }

            data.currentFocus = 0;
            _battleMenu.UpdateEnemyUI(data);
        }
    }

    private void PlayerGainCripple(float amountOfCripple)
    {
        _currentSaveState.amountOfCripple += (int)amountOfCripple;
        _battleMenu.UpdateCrippledState(_currentSaveState.amountOfCripple);
        //player rolls less dice
    }

    private IEnumerator DealWithPlayerAction(List<PlayerCharacterAction> actions, PlayerCharacterData data, int castingUnitIndex, int rollNumber)
    {
        bool maxRoll = rollNumber == 5;

        if (maxRoll)
        {
            _currentSaveState.rewardData.fiveOfAKindsUsed++;
        }
        
        yield return new WaitForSeconds(_fieldController.PlayerCharacterMoveForward(data));

        foreach (var action in actions)
        {
            if (action.type == PlayerActionType.Damage)
            {

                float damage = action.value;
                
                if (data.statusEffects.ContainsKey(StatusEffects.Empower) &&
                    data.statusEffects[StatusEffects.Empower] > 0)
                {
                    damage += data.statusEffects[StatusEffects.Empower];

                    data.statusEffects[StatusEffects.Empower] = 0;
                    _battleMenu.UpdatePlayerCharacterEmpowerUI(castingUnitIndex, 0);
                }
                
                if(!maxRoll)
                {
                    PlayerAttackEnemy(damage);
                }
                else
                {
                    PlayerAttackEnemy(damage);
                }


                if (CheckEnemyHealth())
                {
                    _enemyDied = true;
                    break;
                }
                
            }

            if (action.type == PlayerActionType.Bleed)
            {
                ApplyEnemyBleed(action.value);
            }


            if (action.type == PlayerActionType.Heal)
            {

                for (int i = 0; i < 6;i++) 
                {
                    HealPlayerUnit(i, action.value);
                }
                    
                _currentSaveState.playerHasHealed = true;
            }
            

            if (action.type == PlayerActionType.Guard)
            {
                for (int i = 0; i < 6;i++) 
                {
                    GuardPlayerUnit(i, action.value);
                }
                
            }

            if (action.type == PlayerActionType.Order)
            {
                AddOrder(action.value);
            }

            if (action.type == PlayerActionType.Empower)
            {
                for (int i = 0; i < 6;i++) 
                {
                    EmpowerPlayerUnit(i, action.value);
                }
                
            }

            if (action.type == PlayerActionType.Inspire)
            {
                for (int i = 0; i < 6;i++) 
                {
                    _currentSaveState.playerCharacters[i].currentFocus++;
                    _battleMenu.UpdatePlayerCharacterFocus(_currentSaveState.playerCharacters[i]);
                }
                
            }

            yield return new WaitForSeconds(0.1f);
        }

        if(!_enemyDied)
        {
            _fieldController.WipePlayerCharacterIntent(data);
            _battleMenu.ResetDice(castingUnitIndex);
        }
    }

    private void EmpowerPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty || !_currentSaveState.playerCharacters[unitIndex].IsAlive)
        {
            return;
        }
        
        if (_currentSaveState.playerCharacters[unitIndex].statusEffects.ContainsKey(StatusEffects.Empower))
        {
            _currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Empower] += amount;
        }
        else
        {
            _currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Empower] = amount;
        }
        
        
        _battleMenu.UpdatePlayerCharacterEmpowerUI(unitIndex,
            (_currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Empower]));
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

    private void ApplyPlayerBleed(int index, float amount)
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
        
        
        _fieldController.PlayerGainBleedDebuff(_currentSaveState.playerCharacters[index]);
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
        
        _fieldController.EnemyGainBleedDebuff(_currentSaveState.enemyCharacters[0]);
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
    
    private void GuardEnemyUnit(float amount)
    {

        if (_currentSaveState.enemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Guard))
        {
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard] += amount;
        }
        else
        {
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard] = amount;
        }
        
        _battleMenu.UpdateEnemyGuardUI(_currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard]);
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
        int enemyIndex = 0;
        float newDamageNum = CheckStatusEffectsForGuard(ref _currentSaveState.enemyCharacters[enemyIndex].statusEffects, num);

        if (num != newDamageNum)
        {
            _battleMenu.UpdateEnemyGuardUI(_currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard]);
        }

        EnemyTakeDamage(newDamageNum);

        if (newDamageNum >= _currentSaveState.enemyCharacters[0].maxHealth / 2)
        {
            _currentSaveState.rewardData.fiftyPercentDamageOneAttack = true;
        }
        
        //todo, check full damage attack here
        
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
        if (playerIndex < 0 || playerIndex >= 6)
        {
            return;
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

    private void EnemyTakeDamage(float num, StatusEffects effect = StatusEffects.Guard)
    {
        _currentSaveState.enemyCharacters[0].currentHealth -= num;

        if (effect == StatusEffects.Bleed)
        {
            _fieldController.EnemyTakeBleedDamage(_currentSaveState.enemyCharacters[0], num);
        }
        else
        {
            _fieldController.EnemyTakeDamageDamage(_currentSaveState.enemyCharacters[0], num);
        }
        
        
        _battleMenu.UpdateEnemyUI(_currentSaveState.enemyCharacters[0]);
    }
    
    private void PlayerTakeDamage(int index, float num, StatusEffects effect = StatusEffects.Guard)
    {
        _currentSaveState.playerCharacters[index].currentHealth -= num;

        if (effect == StatusEffects.Bleed)
        {
            _fieldController.PlayerTakeBleedDamage(_currentSaveState.playerCharacters[index], num);
        }
        else
        {
            _fieldController.PlayerTakeDamageDamage(_currentSaveState.playerCharacters[index], num);
        }
        
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
                float newDamage = Mathf.Abs(effects[StatusEffects.Guard]);
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
        
        _battleMenu.ShakeDiceTray();

        int diceRolled = _amountOfDiceRolledPerTurn - _currentSaveState.diceInCharacterTrays - _currentSaveState.amountOfCripple;

        int[] dice = new int[diceRolled];
        
        for(int i =0; i < diceRolled; i++)
        {
            dice[i] = _gameManager.GetNewMainRandom(1, 7);
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
        SaveGame?.Invoke();
        
    }
    
    private void EnemyRollDice()
    {
        
        int[] dice = new int[_amountOfDiceRolledPerTurn];
        _currentSaveState.activeDiceRolls = new[] { 0, 0, 0, 0, 0, 0};
        string debugString = "Enemy Dice:\n";

        //only rolling 5 dice
        for(int i =0; i < _amountOfDiceRolledPerTurn; i++)
        {
            dice[i] = _gameManager.GetNewMainRandom(1, 7);
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
