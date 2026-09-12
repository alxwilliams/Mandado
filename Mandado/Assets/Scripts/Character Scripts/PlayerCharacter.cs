using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Character", menuName = "Character/Player Character")]
public class PlayerCharacter : BaseCharacter
{
    [SerializeField] private List<LevelUpActionSet> _levelUpSets;
    [SerializeField] private DiceActionSet _baseActionSet;
    
    public PlayerCharacterData GetFullHealthCharacterData()
    {
        PlayerCharacterData data = new PlayerCharacterData();
        
        data.name = _name;
        data.currentHealth =_baseHealth;
        data.actionSet = _baseActionSet;
        
        data.frontSprite = _frontSprite;
        data.backSprite = _backSprite;
        data.width = _width;

        return data;
    }
}

[Serializable]
public class LevelUpActionSet
{
    [SerializeField] private string _id;
    
    [Header("Level 2")]
    [SerializeField] private string _levelTwoPrefixName;
    [SerializeField] private float _levelTwoHealth = 20;
    [SerializeField] private DiceActionSet _levelTwoAction;
    
    [Header("Level 3")]
    [SerializeField] private string _levelThreePrefixName;
    [SerializeField] private float _levelThreeHealth = 35;
    [SerializeField] private DiceActionSet _levelThreeAction;
}

[Serializable]
public class PlayerCharacterData
{
    public string name;
    public float currentHealth;
    public DiceActionSet actionSet;
    public float width;
    public Sprite frontSprite;
    public Sprite backSprite;
}



