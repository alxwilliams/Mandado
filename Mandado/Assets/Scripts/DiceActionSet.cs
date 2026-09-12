using System;
using System.Collections.Generic;

[Serializable]
public class DiceActionSet
{
    public List<CharacterAction> rollOneActions;
    public List<CharacterAction> rollTwoActions;
    public List<CharacterAction> rollThreeActions;
    public List<CharacterAction> rollFourActions;
    public List<CharacterAction> rollFiveActions;

    public List<CharacterAction> focusActions;
}

[Serializable]
public class CompoundDiceActionSet
{
    public int diceRollSpan = 1;
    public DiceActionSet _diceActionSet;
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



