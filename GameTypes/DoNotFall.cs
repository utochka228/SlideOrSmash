using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotFall : GameTypeManager
{
    public static DoNotFall DF;
    [SerializeField]
    private float timeToMatchEnd = 20f;
    private float rememberTime = 5f;
    private float movingTime = 5f;

    private float timer;

    public int imageCount = 3;

    [SerializeField]
    private Sprite[] images;

    private int saveNumber;

    private List<Vector2> saveCells = new List<Vector2>();

    private Camera cam;

    void OnEnable()
    {
        DF = this;
        cam = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateGameMap(mapSize);

        timer = timeToMatchEnd;
        MenuManager.MG.timer.text = timeToMatchEnd.ToString();

        cam.GetComponent<CameraFitObject>().enabled = true;
        cam.GetComponent<CameraMultiTarget>().enabled = false;
        BoxCollider collider = GameManager.GM.gmField.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(GameManager.GM.MapSize.x, 1f, GameManager.GM.MapSize.y);
        cam.GetComponent<CameraFitObject>().collider = collider;

    }

    // Update is called once per frame
    void Update()
    {
        if(matchStarted)
            CountDown();
#if UNITY_ANDROID

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = cam.ScreenPointToRay(touch.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000f, ~(1 << 2)))
            {
                ChangeMaterial change = hit.transform.GetComponent<ChangeMaterial>();
                if(change != null)
                {
                    GameManager.GM.players[0].SetStopPoint(new Vector2(hit.transform.position.x, hit.transform.position.z));
                }
            }
        }
#endif
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000f, ~(1 << 2)))
            {
                ChangeMaterial change = hit.transform.GetComponent<ChangeMaterial>();
                if (change != null)
                {
                    GameManager.GM.players[0].SetStopPoint(new Vector2(hit.transform.position.x, hit.transform.position.z));
                }
            }
        }
#endif
    }

    public override void ShowMatchUI()
    {
        base.ShowMatchUI();
        MenuManager.MG.doNotFallPanel.SetActive(true);
    }

    public override void OnMatchStarted()
    {
        base.OnMatchStarted();
        StartCoroutine(StartMatchLoop());
        
    }

    IEnumerator StartMatchLoop()
    {
        ChangeCellsImage();
        FlipCells(true);
        //Wait for remember
        yield return new WaitForSeconds(rememberTime);
        //Flip(hide) cells
        FlipCells(false);
        //Wait for randomizing image
        MenuManager.MG.doNotFallImage.gameObject.SetActive(true);
        //Get player move Time
        yield return new WaitForSeconds(movingTime);
        //Flip(show) cells
        FlipCells(true);

        if (!saveCells.Contains(GameManager.GM.players[0].currentPosition))
            MenuManager.MG.EndMatch();
        else
            StartCoroutine(StartMatchLoop());
    }

    void FlipCells(bool show)
    {
        if (show)
        {
            for (int y = 0; y < GameManager.GM.MapSize.y; y++)
            {
                for (int x = 0; x < GameManager.GM.MapSize.x; x++)
                {
                    GameManager.GM.gameField[new Vector2(x, y)].ShowCellImage();
                    GameManager.GM.gameField[new Vector2(x, y)].cellObject.transform.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
        else
        {
            for (int y = 0; y < GameManager.GM.MapSize.y; y++)
            {
                for (int x = 0; x < GameManager.GM.MapSize.x; x++)
                {
                    GameManager.GM.gameField[new Vector2(x, y)].HideCellImage();
                    GameManager.GM.gameField[new Vector2(x, y)].cellObject.transform.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
    }

    void ChangeCellsImage()
    {
        saveCells.Clear();
        saveNumber = Random.Range(0, imageCount);
        MenuManager.MG.doNotFallImage.sprite = images[saveNumber];
        MenuManager.MG.doNotFallImage.gameObject.SetActive(false);

        //Change image to random for each cell
        for (int y = 0; y < GameManager.GM.MapSize.y; y++)
        {
            for (int x = 0; x < GameManager.GM.MapSize.x; x++)
            {
                Vector2 position = new Vector2(x, y);
                int cellImageIndex = GameManager.GM.gameField[position].ChangeImage(imageCount);

                if(cellImageIndex == saveNumber)
                {
                    saveCells.Add(position);
                }
            }
        }
    }

    void CountDown()
    {
        timer -= Time.deltaTime;

        string timerStr = timer.ToString();
        if (timerStr.IndexOf(',') < 0)
            MenuManager.MG.timer.text = timerStr;
        else
            MenuManager.MG.timer.text = timerStr.Substring(0, timerStr.IndexOf(','));

        if (timer <= 0f)
        {
            playerWon = true;
            MenuManager.MG.EndMatch();
        }
    }

    public override void Result()
    {
        cam.GetComponent<CameraFitObject>().enabled = false;
        cam.GetComponent<CameraMultiTarget>().enabled = true;
        cam.GetComponent<CameraFitObject>().collider = null;

        base.Result();
    }
}
