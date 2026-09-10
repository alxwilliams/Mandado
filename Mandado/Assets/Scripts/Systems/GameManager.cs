
using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BattleSystem _battleSystem;
    [SerializeField] private MenuSystem _menuSystem;
    [SerializeField] private Camera _camera;    
    public static GameManager Instance { get; private set; }
    public static Action InitializedEvent;

    public MenuSystem MenuSystem => _menuSystem;
    public BattleSystem BattleSystem => _battleSystem;

    private bool _initialized = false;

    public bool Initialized => _initialized;

    public Camera Camera => _camera;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            StartCoroutine(Initialize());
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
        
        _initialized = true;
        InitializedEvent?.Invoke();
        
        yield return null;
    }

    [ContextMenu("Show Battle Menu")]
    public void StartBattle()
    {
        _battleSystem.ShowBattleMenu();
    }

}
