using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DiceActionSet
{
    public List<PlayerCharacterAction> rollOneActions;
    public List<PlayerCharacterAction> rollTwoActions;
    public List<PlayerCharacterAction> rollThreeActions;
    public List<PlayerCharacterAction> rollFourActions;
    public List<PlayerCharacterAction> rollFiveActions;

    [Header("Focus")]
    public string focusValueDescription;
    public List<float> focusValues;
}


[Serializable]
public class EnemyAttackSet
{
    public List<ConditionValueCombo> conditionList;
    public List<EnemyAttackSequence> sequencedAttacks;
}

[Serializable]
public class EnemyAttackSequence
{
    public List<EnemyCharacterAction> actionSet;
}

[Serializable] 
public class ConditionValueCombo
{
    public EnemyAttackCondition condition;
    public float value;
}

[Serializable]
public enum EnemyAttackCondition
{
    HealthLowerThan,
    HealthGreaterThan,
    TurnCountEqualTo,
    TurnCountLessThan,
    TurnCountGreaterThan,
    PlayerHasHealed,
    PlayerHasNotHealed
}

[Serializable]
public class PlayerCharacterAction
{
    public float value;
    public PlayerActionType type;
}

[Serializable]
public class EnemyCharacterAction
{
    public float value;
    public EnemyActionType type;
}

[Serializable]
public enum PlayerActionType
{
    Damage,
    BigAttack,
    Heal,
    HealSelf,
    HealNearby,
    DamageSelf,
    Guard,
    Order,
    Bleed,
    Empower,
    Inspire,
    //Sometimes We'll Need Specific IDs if, for example, the 3FP cast does something different. 
}

public enum EnemyActionType
{
    Focus,
    BleedRandom,
    BleedAll,
    DamageRandom,
    DamageAll,
    Guard,
    Heal
}



