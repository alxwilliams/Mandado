using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMenu : MonoBehaviour
{
    private bool _initialized = false;
    public Action InitializedAction;
    private bool _active = false;

    public bool Active
    {
        get => _active;
    }

    public virtual void Initialize()
    {
        InitializedAction?.Invoke();
        _initialized = true;
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
        _active = show;
    }
}
