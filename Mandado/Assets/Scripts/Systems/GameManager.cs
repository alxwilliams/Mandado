
using System;
using System.Collections;
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

    public int GetNewMainRandom(int floor, int ceiling)
    {
        int num = _mainRandom.Next(floor, ceiling);
        _currentSaveData.mainRandomCalls++;
        return num;
    }
    
    public int GetNewTargetRandom(int floor, int ceiling)
    {
        int num = _mainRandom.Next(floor, ceiling);
        _currentSaveData.targetRandomCalls++;
        return num;
    }
    
    public int GetNewMiscRandom(int floor, int ceiling)
    {
        int num = _mainRandom.Next(floor, ceiling);
        _currentSaveData.miscRandomCalls++;
        return num;
    }

    private void StartNewGame()
    {
        GenerateNewSeed();
        GenerateRandomMachines();
        _currentSaveData = new SaveData();
        _currentSaveData.mainRandomCalls = 0;
        _currentSaveData.targetRandomCalls = 0;
        _currentSaveData.miscRandomCalls = 0;

        StartBattle();
    }

    [ContextMenu("Load")]
    public void LoadGameFromFile()
    {
        var data = _saveSystem.LoadGame();
        _mainDiceSeed = data.mainSeed;
        GenerateRandomMachines();
        _currentSaveData = data;

        for (int i = 0; i < data.mainRandomCalls;i++)
        {
            _mainRandom.Next();
        }
        for (int i = 0; i < data.targetRandomCalls;i++)
        {
            _targetRandom.Next();
        }
        for (int i = 0; i < data.miscRandomCalls;i++)
        {
            _miscRandom.Next();
        }
        
        _battleSystem.LoadSavedBattle(data.battleState);
    }

    [ContextMenu("Save")]
    public void SaveGame()
    {
        _currentSaveData.battleState = _battleSystem.GetBattleSystemState();
        _saveSystem.SaveGame(_currentSaveData);
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
        _menuSystem.Initialize(this);
        _battleSystem.Initialize(this);
        _cameraSystem.Initialize(this);
        _saveSystem.Initialize(this);
        
        _initialized = true;
        StartNewGame();
        InitializedEvent?.Invoke();
        _initializationRoutine = null;
        
        yield return null;
    }

    public void StartBattle()
    {
        _battleSystem.ShowBattleMenu();
        _battleSystem.StartNewBattle();
    }

    private void OnDestroy()
    {
        if (_initializationRoutine != null)
        {
            StopCoroutine(_initializationRoutine);
        }
        InitializedEvent = null;
    }
}
