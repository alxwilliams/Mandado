using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private DiceActionSet _baseActionSet;
    
    [Header("Sprites")]
    [SerializeField] private Sprite _frontSprite;
    [SerializeField] private Sprite _backSprite;
}
