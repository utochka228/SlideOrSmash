using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class Cell
{
    public GameObject cellObject;
    public ChangeMaterial change;

    public Vector2 myPosition;
    public Cell cellParent;

    public int cost;
    public int manhattanDistance;
    private int weight;

    public int Weight
    {
        get
        {
            return weight;
        }
        set
        {
            if(value >= 0)
            {
                weight = value;
                //change.weight.text = weight.ToString();
            }
        }
    }
    public Cell(GameObject cell, Vector2 myPos)
    {
        cellObject = cell;
        myPosition = myPos;
        change = cellObject.GetComponent<ChangeMaterial>();
    }

    public void NullAllPathVariables()
    {
        cellParent = null;
        cost = 0;
        manhattanDistance = 0;
        Weight = 0;
    }


    #region DoNotFallGameType

    private int myCellImage = -1;
    public int ChangeImage(int imageCount)
    {
        myCellImage = Random.Range(0, imageCount);
        //Change image to index

        return myCellImage;
    }

    public void ShowCellImage()
    {
        change.ShowCellImage(myCellImage);
    }
    public void HideCellImage()
    {
        change.HideCellImage(myCellImage);
    }
    #endregion
}

public enum GameType { Survival, ClearingEnemies, Dodge, Painting, DoNotFall, Gathering}

public class GameManager : MonoBehaviour
{
    public static GameManager GM; //Singleton
    #region PublicVars

    public GameType gameType;
    [SerializeField]
    private GameObject[] gameTypePrefabs;

    IResultMatch resultatorMatch;


    //GameLoader
    public Color[] backColors;
    public Sprite[] gameLoaderSprites;
    public string[] gameLoaderTipTexts;
    ///

    public bool singleMatch = true;

    public GameObject[] floatText;

    public TextMeshProUGUI healthFirstPlayer;
    public TextMeshProUGUI healthSecondPlayer;
    public TextMeshProUGUI bombFirstPlayer;
    public TextMeshProUGUI bombSecondPlayer;
    public TextMeshProUGUI playerNickNameFirst;
    public TextMeshProUGUI playerNickNameSecond;
    public TextMeshProUGUI playerNickNameWin;

    public TMP_InputField inputFieldFirst;
    public TMP_InputField inputFieldSecond;

    public Dictionary<Vector2, Cell> gameField = new Dictionary<Vector2, Cell>();
    public Grid grid;

    private Vector2 mapSize;
    public Vector2 MapSize
    {
        get
        {
            return mapSize;
        }
        set
        {
            mapSize = value;
        }
    }

    public GameObject gmField;
    public event GameEventHandler PlayerDied;
    #endregion

    #region PrivateVariables


    [SerializeField]
    private float spawningTimeIdle = 5f;
    [SerializeField]
    private float scale = 1f;
    [SerializeField]
    private float xOffset = 100f;
    [SerializeField]
    private float yOffset = 100f;
    [SerializeField]
    private float[] chances;
    [SerializeField]
    private LayerMask layer;
    [SerializeField]
    private CameraMultiTarget multiTarget;

    [SerializeField]
    private Material[] playersMaterial;

    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private GameObject crestPrefab;
    [SerializeField]
    private GameObject[] itemsArray;
    [SerializeField]
    private GameObject winTable;
    [SerializeField]
    private GameObject fieldCell;
    [SerializeField]
    private GameObject secondPlayerController;
    [SerializeField]
    private GameObject secondPlayerHUD;
    [SerializeField]
    private GameObject pauseMenu;
    


    #endregion

