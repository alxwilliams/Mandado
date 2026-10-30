
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BattleSystem _battleSystem;
    [SerializeField] private MenuSystem _menuSystem;
    [SerializeField] private CameraSystem _cameraSystem;
    [SerializeField] private SaveSystem _saveSystem;
    [SerializeField] private RewardSystem _rewardSystem;
    
    private string _mainDiceSeed;
    private string _targetSeed;
    private string _miscSeed;

    private Random _mainRandom;
    private Random _targetRandom;
    private Random _miscRandom;

    private SaveData _currentSaveData;

    private Coroutine _initializationRoutine;
    
    public static GameManager Instance { get; private set; }
    
    public static Action InitializedEvent;

    public MenuSystem MenuSystem => _menuSystem;
    public BattleSystem BattleSystem => _battleSystem;

    public SaveSystem SaveSystem => _saveSystem;

    public CameraSystem CameraSystem => _cameraSystem;

    private bool _initialized = false;

    public bool Initialized => _initialized;

    public Camera MainCamera => _cameraSystem.MainCamera;

    public string MainDiceSeed => _mainDiceSeed;

    private bool _inBattle = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (_initializationRoutine != null)
            {
                StopCoroutine(_initializationRoutine);
            }
            
            _initializationRoutine = StartCoroutine(Initialize());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Initialize()
    {
        _menuSystem.Initialize(this, StartNewAdventure, LoadAdventureFromFile);
        _battleSystem.Initialize(this,SaveGameWithBattle);
        _cameraSystem.Initialize(this);
        _rewardSystem.Initialize(this);
        
        Task loadTask = _saveSystem.Initialize(this);
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (_saveSystem.LoadedGameFound)
        {
            _currentSaveData = _saveSystem.LoadGame();
        }
        else
        {
            _currentSaveData = new SaveData();
            SaveGame();
        }
        
        _initialized = true;
        InitializedEvent?.Invoke();
        _initializationRoutine = null;

        _battleSystem.GameLoaded += StartBattle;
        
        yield return null;
        _menuSystem.ShowTitleScreen();
    }

    public void GameOver()
    {
        BackToTitleScreen();
        ResetBattle();
    }

    public void Victory()
    {
        BackToTitleScreen();
        ResetBattle();
    }

    private void ResetBattle()
    {
        _currentSaveData.battleState = new BattleSystemState();
        SaveGame();
    }

    private void BackToTitleScreen()
    {
        _inBattle = false;
        _menuSystem.ShowTitleScreen();
    }
    
    private void GenerateNewSeed()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[15];
        
        Random tempRng = new Random(); 
        for (int i = 0; i < 15; i++)
        {
            stringChars[i] = chars[tempRng.Next(chars.Length)];
        }

        _mainDiceSeed = new string(stringChars);
    }

    public void AddCoins(int coins)
    {
        _currentSaveData.coins += coins;
        SaveGame();
    }

    public int GetCoinData()
    {
        return _currentSaveData.coins;
    }

    public void ShowRewardMenu()
    {
        _menuSystem.ShowRewardMenu(_rewardSystem.GetRewards());
    }
    
    private void GenerateRandomMachines()
    {
        int numericSeed = StringToDeterministicHash(_mainDiceSeed);
        
        _mainRandom = new Random(numericSeed);
        
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[15];
        char[] stringChars2 = new char[15];
        
        
        for (int i = 0; i < 15; i++)
        {
            stringChars[i] = chars[_mainRandom.Next(chars.Length)];
        }
        for (int i = 0; i < 15; i++)
        {
            stringChars2[i] = chars[_mainRandom.Next(chars.Length)];
        }
        
        _targetSeed= new string(stringChars);
        _miscSeed= new string(stringChars2);

        _targetRandom = new Random(StringToDeterministicHash(_targetSeed));
        _miscRandom = new Random(StringToDeterministicHash(_miscSeed));
    }

    public int GetNewMainRandom(int inclusiveFloor, int exclusiveCeiling)
    {
        int num = _mainRandom.Next(inclusiveFloor, exclusiveCeiling);
        _currentSaveData.battleState.mainRandomCalls++;
        return num;
    }
    
    public int GetNewTargetRandom(int inclusiveFloor, int exclusiveCeiling)
    {
        int num = _mainRandom.Next(inclusiveFloor, exclusiveCeiling);
        _currentSaveData.battleState.targetRandomCalls++;
        return num;
    }
    
    public int GetNewMiscRandom(int inclusiveFloor, int exclusiveCeiling)
    {
        int num = _mainRandom.Next(inclusiveFloor, exclusiveCeiling);
        _currentSaveData.battleState.miscRandomCalls++;
        return num;
    }

    [ContextMenu("Start New Game")]
    private void StartNewAdventure()
    {
        if (_inBattle)
        {
            return;
        }

        _inBattle = true;
        _currentSaveData.battleState.battleStarted = true;
        GenerateNewSeed();
        GenerateRandomMachines();
        _currentSaveData.battleState = new BattleSystemState();
        _currentSaveData.battleState.mainRandomCalls = 0;
        _currentSaveData.battleState.targetRandomCalls = 0;
        _currentSaveData.battleState.miscRandomCalls = 0;

        _battleSystem.GoToNextSequencedEvent(0);
    }

    public void LoadAdventureFromFile()
    {
        if (!_saveSystem.LoadedGameFound || _inBattle || !_currentSaveData.battleState.battleStarted)
        {
            return;
        }

        _inBattle = true;
        var data = _saveSystem.LoadGame();
        _mainDiceSeed = data.battleState.mainSeed;
        GenerateRandomMachines();
        _currentSaveData = data;

        for (int i = 0; i < data.battleState.mainRandomCalls;i++)
        {
            _mainRandom.Next();
        }
        for (int i = 0; i < data.battleState.targetRandomCalls;i++)
        {
            _targetRandom.Next();
        }
        for (int i = 0; i < data.battleState.miscRandomCalls;i++)
        {
            _miscRandom.Next();
        }
        
        _battleSystem.LoadSavedBattle(data.battleState);
    }

    public void SaveGame()
    {
        _saveSystem.SaveGame(_currentSaveData);
    }
    
    public void SaveGameWithBattle()
    {
        _currentSaveData.battleState = _battleSystem.GetBattleSystemState();
        SaveGame();
    }

    public void StartNextIndexedBattle()
    {
        _currentSaveData.battleState.currentEnemyIndex++;
        _battleSystem.GoToNextSequencedEvent(_currentSaveData.battleState.currentEnemyIndex);
    }
    
    private int StringToDeterministicHash(string str)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in str)
            {
                char upperChar = char.ToUpperInvariant(c);
                hash = (hash * 31) + upperChar;
            }
            return hash;
        }
    }
    

    public void StartBattle()
    {
        _menuSystem.ShowBattleMenu();
    }

    private void OnDestroy()
    {
        if (_initializationRoutine != null)
        {
            StopCoroutine(_initializationRoutine);
        }
        InitializedEvent = null;
        
        
        _battleSystem.GameLoaded -= StartBattle;
    }
}
