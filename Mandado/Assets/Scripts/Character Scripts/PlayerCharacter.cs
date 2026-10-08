using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Character", menuName = "Character/Player Character")]
public class PlayerCharacter : BaseCharacter
{
    [SerializeField] private List<LevelUpActionSet> _levelUpSets;
    [SerializeField] private DiceActionSet _baseActionSet;
    [SerializeField] private ClassType _classType;

    public ClassType GetClassType()
    {
        return _classType;
    }
    
    public PlayerCharacterData GetFullHealthCharacterData()
    {
        PlayerCharacterData data = new PlayerCharacterData();
        
        data.name = _name;
        data.classType = _classType;
        data.currentHealth =_baseHealth;
        data.maxHealth = _baseHealth;
        data.currentFocus = 0;
        data.actionSet = _baseActionSet;

        data.characterSpriteData = _characterSpriteData;
        
        data.statusEffects = new Dictionary<StatusEffects, float>();
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
    public ClassType classType;
    public float currentHealth;
    public float maxHealth;
    public float currentFocus;
    public DiceActionSet actionSet;
    public SerializableStatusDictionary statusEffectsSerialized;
    public Dictionary<StatusEffects, float> statusEffects;
    public float currentDamageMultiplier = 1;
    public float width;
    public CharacterSpriteData characterSpriteData;
    public int currentIndex;


    public PlayerCharacterData()
    {
        classType = ClassType.Empty;
    }
    
    public bool IsAlive
    {
        get
        {
            return currentHealth > 0 && classType != ClassType.Empty;
        }
    }
}

public enum ClassType
{
    Sentinel,
    Pilgrim,
    Warrior,
    Empty
}



