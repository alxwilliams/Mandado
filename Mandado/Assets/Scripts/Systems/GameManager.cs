
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

    public CameraSystem CameraSystem => _cameraSystem;

    private bool _initialized = false;

    public bool Initialized => _initialized;

    public Camera MainCamera => _cameraSystem.MainCamera;

    public string MainDiceSeed => _mainDiceSeed;

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
        GenerateNewSeed();
        GenerateRandomMachines();
        _currentSaveData.battleState = new BattleSystemState();
        _currentSaveData.battleState.mainRandomCalls = 0;
        _currentSaveData.battleState.targetRandomCalls = 0;
        _currentSaveData.battleState.miscRandomCalls = 0;

        _battleSystem.StartNewBattle(0);
    }

    public void LoadAdventureFromFile()
    {
        if (!_saveSystem.LoadedGameFound)
        {
            return;
        }
        
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
