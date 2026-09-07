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

    private CharacterData _data;
    private bool _characterLoaded = false;

    private void OnEnable()
    {
        _characterLoaded = false;
    }

    public CharacterData GetFullHealthCharacterData()
    {
        if (!_characterLoaded)
        {
            _data.name = _name;
            
            _data.frontSprite = _frontSprite;
            _data.backSprite = _backSprite;
            _data.width = _width;

            _characterLoaded = true;
        }

        return _data;
    }

}

[Serializable]
public class CharacterData
{
    public string name;
    public float width;
    public Sprite frontSprite;
    public Sprite backSprite;
}
