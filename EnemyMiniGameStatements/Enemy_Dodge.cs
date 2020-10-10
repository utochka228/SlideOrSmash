using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Dodge : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    private Vector2 stopPoint; //Позиция к которой стремится игрок
    [SerializeField]
    private int countOfChangingWay = 2;
    private int currentCountOfChangingWay;
    private Vector2 currentPosition;

    // Start is called before the first frame update
    void Start()
    {
        currentCountOfChangingWay = countOfChangingWay;
    }

    // Update is called once per frame
    void Update()
    {
        currentPosition = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        MoveEnemy();
    }


    void MoveEnemy()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(stopPoint.x, transform.position.y, stopPoint.y), step);
    }

    bool FindStopPoint(Vector2 dir)
    {
        Vector2 position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        Cell cell;
        int length = 0;
        if (dir.x == 0)
            length = (int)GameManager.GM.MapSize.y;
        if (dir.y == 0)
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
        if (currentCountOfChangingWay <= 0 && !dirChangedByWorld)
            return;

        if (!dirChangedByWorld)
            transform.position = new Vector3(currentPosition.x, transform.position.y, currentPosition.y);

        if (!FindStopPoint(dir)) return;

        if (!dirChangedByWorld)
            currentCountOfChangingWay--;

    }
}
