using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleAction
{
    public ActionType Type;
    public int Weight; // How likely is the "AI" going to choose this action
    [Header("For Healing, over time healing, and poison")]
    public int Value;
    public int Duration;
    [Header("Buff or Debuff Only")]
    public BonusType Bonus; // Only useful if it is a buffing or debuffing action

    public BattleAction(ActionType type, int value, int duration, int weight)
    {
        Type = type;
        Value = value;
        Duration = duration;
        Weight = weight;
    }
}
