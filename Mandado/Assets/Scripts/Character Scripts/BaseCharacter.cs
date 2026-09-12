using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : ScriptableObject
{
    [SerializeField] protected string _name;
    [SerializeField] protected float _width = 10;
    [SerializeField] protected float _baseHealth = 15;
    
    [Header("Sprites")]
    [SerializeField] protected Sprite _frontSprite;
    [SerializeField] protected Sprite _backSprite;

    

}

