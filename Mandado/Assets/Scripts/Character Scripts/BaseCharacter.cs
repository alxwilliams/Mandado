using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private DiceActionSet _baseActionSet;
    [SerializeField] private float _width = 10;
    
    [Header("Sprites")]
    [SerializeField] private Sprite _frontSprite;
    [SerializeField] private Sprite _backSprite;



    public CharacterData GetFullHealthCharacterData()
    {
        CharacterData data = new CharacterData();
        
        data.name = _name;
        data.currentHealth =_baseActionSet.maxHealth;
        data.actionSet = _baseActionSet;
        
        data.frontSprite = _frontSprite;
        data.backSprite = _backSprite;
        data.width = _width;

        return data;
    }

}

[Serializable]
public class CharacterData
{
    public string name;
    public float currentHealth;
    public DiceActionSet actionSet;
    public float width;
    public Sprite frontSprite;
    public Sprite backSprite;
}
