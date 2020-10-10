using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void StatementDelegate(EnemyStatement oldStatement, EnemyStatement newStatement);

public enum EnemyStatement { Patroling, Attacking, Escaping, Chaising, Healing}
public class Enemy : MonoBehaviour, IDamageble
{
    public int health = 3;
    [SerializeField]
    private int lowHpValue = 1;

    public EnemyStatement currentStatement { get; private set; }

    public Vector2 currentPosition { get; private set; }
    public Vector2 oldPosition { get; private set; }

    [SerializeField]
    private Unit movementScript;

    [SerializeField]
    public GameObject dodgeTrail { get; private set; }

    public Transform target;

    private bool canAttackPlayer = true;



    public event ControllerDelegate OnChangedPosition;
    public event StatementDelegate OnStatementWasChanged;

    public void Die(GameObject murderer)
    {
        GameTypeManager.GTM.CallEnemyKilledEvent(murderer);
        Destroy(gameObject);
    }

    public void TakeDamage(GameObject hitter)
    {
        health--;
        if (health == 0)
            Die(hitter);
    }

    void PositionHasChanged()
    {
        if (OnChangedPosition != null)
            OnChangedPosition();
    }

    void Start()
    {
        MenuManager.MG.MatchEnded += DestroyGameObject;
        movementScript.OnPathNotSuccessful += Patroling;

        oldPosition = currentPosition;
        target = GameManager.GM.players[0].transform;

    }

    void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        MenuManager.MG.MatchEnded -= DestroyGameObject;
    }


    // Update is called once per frame
    void Update()
    {
        currentPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        if(Input.GetKeyDown(KeyCode.F))
            Patroling(); 

        if (currentPosition != oldPosition)
        {
            PositionHasChanged();

            oldPosition = currentPosition;
        }

        

    }

    public void ChangeEnemyStatement(EnemyStatement newStatement)
    {
        EnemyStatement oldStatement = currentStatement;
        currentStatement = newStatement;
        Debug.Log("Enemy statement was changed: " + oldStatement + "->" + newStatement);
        if (OnStatementWasChanged != null)
            OnStatementWasChanged(oldStatement, newStatement);
    }

    public void MoveToTarget(Vector2 myPos, Transform target)
    {
        movementScript.MoveToTarget(myPos, target);
    }


    IEnumerator Cor_RandomizeTargetPoint()
    {
        bool positionNotFound = true;
        Vector2 position = -Vector2.one;
        Vector2 _curPos = currentPosition;
        List<Vector2> deadZone = new List<Vector2>()
        {
            new Vector2(_curPos.x, _curPos.y),
            new Vector2(_curPos.x+1, _curPos.y),
            new Vector2(_curPos.x-1, _curPos.y),
            new Vector2(_curPos.x, _curPos.y+1),
            new Vector2(_curPos.x, _curPos.y-1),
            new Vector2(_curPos.x+1, _curPos.y+1),
            new Vector2(_curPos.x-1, _curPos.y-1),
            new Vector2(_curPos.x+1, _curPos.y-1),
            new Vector2(_curPos.x-1, _curPos.y+1),
        };
        while (positionNotFound)
        {
            position = new Vector2(Random.Range(0, (int)GameManager.GM.MapSize.x), Random.Range(0, (int)GameManager.GM.MapSize.y));

            if (deadZone.Contains(position))
                continue;

            if (GameManager.GM.CheckIsCellAvaiallable(position))
            {
                positionNotFound = false;
            }
            yield return null;
        }
        targetPos = position;
        GameObject _target = new GameObject();
        _target.transform.position = new Vector3(position.x, 0f, position.y);


        MoveToTarget(_curPos, _target.transform);
    }
    private Vector2 targetPos;

    private bool m_coroutineStartedFlag = false;
    public void CheckOnPatrolingTargetAchieved()
    {
        if (movementScript.target == null)
            return;

        if (transform.position == new Vector3(targetPos.x, transform.position.y, targetPos.y) /*&& !movementScript.isMoving*/)
        {
            if(!m_coroutineStartedFlag)
                StartCoroutine(WaitAndRandomize());
        }
    }
    [SerializeField]
    private float enemyWaitingTime = 1f;

    IEnumerator WaitAndRandomize()
    {
        m_coroutineStartedFlag = true;
        yield return new WaitForSeconds(enemyWaitingTime);
        Patroling();
        m_coroutineStartedFlag = false;
    }

    public bool isLowHp()
    {
        bool result = health <= lowHpValue ? true : false;
        return result;
    }

    #region VirtualEnemyMethods
    //Методы для переопределения новым существом
    public virtual EnemyStatement Attack()
    {
        if (canAttackPlayer)
            Debug.Log("Attacking player!");

        return EnemyStatement.Escaping;
    }

    public virtual void Patroling()
    {
        StartCoroutine(Cor_RandomizeTargetPoint());
    }
    public virtual void Chaising()
    {
        Debug.Log("Chaising!");

        MoveToTarget(currentPosition, target.transform);

    }
    public virtual void Escaping(Vector3 escapingPoint)
    {
        Debug.Log("Escaping!");
        GameObject obj = new GameObject();
        Vector3 dirToPlayer = Vector3.zero;

        if (escapingPoint == -Vector3.one)
            dirToPlayer = transform.position - target.position;
        else
            dirToPlayer = escapingPoint - target.position;

        Vector3 newPos = transform.position + dirToPlayer;
        int xPos = Mathf.RoundToInt(newPos.x);
        xPos = Mathf.Clamp(Mathf.RoundToInt(newPos.x), 0, (int)GameManager.GM.MapSize.x);
        int zPos = Mathf.RoundToInt(newPos.y);
        zPos = Mathf.Clamp(Mathf.RoundToInt(newPos.z), 0, (int)GameManager.GM.MapSize.y);
        newPos = new Vector3(xPos, 0f, zPos);
        Debug.Log("New escape pos: " + newPos);
        obj.transform.position = newPos;
        MoveToTarget(currentPosition, obj.transform);
    }
    public virtual void Healing()
    {
        Debug.Log("Healing!");
    }
    #endregion
}
