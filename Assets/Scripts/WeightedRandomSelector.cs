using System;
using System.Collections.Generic;

class WeightedRandomSelector
{
    private List<BattleAction> actions;
    private Random random = new Random();

    public WeightedRandomSelector(List<BattleAction> actions)
    {
        this.actions = actions;
    }

    public BattleAction GetRandomAction()
    {
        int totalWeight = 0;
        foreach (var action in actions)
        {
            totalWeight += action.Weight;
        }

        int randomValue = random.Next(0, totalWeight); // Generate random number within total weight range

        int cumulative = 0;
        for (int i = 0; i < actions.Count; i++)
        {
            cumulative += actions[i].Weight;
            if (randomValue < cumulative)
            {
                return actions[i];
            }
        }

        return default; // Should never reach here
    }
}