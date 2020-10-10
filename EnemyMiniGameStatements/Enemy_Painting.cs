using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Painting : MonoBehaviour
{
    private Enemy myEnemy;
    // Start is called before the first frame update
    void Start()
    {
        myEnemy = GetComponent<Enemy>();
        myEnemy.OnChangedPosition += Paint;
        myEnemy.ChangeEnemyStatement(EnemyStatement.Patroling);
    }

    void Paint()
    {
        Painting.painting.Paint(myEnemy.oldPosition, false);
    }
}
