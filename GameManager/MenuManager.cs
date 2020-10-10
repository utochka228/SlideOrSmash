using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public delegate void GameEventHandler();

public class MenuManager : MonoBehaviour
{
    public static MenuManager MG;

    #region PublicVariables
    public event GameEventHandler MatchStarted;
    public event GameEventHandler MatchEnded;
    public TextMeshProUGUI menuMoneyText;
    public TextMeshProUGUI storeMoneyText;
    public GameObject controllers;

    #endregion

    #region PrivateVariables

    [SerializeField]
    private int menuIndex;

    [SerializeField]
    private GameObject[] gamePanels;

    [SerializeField]
    private GameObject activePanel;
    [SerializeField]
    private GameObject menuCanvas;
    public GameObject gameCanvas;
    [SerializeField]
    private GameObject pauseMenuPanel;
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private ParticleSystem starsEffect;
    [SerializeField]
    private GameObject menuCharacter;
    [SerializeField]
    private GameObject gameLoader;
    [SerializeField]
    private GameObject countDownPrefab;

    public Slider expSlider;
    public TextMeshProUGUI lvlText;
    public GameObject congratulations;

    private Vector3 menuCameraEulerAngles;
    private Vector3 menuCameraPos;

    public SavingData savingData = new SavingData();
    public delegate void Saving();
    public event Saving OnSaving;

    #endregion
    #region DoNotFallType
    public GameObject doNotFallPanel;
    public Image doNotFallImage;
    public TextMeshProUGUI timer;

    #endregion
    void Awake()
    {
        MG = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            PlayerInfo.PI.Expirience += 5f;
    }

    void LoadData()
    {
        savingData = JsonUtility.FromJson<SavingData>(PlayerPrefs.GetString("Saves"));
        PlayerInfo.PI.Health = savingData.healthCount;
        PlayerInfo.PI.Money = savingData.money;
    }

    void Start()
    {
        if (PlayerPrefs.HasKey("Saves"))
        {
            LoadData();
        }

        //For start of match
        MatchStarted += GameManager.GM.RandomizeGameType;
        MatchStarted += SpawnGameLoader;
        //MatchStarted += GameManager.GM.RandomizeGameField;
        MatchStarted += GameManager.GM.SpawnPlayer;
        MatchStarted += HideOtherUI;

        //For end of match
        MatchEnded += GameManager.GM.ResultMatch;
        MatchEnded += GameManager.GM.DestroyGameField;
        MatchEnded += GameManager.GM.NullCameraTargets;
        MatchEnded += GameManager.GM.DestroyPlayers;
        MatchEnded += SetOldDataToCamera;
        MatchEnded += ActiveMenuPanel;
        MatchEnded += HideOtherUI;

        //Saving camera menu data
        menuCameraEulerAngles = cameraTransform.eulerAngles;
        menuCameraPos = cameraTransform.position;

        storeMoneyText.text = PlayerInfo.PI.Money.ToString();
        menuMoneyText.text = PlayerInfo.PI.Money.ToString();
    }

    public void ActivatePanel(int index)
    {
        activePanel.SetActive(false);
        activePanel = gamePanels[index];
        gamePanels[index].SetActive(true);

        if(index == menuIndex)
        {
            //Activate
            menuCharacter.SetActive(true);
            starsEffect.Play();
        }
        else
        {
            //Deactivate
            menuCharacter.SetActive(false);
            starsEffect.Stop();
        }
    }

    public void StartMatch()
    {
        if(MatchStarted != null)
        {
            MatchStarted();
        }
    }
    public void EndMatch()
    {
        if(MatchEnded != null)
        {
            MatchEnded();
        }
    }

    void ActiveMenuPanel()
    {
        activePanel.SetActive(false);
        activePanel = gamePanels[menuIndex];
        gamePanels[menuIndex].SetActive(true);
    }

    void HideOtherUI()
    {
        if (menuCanvas.activeSelf)
        {
            menuCanvas.SetActive(false);

            menuCharacter.SetActive(false);
            starsEffect.Stop();

            if(GameLoader.GL != null)
                Destroy(GameLoader.GL.gameObject, 4f);

            return;
        }
        if (gameCanvas.activeSelf)
        {
            Time.timeScale = 1f;
            menuCanvas.SetActive(true);

            if(pauseMenuPanel.activeSelf)
                pauseMenuPanel.SetActive(false);


            menuCharacter.SetActive(true);
            starsEffect.Play();

            gameCanvas.SetActive(false);
            return;
        }
    }

    void SpawnGameLoader()
    {
        Instantiate(gameLoader);
    }

    void SetOldDataToCamera()
    {
        cameraTransform.eulerAngles = menuCameraEulerAngles;
        cameraTransform.position = menuCameraPos;
    }

    public void SpawnCountDown()
    {
        GameObject obj = Instantiate(countDownPrefab, gameCanvas.transform);
        StartCoroutine(WaitSecondsAfterGameLoader(obj));
    }

    IEnumerator WaitSecondsAfterGameLoader(GameObject countDownPrefab)
    {
        yield return new WaitForSecondsRealtime(4f);
        Destroy(countDownPrefab);
        SetTimeScaleToNormal();
        GameTypeManager.GTM.OnMatchStarted();
    }

    public void SetTimeScaleToNormal()
    {
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    void OnApplicationQuit()
    {
        SaveData();
    }

    void SaveData()
    {
        if (OnSaving != null)
            OnSaving();

        savingData.healthCount = PlayerInfo.PI.Health;
        savingData.money = PlayerInfo.PI.Money;
        PlayerPrefs.SetString("Saves", JsonUtility.ToJson(savingData));

    }
}

[Serializable]
public class SavingData
{
    public int healthCount;
    public int money;

    //Stats
    public int[] sliderProgress = new int[4];
    public int[] moneyCost = new int[4];

}
