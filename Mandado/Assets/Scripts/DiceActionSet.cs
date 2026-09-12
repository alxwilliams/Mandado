using System;
using System.Collections.Generic;

[Serializable]
public class DiceActionSet
{
    public float maxHealth = 15;
    public List<CharacterAction> rollOneActions;
    public List<CharacterAction> rollTwoActions;
    public List<CharacterAction> rollThreeActions;
    public List<CharacterAction> rollFourActions;
    public List<CharacterAction> rollFiveActions;

    public List<CharacterAction> focusActions;
}

[Serializable]
public class CharacterAction
{
    public float value;
    public ActionType type;
}

[Serializable]
public enum ActionType
{
    Damage,
    BigAttack,
    HealSelf,
    HealNearby,
    DamageSelf,
    Guard,
    Order
    //Sometimes We'll Need Specific IDs if, for example, the 3FP cast does something different. 
}



