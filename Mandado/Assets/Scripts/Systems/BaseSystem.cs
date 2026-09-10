using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSystem : MonoBehaviour
{
    private bool _initialized = false;
    public Action InitializedAction;
    protected GameManager _gameManager;

    public bool Initialized => _initialized;

    public virtual void Initialize(GameManager gameManager)
    {
        if (_initialized)
        {
            return;
        }
        _gameManager = gameManager;
        InitializedAction?.Invoke();
        _initialized = true;
    }

}
