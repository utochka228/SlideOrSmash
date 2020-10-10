using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearingEnemies : GameTypeManager
{
    private int killsGoal;

    // Start is called before the first frame update
    void Start()
    {
        GenerateGameMap(mapSize);
    }

    public override void ShowMatchUI()
    {
        base.ShowMatchUI();
        MenuManager.MG.controllers.SetActive(true);
    }
    public override void AddEnemyDeath(GameObject murderer)
    {
        base.AddEnemyDeath(murderer);
        if(countOfKilledEnemies == killsGoal)
        {
            playerWon = true;
            MenuManager.MG.EndMatch();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
