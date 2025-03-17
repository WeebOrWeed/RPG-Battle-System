using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AgentStatus : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text attackText;
    public TMP_Text defenseText;
    public TMP_Text speedText;
    public TMP_Text healthText;

    public void SetAgentName(string name)
    {
        nameText.text = name;
    }

    public void SetAttack(int attack, int offset)
    {
        attackText.text = (attack + offset).ToString();
    }

    public void SetDefense(int defense, int offset)
    {
        defenseText.text = (defense + offset).ToString();
    }

    public void SetSpeed(int speed, int offset)
    {
        speedText.text = (speed + offset).ToString();
    }

    public void SetHealth(int health)
    {
        healthText.text = health.ToString();
    }
}
