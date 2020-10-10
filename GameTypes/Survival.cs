using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survival : GameTypeManager
{
    [SerializeField]
    private float timeToSurvive = 60f;

    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private GameObject[] itemsArray;

    private float timer;

    void Start()
    {
        GenerateGameMap(mapSize);
        for (int i = 0; i < 3; i++)
        {
            SpawnEnemy(enemyPrefab);
        }
        timer = timeToSurvive;
        if (itemsArray.Length > 0)
            StartCoroutine(GameManager.GM.SpawnItem(itemsArray));
    }

    //void AddMoneyToPlayer(GameObject murderer)
    //{
    //    PlayerController player = murderer.GetComponent<PlayerController>();
    //    if(player == GameManager.GM.players[0])
    //    {
    //        MenuManager.MG.money += 6;
    //    }
    //}

    public override void AddEnemyDeath(GameObject murderer)
    {
        base.AddEnemyDeath(murderer);
    }

    void Update()
    {
        if(matchStarted)
            CountDown();
    }

    

    void CountDown()
    {
        timer -= Time.deltaTime;

        if(timer <= 0f)
        {
            playerWon = true;
            MenuManager.MG.EndMatch();
        }
    }

    public override void SpawnEnemy(GameObject enemy)
    {
        base.SpawnEnemy(enemy);
        Transform enemyTransform = Instantiate(enemy).transform;
        Vector2 randPos = GameManager.GM.RandomizePosionOnMap((int)mapSize.x, (int)mapSize.y);
        enemyTransform.position = new Vector3(randPos.x, 0.5f, randPos.y);
        enemyTransform.GetComponent<Enemy_Survival>().enabled = true;
    }

    public override void ShowMatchUI()
    {
        base.ShowMatchUI();
        MenuManager.MG.controllers.SetActive(true);
    }

    public override void Result()
    {

        moneyTrophy += countOfKilledEnemies;

        base.Result();
    }
}
