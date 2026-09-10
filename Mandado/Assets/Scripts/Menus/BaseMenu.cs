using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMenu : MonoBehaviour
{
    private bool _initialized = false;
    public Action InitializedAction;
    private bool _active = false;

    private MenuSystem _menuSystem;

    public bool Active
    {
        get => _active;
    }

    public virtual void Initialize(MenuSystem menuSystem)
    {
        if (_initialized)
        {
            return;
        }
        _menuSystem = menuSystem;
        InitializedAction?.Invoke();
        _initialized = true;
    }

    public void Show(bool show)
    {
        if(show)
        {
            _menuSystem.SetNewMenu(this);
        }
        gameObject.SetActive(show);
        _active = show;
    }
}
