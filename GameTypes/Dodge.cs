using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dodge : GameTypeManager
{
    public static Dodge dodge; 
    public List<GameObject> gamePlayers = new List<GameObject>();

    [SerializeField]
    private GameObject dodgeWall;
    [SerializeField]
    private float dodgeWallDeathTime;
    [SerializeField]
    private float spawningWallDelay = 1f;

    // Start is called before the first frame update
    void OnEnable()
    {
        dodge = this;
    }
    GameObject[] trails;
    void Start()
    {
        GenerateGameMap(mapSize);
        gamePlayers.AddRange(GameManager.GM.players.Select(x => x.gameObject));


        foreach (var gamePlayer in gamePlayers)
        {
            PlayerController pc = gamePlayer.GetComponent<PlayerController>();
            Enemy enemyController = gamePlayer.GetComponent<Enemy>();
            pc?.dodgeTrail.SetActive(true);
            enemyController?.dodgeTrail.SetActive(true);
        }

        trails = GameObject.FindGameObjectsWithTag("DodgeTrail");

        foreach (var trail in trails)
        {
            trail.GetComponent<TrailRenderer>().time = dodgeWallDeathTime + 0.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ShowMatchUI()
    {
        base.ShowMatchUI();
        MenuManager.MG.controllers.SetActive(true);
    }

    public void SpawnDodgeWall(Vector2 position)
    {
        StartCoroutine(SpawnWall(position));
        
    }

    public override void SpawnEnemy(GameObject enemy)
    {
        base.SpawnEnemy(enemy);
    }

    IEnumerator SpawnWall(Vector2 position)
    {
        yield return new WaitForSeconds(spawningWallDelay);

        Transform wall = Instantiate(dodgeWall).transform;
        wall.position = new Vector3(position.x, 0, position.y);
        wall.SetParent(transform);
        wall.GetComponent<DodgeWall>().deathTime = dodgeWallDeathTime;
    }
}
