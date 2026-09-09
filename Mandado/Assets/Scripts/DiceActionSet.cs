using System;
using System.Collections.Generic;

[Serializable]
public class DiceActionSet
{
    public List<CharacterAction> _rollOneActions;
    public List<CharacterAction> _rollTwoActions;
    public List<CharacterAction> _rollThreeActions;
    public List<CharacterAction> _rollFourActions;
    public List<CharacterAction> _rollFiveActions;

    public List<CharacterAction> _focusActions;
}

[Serializable]
public class CharacterAction
{
    public float _num;
    public ActionType _type;
}

[Serializable]
public enum ActionType
{
    Damage,
    HealSelf,
    HealNearby,
    DamageSelf,
    Guard,
    Order
    //Sometimes We'll Need Specific IDs if, for example, the 3FP cast does something different. 
}



