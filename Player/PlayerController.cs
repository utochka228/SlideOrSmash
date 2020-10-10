using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

interface IDamageble
{
    void TakeDamage(GameObject hitter);
    void Die(GameObject murderer);
}
public delegate void ControllerDelegate();
public class PlayerController : MonoBehaviour, IDamageble
{
    #region PublicVariables

    public Rigidbody rb;

    public new string name;

    public float timeForIdle = 1f; //Время для простоя игрока
    public float speed = 10f; //Скорость передвижения игрока

    public int secondsForDie = 3; //Через сколько игрок умрёт
    public int health = 5;
    public int bombCount = 0;
    public int countOfChangingWay = 2; //"Двойной прыжок" - дополнительный прыжок

    public Text deathTimerText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI bombText;

    public bool isFirstPlayer;
    public bool disableInput;

    public PlayerController enemy;

    public GameObject dodgeTrail;
    public GameObject bomb;
    public GameObject floatingBombText;
    public GameObject floatingHealthText;

    public event ControllerDelegate OnChangedPlayerPosition;

    public MeshRenderer meshRenderer;

    public Vector2 currentPosition;
    private Vector2 oldPosition;

    #endregion

    #region PrivateVariables

    private Vector2 stopPoint; //Позиция к которой стремится игрок
    //Timers
    private float idleTimer;
    private float deathTimer;

    private int currentCountOfChangingWay;

    [SerializeField]
    private GameObject[] skins;
    #endregion

    void UpdatePlayerSkin()
    {
        GameObject skin = skins[SkinChanger.skinChanger.currentActiveSkinIndex];
        skin.SetActive(true);
    }

    void PlayerPositionHasChanged()
    {
        if (OnChangedPlayerPosition != null)
            OnChangedPlayerPosition();
    }

    void Start()
    {
        OnChangedPlayerPosition += SetTimersToDefaultValues;
        health = PlayerInfo.PI.Health;
            
        stopPoint = new Vector2(transform.position.x, transform.position.z);
        oldPosition = currentPosition;

        idleTimer = timeForIdle;
        deathTimer = secondsForDie;

        UpdatePlayerSkin();

        //Floating Text Initializing
        foreach (var item in GameManager.GM.floatText)
        {
            FloatingText text = item.GetComponent<FloatingText>();
            if (text.isFirstPlayer.Equals(isFirstPlayer))
            {
                if(text.type == FloatingTextType.Bomb)
                    floatingBombText = item;
                if (text.type == FloatingTextType.Health)
                    floatingHealthText = item;
            }
        }
        //////////
    }

    void Update()
    {
        currentPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        if (currentPosition != oldPosition)
        {
            PlayerPositionHasChanged();
            if (GameManager.GM.gameType == GameType.Dodge)
            {
                Dodge.dodge.SpawnDodgeWall(oldPosition);
            }
            if(GameManager.GM.gameType == GameType.Painting)
            {
                Painting.painting.Paint(oldPosition, true);
                oldPosition = currentPosition;
            }
            oldPosition = currentPosition;
        }

        //Спавним бомбму для
        if (!isFirstPlayer) //красного игрока
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Die(gameObject);
        }
        else //зелёного игрока
        {
            if (Input.GetKeyDown(KeyCode.RightControl))
                Die(gameObject);
        }

        if (currentPosition == stopPoint)
        {
            disableInput = false;
            currentCountOfChangingWay = countOfChangingWay;
        }

        MovePlayer();

        CountForDeath();

        //////////////////UI//////////////////////
        healthText.text = health.ToString();
        bombText.text = bombCount.ToString();

    }

    public void SetStopPoint(Vector2 position)
    {
        stopPoint = position;
    }

    void MovePlayer()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(stopPoint.x, transform.position.y, stopPoint.y), step);
    }

    public void SpawnBomb()
    {
        if(bombCount > 0)
        {
            GameObject _bomb = Instantiate(bomb);
            _bomb.GetComponent<Bomb>().owner = gameObject;
            _bomb.transform.position = transform.position;
            bombCount--;
        }
    }

    bool FindStopPoint(Vector2 dir)
    {
        Vector2 position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        Cell cell;
        int length = 0;
        if (dir.x == 0)
            length = (int)GameManager.GM.MapSize.y;
        if(dir.y == 0)
            length = (int)GameManager.GM.MapSize.x;

        for (int i = 0; i < length; i++)
        {
            position += dir;
            if (!GameManager.GM.gameField.TryGetValue(position, out cell))
            {
                if (i == 0)
                {
                    return false;
                }

                stopPoint = position - dir;

                return true;
            }
        }

        return false;
    }

    public void MovePlayerTo(Vector2 dir, bool dirChangedByWorld)
    {
        if (disableInput)
            return;

        if (currentCountOfChangingWay <= 0 && !dirChangedByWorld)
            return;

        if (!dirChangedByWorld)
            transform.position = new Vector3(currentPosition.x, transform.position.y, currentPosition.y);

        if (!FindStopPoint(dir)) return;

        if(!dirChangedByWorld)
            currentCountOfChangingWay--;

    }


    void CountForDeath() //Дефолтный таймер
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0)
        {
            deathTimerText.gameObject.SetActive(true);
            deathTimer -= Time.deltaTime;
            string timerStr = deathTimer.ToString();

            if (timerStr.IndexOf(',') < 0)
                deathTimerText.text = timerStr;
            else deathTimerText.text = timerStr.Substring(0, timerStr.IndexOf(','));

            if (deathTimer <= 0)
                Die(gameObject);
        }
        else deathTimerText.gameObject.SetActive(false);
    }

    void SetTimersToDefaultValues()
    {
        idleTimer = timeForIdle;
        deathTimer = secondsForDie;
    }

    public void Die(GameObject murderer)
    {
        Debug.Log(transform.name + " was killed by " + murderer.name);
        //GameManager.GM.WinBattle(enemy.transform);
        GameManager.GM.CallPlayerDieEvent();
        Destroy(gameObject);
    }

    public void TakeDamage(GameObject hitter)
    {
        health--;
        if (health == 0)
            Die(hitter);
    }

    void OnTriggerEnter(Collider other)
    {
        IItem item = other.GetComponent<IItem>();
        if(item != null)
        {
            item.Use(this);
        }
    }
    int floatingTextValue = 0;
    public void ActivateFloatingText(int value, GameObject floatingText)
    {
        if (!floatingText.activeSelf)
        {
            floatingTextValue = 0;
            floatingText.SetActive(true);
            floatingText.GetComponent<Text>().text = "+" + value;
            floatingTextValue += value;
        }
        else
        {
            floatingText.GetComponent<FloatingText>().NullTimer();
            floatingTextValue += value;
            floatingText.GetComponent<Text>().text = "+" + floatingTextValue;
        }
    }
}
