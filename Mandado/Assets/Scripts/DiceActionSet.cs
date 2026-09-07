using System;
using System.Collections.Generic;

[Serializable]
public class DiceActionSet
{
    public List<Action> _rollOneActions;
    public List<Action> _rollTwoActions;
    public List<Action> _rollThreeActions;
    public List<Action> _rollFourActions;
    public List<Action> _rollFiveActions;

    public List<Action> _focusActions;
}

[Serializable]
public class Action
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



