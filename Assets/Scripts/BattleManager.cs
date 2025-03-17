using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Battle Manager to control game flow
public class BattleManager : MonoBehaviour
{
    [Header("Game Setup")]
    [Range(1, 6)]
    public int numberOfPlayers = 1;
    [Range(1, 6)]
    public int numberOfEnemies = 1;

    [Header("Agent Prefabs")]
    public List<BattleAgent> playerPrefabs;
    public List<BattleAgent> enemyPrefabs;
    public Spawner spawner;

    public TMP_Text roundCount;
    [HideInInspector]
    public List<BattleAgent> Players = new List<BattleAgent>();
    [HideInInspector]
    public List<BattleAgent> Enemies = new List<BattleAgent>();
    private List<BattleAgent> actionQueue = new List<BattleAgent>();
    

    void Start()
    {
        SetupBattle();
        StartCoroutine(RunBattle());
    }

    public bool CheckGameState()
    {
        // Declare winner if one side is eliminated
        if (Players.Count == 0)
        {
            roundCount.text = "Enemy wins!";
            StopAllCoroutines();
            return true;
        }
        if (Enemies.Count == 0)
        {
            roundCount.text = "Player wins!";
            StopAllCoroutines();
            return true;
        }
        return false;
    }

    public BattleAgent GetRandomAgentFromTeam(bool playerTeam)
    {
        if (Players.Count == 0 || Enemies.Count == 0) return null;
        if (playerTeam)
        {
            return Players[Random.Range(0, Players.Count)];
        } else
        {
            return Enemies[Random.Range(0, Enemies.Count)];
        }
    }

    void SetupBattle()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            BattleAgent agent = Instantiate(playerPrefabs[Random.Range(0, playerPrefabs.Count - 1)]);
            Players.Add(agent);
            spawner.PutPlayer(agent);
            agent.curHealth = agent.HP;
            agent.realAttack = agent.Attack;
            agent.realDefense = agent.Defense;
            agent.realSpeed = agent.Speed;
        }
        for (int i = 0; i < numberOfEnemies; i++)
        {
            BattleAgent agent = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count - 1)]);
            Enemies.Add(agent);
            spawner.PutEnemy(agent);
            agent.curHealth = agent.HP;
            agent.realAttack = agent.Attack;
            agent.realDefense = agent.Defense;
            agent.realSpeed = agent.Speed;
        }
    }

    IEnumerator RunBattle()
    {
        int turn = 1;
        while (Players.Count > 0 && Enemies.Count > 0)
        {
            // If not a party get decimated, continue the next turn
            roundCount.text = "Round: " + turn;
            // Generate Action Queue
            CreateActionQueue();
            // At beginning, apply buffs, overtime damages, and overtime heals
            ExecuteEffects();
            yield return new WaitForSeconds(1f); // Wait for 1 second for user to see the calculation
            // Execute Turn
            while (actionQueue.Count() > 0)
            {
                yield return new WaitForSeconds(0.2f);
                BattleAgent activeAgent = actionQueue.First();
                actionQueue.RemoveAt(0);
                activeAgent.IsActing = true;
                activeAgent.ApplyTurn();
                yield return new WaitUntil(() => !activeAgent.IsActing); // Wait Until the agent finish the action motion, where we control with animation
            }
            turn++;
        }
    }

    private void ExecuteEffects()
    {
        foreach (BattleAgent agent in actionQueue)
        {
            agent.ApplyEffects();
        }
    }

    private void CreateActionQueue()
    {
        actionQueue = Players.Concat(Enemies).ToList();
        SortActionQueueBySpeed();
    }

    private void SortActionQueueBySpeed()
    {
        actionQueue.Sort((tp1, tp2) => tp2.realSpeed.CompareTo(tp1.realSpeed));
    }

    // Upon agent death, remove it from the agent list and the action list
    public void AgentDeath(BattleAgent agent)
    {
        if (Players.Contains(agent))
        {
            Players.Remove(agent);
        }
        if (Enemies.Contains(agent))
        {
            Enemies.Remove(agent);
        }
        foreach (BattleAgent ba in actionQueue.ToList())
        {
            if (ba == agent)
            {
                actionQueue.Remove(ba);
            }   
        }
        // Execute the agent object
        Destroy(agent.gameObject);
        SortActionQueueBySpeed();
        CheckGameState();
    }
    
}