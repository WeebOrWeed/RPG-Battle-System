using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    private BattleAgent agent;

    private void Start()
    {
        agent = transform.parent.GetComponent<BattleAgent>();
    }

    public void OnActionComplete()
    {
        agent.IsActing = false;
    }

    public void SetEffects(string effect, bool active)
    {
        // Effects are set directly through active/inactive due to animator difficulties
        switch (effect)
        {
            case "buffed":
                transform.GetChild(0).gameObject.SetActive(active);
                break;
            case "debuffed":
                transform.GetChild(1).gameObject.SetActive(active);
                break;
            case "poisoned":
                transform.GetChild(2).gameObject.SetActive(active);
                break;
            case "healing":
                transform.GetChild(3).gameObject.SetActive(active);
                break;
            default:
                Debug.LogError("Shouldn't be here");
                break;
        }
    }
}
