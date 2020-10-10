using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Survival : MonoBehaviour
{
    private Enemy myEnemy;
    private Transform player;
    private PlayerController playerController;

    private float distanceToPlayer;
    [SerializeField]
    private float distanceToDetecting = 8f;
    [SerializeField]
    private float distanceToAttacking = 2f;
    // Start is called before the first frame update
    void Start()
    {
        myEnemy = GetComponent<Enemy>();
        myEnemy.OnStatementWasChanged += Attack;
        myEnemy.OnStatementWasChanged += Patroling;
        myEnemy.OnStatementWasChanged += Escaping;
        myEnemy.OnStatementWasChanged += Healing;
        myEnemy.OnStatementWasChanged += Chaising;
        myEnemy.ChangeEnemyStatement(EnemyStatement.Patroling);
        player = myEnemy.target;
        playerController = player.GetComponent<PlayerController>();
    }

    private Vector3 escapingPoint = -Vector3.one;
    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (myEnemy.currentStatement == EnemyStatement.Patroling)
        {
            if (distanceToPlayer <= distanceToDetecting)
            {
                escapingPoint = transform.position;
                if (myEnemy.isLowHp())
                    myEnemy.ChangeEnemyStatement(EnemyStatement.Escaping);
                else
                    myEnemy.ChangeEnemyStatement(EnemyStatement.Chaising);
            }

            myEnemy.CheckOnPatrolingTargetAchieved();
        }
        if (myEnemy.currentStatement == EnemyStatement.Escaping)
        {
            if (distanceToPlayer > distanceToDetecting*2f)
            {
                if (myEnemy.isLowHp())
                    myEnemy.ChangeEnemyStatement(EnemyStatement.Healing);
                else
                    myEnemy.ChangeEnemyStatement(EnemyStatement.Patroling);
            }
        }
        if (myEnemy.currentStatement == EnemyStatement.Chaising)
        {
            if (transform.position == new Vector3(player.position.x, transform.position.y, player.position.z))
            {
                if (myEnemy.isLowHp())
                {
                    playerController.OnChangedPlayerPosition -= myEnemy.Chaising;
                    myEnemy.ChangeEnemyStatement(EnemyStatement.Escaping);
                }
                else
                {
                    playerController.OnChangedPlayerPosition -= myEnemy.Chaising;
                    myEnemy.ChangeEnemyStatement(EnemyStatement.Attacking);
                }
            }
        }
        if (myEnemy.currentStatement == EnemyStatement.Healing)
        {
            if (!myEnemy.isLowHp())
                myEnemy.ChangeEnemyStatement(EnemyStatement.Patroling);
            //Else finding healing
        }
    }

    public void Attack(EnemyStatement oldState, EnemyStatement newState)
    {
        if (newState != EnemyStatement.Attacking)
            return;

        //Do attack actions
        EnemyStatement newStatement = myEnemy.Attack();

        myEnemy.ChangeEnemyStatement(newStatement);
    }
    public void Patroling(EnemyStatement oldState, EnemyStatement newState)
    {
        if (newState != EnemyStatement.Patroling)
            return;

        //Do patroling actions
        myEnemy.Patroling();
    }
    public void Chaising(EnemyStatement oldState, EnemyStatement newState)
    {
        if (newState != EnemyStatement.Chaising)
            return;

        playerController.OnChangedPlayerPosition += myEnemy.Chaising;

        //Do Chaising actions
        myEnemy.Chaising();
    }
    public void Escaping(EnemyStatement oldState, EnemyStatement newState)
    {
        if (newState != EnemyStatement.Escaping)
            return;

        //Do Escaping actions
        myEnemy.Escaping(escapingPoint);
    }
    public void Healing(EnemyStatement oldState, EnemyStatement newState)
    {
        if (newState != EnemyStatement.Healing)
            return;

        //Do Healing actions
        myEnemy.Healing();
    }
}
