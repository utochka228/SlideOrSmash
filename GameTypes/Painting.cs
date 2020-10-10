using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Painting : GameTypeManager
{
    public static Painting painting;

    private int countOfPaintedPlayerCells;
    private int countOfPaintedOtherCells;

    [SerializeField]
    private float timeToResult = 10f;

    [SerializeField]
    private GameObject[] itemsArray;

    [SerializeField]
    private GameObject enemyPrefab;

    private float timer;
    void OnEnable()
    {
        painting = this;
    }

    void Start()
    {
        GenerateGameMap(mapSize);
        SpawnEnemy(enemyPrefab);
        timer = timeToResult;
        if (itemsArray.Length > 0)
            StartCoroutine(GameManager.GM.SpawnItem(itemsArray));

    }

    // Update is called once per frame
    void Update()
    {
        if (matchStarted)
            CountDown();
    }
    void CountDown()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            MenuManager.MG.EndMatch();
        }
    }
    public override void OnMatchStarted()
    {
        base.OnMatchStarted();
    }

    public override void SpawnEnemy(GameObject enemy)
    {
        base.SpawnEnemy(enemy);
        Transform enemyTransform = Instantiate(enemy).transform;
        Vector2 randPos = GameManager.GM.RandomizePosionOnMap((int)mapSize.x, (int)mapSize.y);
        enemyTransform.position = new Vector3(randPos.x, 0f, randPos.y);
        enemyTransform.GetComponent<Enemy_Painting>().enabled = true;
    }

    public void Paint(Vector2 position, bool isPlayerPainted)
    {
        ChangeMaterial change = GameManager.GM.gameField[position].cellObject.GetComponent<ChangeMaterial>();
        GameManager.GM.gameField[position].cellObject.transform.GetComponent<MeshRenderer>().enabled = false;

        if (isPlayerPainted)
        {
            if (!change.cellPaintedByPlayer.activeSelf)
            {
                countOfPaintedPlayerCells++;
                change.ChangeToPlayerColor();
            }
        }
        else
        {
            if (!change.cellPaintedByOther.activeSelf)
            {
                countOfPaintedOtherCells++;
                change.ChangeToOtherColor();
            }
        }
    }

    public override void ShowMatchUI()
    {
        base.ShowMatchUI();
        MenuManager.MG.controllers.SetActive(true);
    }

    public override void Result()
    {
        if (countOfPaintedPlayerCells >= countOfPaintedOtherCells)
            playerWon = true;

        base.Result();
    }

    void OnDestroy()
    {
        GameManager.GM.StopAllCoroutines();
    }
}
