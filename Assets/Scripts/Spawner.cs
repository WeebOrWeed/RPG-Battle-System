using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private Transform playerSpots;
    private Transform enemySpots;
    private int playerCount;
    private int enemyCount;
    // Start is called before the first frame update
    void Start()
    {
        playerCount = 0;
        enemyCount = 0;
        playerSpots = transform.GetChild(0);
        enemySpots = transform.GetChild(1);
    }

    public void PutPlayer(BattleAgent agent)
    {
        agent.transform.position = playerSpots.GetChild(playerCount).transform.position;
        playerCount++;
    }

    public void PutEnemy(BattleAgent agent)
    {
        agent.transform.position = enemySpots.GetChild(enemyCount).transform.position;
        enemyCount++;
    }
}
