using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Character", menuName = "Character/Player Character")]
public class PlayerCharacter : BaseCharacter
{
    [SerializeField] private List<LevelUpActionSet> _levelUpSets;
}

[Serializable]
public class LevelUpActionSet
{
    [SerializeField] private string _id;
    
    [Header("Level 2")]
    [SerializeField] private string _levelTwoPrefixName;
    [SerializeField] private DiceActionSet _levelTwoAction;
    
    [Header("Level 3")]
    [SerializeField] private string _levelThreePrefixName;
    [SerializeField] private DiceActionSet _levelThreeAction;
}


