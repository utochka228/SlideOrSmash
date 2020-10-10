using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IResultMatch
{
    void ResultMatch();
}

public class GameTypeManager : MonoBehaviour, IResultMatch
{
    public static GameTypeManager GTM;

    public string gameTypeName = "DefaultName";
    public int moneyTrophy;
    public float expirience = 5f;

    public delegate void GameKillerHandler(GameObject killer);
    public event GameKillerHandler EnemyWasKilled;

    [SerializeField]
    protected bool playerWon;

    protected bool matchStarted;

    protected int enemiesCount;
    protected int countOfKilledEnemies;

    [SerializeField]
    protected Vector2 mapSize;

    public virtual void OnMatchStarted()
    {
        matchStarted = true;
        Debug.Log("Match started!");
    }

    void Awake()
    {
        GTM = this;
        GameManager.GM.PlayerDied += PlayerLostMatch;
        EnemyWasKilled += AddEnemyDeath;

        ShowMatchUI();
    }

    protected void GenerateGameMap(Vector2 mapSize)
    {
        GameManager.GM.MapSize = mapSize;
        GameManager.GM.GenerateGameField();
    }


    void PlayerLostMatch()
    {
        GameManager.GM.PlayerDied -= PlayerLostMatch;
        MenuManager.MG.EndMatch();
    }

    public void ResultMatch()
    {
        Result();
    }

    public virtual void Result()
    {
        Debug.Log("Resulting match of " + gameTypeName);

        if (playerWon)
        {
            Debug.Log("U Win Match!");
            PlayerInfo.PI.Money += moneyTrophy;
        }
        else
        {
            Debug.Log("U are lost match!");
        }

        PlayerInfo.PI.Expirience += expirience;
        Destroy(gameObject);
    }

    public void CallEnemyKilledEvent(GameObject killer)
    {
        if(EnemyWasKilled != null)
        {
            EnemyWasKilled(killer);
        }
    }

    public virtual void ShowMatchUI()
    {
        MenuManager.MG.doNotFallPanel.SetActive(false);
        MenuManager.MG.controllers.SetActive(false);

    }

    public virtual void SpawnEnemy(GameObject enemy)
    {
        enemiesCount++;
    }

    public virtual void AddEnemyDeath(GameObject murderer)
    {
        countOfKilledEnemies++;
    }
}