    void Awake()
    {
        GM = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Transform enemy = Instantiate(enemyPrefab).transform;
            Vector2 randPos = RandomizePosionOnMap((int)MapSize.x, (int)MapSize.y);
            enemy.position = new Vector3(randPos.x, 0f, randPos.y);
        }
    }

    private Vector2 randomPos;

    public Vector2 RandomizePosionOnMap(int mapSizeX, int mapSizeY)
    {
        StartCoroutine(RandomizePosition(mapSizeX, mapSizeY));
        return randomPos;
    }

    IEnumerator RandomizePosition(int mapSizeX, int mapSizeY)
    {
        bool positionNotFound = true;
        Vector2 position = -Vector2.one;
        while (positionNotFound)
        {
            position = new Vector2(Random.Range(0, mapSizeX), Random.Range(0, mapSizeY));

            if (GameManager.GM.CheckIsCellAvaiallable(position))
            {
                positionNotFound = false;
            }
            yield return null;
        }

        randomPos = position;
    }

    //Спавн вещей для взаимодействия
    public IEnumerator SpawnItem(GameObject[] spawingItemsList)
    {
        yield return new WaitForSeconds(spawningTimeIdle);

        StartCoroutine(RandomizeSpawnPosition(spawingItemsList));

        StartCoroutine(SpawnItem(spawingItemsList));
    }

    public void StopCoroutines()
    {
        Debug.Log("Stopping all coroutines");
        StopAllCoroutines();
    }

    IEnumerator RandomizeSpawnPosition(GameObject[] spawingItemsList)
    {
        bool positionNotFound = true;
        Vector2 position = -Vector2.one;
        while (positionNotFound)
        {
            position = new Vector2(Random.Range(0, (int)MapSize.x), Random.Range(0, (int)MapSize.y));

            if (GameManager.GM.CheckIsCellAvaiallable(position))
            {
                positionNotFound = false;
            }
            yield return null;
        }

        GameObject go = Instantiate(spawingItemsList[(int)Choose(spawingItemsList)]);
        go.transform.position = new Vector3(position.x, go.GetComponent<Item>().spawningOffsetY, position.y);
        go.transform.SetParent(gmField.transform);
    }

    float Choose(GameObject[] items)
    {

        float total = 0;

        foreach (var elem in items)
        {
            total += elem.GetComponent<Item>().spawnChance;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < items.Length; i++)
        {
            if (randomPoint < items[i].GetComponent<Item>().spawnChance)
            {
                return i;
            }
            else
            {
                randomPoint -= items[i].GetComponent<Item>().spawnChance;
            }
        }
        return items.Length - 1;
    }

    public void ResultMatch()
    {
        resultatorMatch.ResultMatch();
    }

    //Генерация игрового поля 
    public void GenerateGameField()
    {
        gmField = new GameObject("GameField");
        gmField.layer = 2;
        gmField.isStatic = true;
        gmField.transform.position = new Vector3((MapSize.x - 1) / 2, 0f, (MapSize.y - 1) / 2);

        //bool[,] _map = new bool[width, height];
        bool[,] map = GenerateMap();

        for (int y = 0; y < MapSize.y; y++)
        {
            for (int x = 0; x < MapSize.x; x++)
            {
                Vector2 position = new Vector2(x, y);
                //if (map[x, y])
                //{

                    //Add cells
                    GameObject quad = Instantiate(fieldCell);


                    quad.transform.position = new Vector3(position.x, 0, position.y);
                    gameField.Add(position, new Cell(quad, position));

                    quad.transform.SetParent(gmField.transform);

                //}
            }
        }
        ////Temp
        bool[,] temp = new bool[(int)MapSize.x, (int)MapSize.y];
        for (int y = 0; y < (int)MapSize.y; y++)
        {
            for (int x = 0; x < (int)MapSize.x; x++)
            {
                temp[x, y] = true;
            }
        }
        grid.CreateGrid(temp, MapSize);
    }

    public void NullCameraTargets()
    {
        multiTarget.NullTargets();
    }

    public void DestroyGameField()
    {
        gameField.Clear();
        Destroy(gmField);
    }
    public void DestroyPlayers()
    {
        foreach (var player in players)
        {
            if(player != null)
                Destroy(player.gameObject);
        }
    }

    private GameObject gameTypeObj;
    public void RandomizeGameType()
    {
        int randomNumber = Random.Range(0, Enum.GetNames(typeof(GameType)).Length);
        //gameTypePrefabs[randomNumber] VMESTO gameTypePrefabs[0]
        gameTypeObj = Instantiate(gameTypePrefabs[0]);
        resultatorMatch = gameTypeObj.GetComponent<IResultMatch>();

        //gameType = (GameType)randomNumber;
        gameType = GameType.Survival;

    }

    public bool CheckIsCellAvaiallable(Vector2 cellPos)
    {
        Cell cell;

        return gameField.TryGetValue(cellPos, out cell);
    }

    void RemoveCell(Vector2 cellPos)
    {
        Destroy(gameField[cellPos].cellObject);
        gameField.Remove(cellPos);
    }

    //Спавн игроков и инициализация переменных
    public PlayerController[] players;
    public void SpawnPlayer()
    {
        if (singleMatch)
        {
            SetSecondPlayerHUD(false);

            players = new PlayerController[1];

            GameObject player = Instantiate(playerPrefab);
            player.transform.position = new Vector3(19, 0.5f, 0);
            players[0] = player.GetComponent<PlayerController>();

            GameObject[] playerTarget = new GameObject[1];
            playerTarget[0] = player;
            multiTarget.SetTargets(playerTarget);

            players[0].healthText = healthFirstPlayer;
            players[0].bombText = bombFirstPlayer;
        }
        else
        {
            SetSecondPlayerHUD(true);

            players = new PlayerController[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject player = Instantiate(playerPrefab);
            
                player.transform.position = new Vector3(0, 0.5f, 0);
                players[i] = player.GetComponent<PlayerController>();

                if (i == 0)
                {
                    player.GetComponent<PlayerController>().isFirstPlayer = true;
                    player.GetComponent<PlayerController>().meshRenderer.material = playersMaterial[0];
                }
            }
            GameObject[] playersGO = new GameObject[2];
            playersGO[0] = players[0].transform.gameObject;
            playersGO[1] = players[1].transform.gameObject;
            multiTarget.SetTargets(playersGO);

            if (players[0].isFirstPlayer)
            {
                players[0].transform.name = playerNickNameFirst.text;
                players[0].healthText = healthFirstPlayer;
                players[0].bombText = bombFirstPlayer;
            }
            if (!players[1].isFirstPlayer)
            {
                players[1].transform.name = playerNickNameSecond.text;
                players[1].transform.position = new Vector3(MapSize.x - 1, 0.5f, MapSize.y - 1);
                players[1].healthText = healthSecondPlayer;
                players[1].bombText = bombSecondPlayer;
            }

            players[0].enemy = players[1];
            players[1].enemy = players[0];
        
        }
    }

    public void WinBattle(Transform player)
    {
        winTable.SetActive(true);
        playerNickNameWin.text = player.name + " победил!";
    }


    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);

    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }
    public void ChangedName(int player)
    {
        if(player == 0)
            playerNickNameFirst.text = inputFieldFirst.text;
        if(player == 1)
            playerNickNameSecond.text = inputFieldSecond.text;
    }

    public void CallPlayerDieEvent()
    {
        if(PlayerDied != null)
        {
            PlayerDied();
        }
    }

    public void SingleMatch()
    {
        singleMatch = true;
    }

    public void NonSingleMatch()
    {
        singleMatch = false;
    }

    void SetSecondPlayerHUD(bool activate)
    {
        secondPlayerController.SetActive(activate);
        secondPlayerHUD.SetActive(activate);
    }

    //Map Generation

    [SerializeField]
    int deathLimit;
    [SerializeField]
    int birthLimit;

    [SerializeField]
    float chanceToStartAlive = 0.45f;

    [SerializeField]
    int numberOfSteps;

    public bool[,] GenerateMap()
    {
        //Create a new map
        bool[,] cellmap = new bool[(int)MapSize.x, (int)MapSize.y];
        //Set up the map with random values
        cellmap = initialiseMap(cellmap);
        //And now run the simulation for a set number of steps
        for (int i = 0; i < numberOfSteps; i++)
        {
            cellmap = doSimulationStep(cellmap);
        }

        return cellmap;
    }

    public bool[,] doSimulationStep(bool[,] oldMap)
    {
        bool[,] newMap = new bool[(int)MapSize.x, (int)MapSize.y];
        //Loop over each row and column of the map
        for (int x = 0; x < oldMap.GetLength(0)-1; x++)
        {
            for (int y = 0; y < oldMap.GetLength(1)-1; y++)
            {
                int nbs = countAliveNeighbours(oldMap, x, y);
                //The new value is based on our simulation rules
                //First, if a cell is alive but has too few neighbours, kill it.
                if (oldMap[x, y])
                {
                    if (nbs < deathLimit)
                    {
                        newMap[x, y] = false;
                    }
                    else
                    {
                        newMap[x, y] = true;
                    }
                } //Otherwise, if the cell is dead now, check if it has the right number of neighbours to be 'born'
                else
                {
                    if (nbs > birthLimit)
                    {
                        newMap[x, y] = true;
                    }
                    else
                    {
                        newMap[x, y] = false;
                    }
                }
            }
        }
        return newMap;
    }
    public bool[,] initialiseMap(bool[,] map)
    {
        for (int x = 0; x < (int)MapSize.x; x++)
        {
            for (int y = 0; y < (int)MapSize.y; y++)
            {
                if (Random.Range(0, 1f) < chanceToStartAlive)
                {
                    map[x, y] = true;
                }
            }
        }
        return map;
    }

    //Returns the number of cells in a ring around (x,y) that are alive.
    public int countAliveNeighbours(bool[,] map, int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int neighbour_x = x + i;
                int neighbour_y = y + j;
                //If we're looking at the middle point
                if (i == 0 && j == 0)
                {
                    //Do nothing, we don't want to add ourselves in!
                    continue;
                }
                //In case the index we're looking at it off the edge of the map
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= map.Length || neighbour_y >= map.GetLength(0))
                {
                    count = count + 1;
                }
                //Otherwise, a normal check of the neighbour
                else if (map[neighbour_x, neighbour_y])
                {
                    count = count + 1;
                }
            }
        }

        return count;
    }

}
