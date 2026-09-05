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
    [SerializeField] private string _prefixName;
    [SerializeField] private DiceActionSet _levelTwoAction;
    [SerializeField] private DiceActionSet _levelThreeAction;
}


