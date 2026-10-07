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
    [SerializeField] protected CharacterSpriteData _characterSpriteData;
}

[Serializable]
public class CharacterSpriteData
{
    public Sprite _frontIdleSprite1;
    public Sprite _frontIdleSprite2;
    public Sprite _frontAttackSprite1;
    public Sprite _frontAttackSprite2;
    public Sprite _frontAttackSprite3;
    public Sprite _characterLabel;
}

public enum StatusEffects
{
    Guard,
    Bleed
}