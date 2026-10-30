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
        _battleMenu.Initialize(gameManager.MenuSystem, RollDice, StartAttackSequence, IncreaseActiveDiceRolls, DecreaseActiveDiceRolls);

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

    private void StartAttackSequence()
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
        
        PlayerStraightCheck();
        
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

            ResetEnemyGuard();
            
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

    private void PlayerStraightCheck()
    {
        if (_currentSaveState.activeDiceRolls[0] > 0 &&
            _currentSaveState.activeDiceRolls[1] > 0 &&
            _currentSaveState.activeDiceRolls[2] > 0 &&
            _currentSaveState.activeDiceRolls[3] > 0 &&
            _currentSaveState.activeDiceRolls[4] > 0)
        {
            _currentSaveState.rewardData.straightsUsed ++;
        }
        
        if (_currentSaveState.activeDiceRolls[5] > 0 &&
            _currentSaveState.activeDiceRolls[1] > 0 &&
            _currentSaveState.activeDiceRolls[2] > 0 &&
            _currentSaveState.activeDiceRolls[3] > 0 &&
            _currentSaveState.activeDiceRolls[4] > 0)
        {
            _currentSaveState.rewardData.straightsUsed ++;
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

    private void PlayerGainCripple(float amountOfCripple)
    {
        _currentSaveState.amountOfCripple += (int)amountOfCripple;
        _battleMenu.UpdateCrippledState(_currentSaveState.amountOfCripple);
        //player rolls less dice
    }

    private void AddOrder(float newValue)
    {
        _currentSaveState.currentOrderTokens = newValue;
        _battleMenu.UpdateOrderTokenText(_currentSaveState.currentOrderTokens);
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

    private void EnemyDamagedRewardChecks(float damage)
    {
        float halfHealth = _currentSaveState.enemyCharacters[0].maxHealth / 2;
        
        if (_currentSaveState.enemyCharacters[0].currentHealth >= halfHealth &&
            damage >= halfHealth)
        {
            _currentSaveState.rewardData.fiftyPercentDamageOneAttack = true;
        }

        if (!_currentSaveState.enemyCharacters[0].IsAlive && !_currentSaveState.enemyCharacters[0].tookDamageThisBattle)
        {
            _currentSaveState.rewardData.enemyKilledWithOneAttack = true;
        }

        if (!_currentSaveState.enemyCharacters[0].tookDamageThisBattle &&
            _currentSaveState.enemyCharacters[0].currentHealth != _currentSaveState.enemyCharacters[0].maxHealth)
        {
            _currentSaveState.enemyCharacters[0].tookDamageThisBattle = true;
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
