using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private Vector2 targetPoint;

    private Cell startCell;
    private Vector2 startCellPos;

    private List<Cell> openList = new List<Cell>();
    private List<Cell> closedList = new List<Cell>();
    public List<Vector2> path = new List<Vector2>();

    private bool pathFinded;
    private bool findPath;

    public delegate void PathDelegate();
    public event PathDelegate OnPathListed;

    private Vector2 currentPos;
    private Vector2 oldPos;

    // Start is called before the first frame update
    void Start()
    {
        oldPos = currentPos;
    }

    // Update is called once per frame
    void Update()
    {
        currentPos = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        if (currentPos != oldPos)
        {
            startCellPos = currentPos;

            if(GameManager.GM.gameField.ContainsKey(startCellPos))
                startCell = GameManager.GM.gameField[startCellPos];

            oldPos = currentPos;
        }

        if (Input.GetKeyDown(KeyCode.F))
            FindPath();

        if (findPath && pathFinded == false)
        {
            SearchPath();
        }
    }

    public void FindPath()
    {
        ClearPath();
        findPath = true;
        pathFinded = false;
    }

    void SearchPath()
    {
        GetNeighbour(startCell);
    }

    void ClearPath()
    {
        foreach (var cell in openList)
        {
            cell.NullAllPathVariables();
        }
        foreach (var cell in closedList)
        {
            cell.NullAllPathVariables();
        }
        openList.Clear();
        closedList.Clear();
        path.Clear();
    }

    void GetNeighbour(Cell cell)
    {
        for (int j = -1; j < 2; j++)
        {
            for (int k = -1; k < 2; k++)
            {
                Vector2 pos = new Vector2(cell.myPosition.x + k, cell.myPosition.y + j);
                if (pos == cell.myPosition)
                    continue;

                if (GameManager.GM.CheckIsCellAvaiallable(pos))
                {
                    Cell neighbour = GameManager.GM.gameField[pos];

                    if(neighbour.myPosition == targetPoint)
                    {
                        neighbour.cellParent = cell;

                        if ((k == -1 && j == -1) || (k == 1 && j == -1) || (k == -1 && j == 1) || (k == 1 && j == 1))
                            neighbour.cost = cell.cost + 14;
                        else
                            neighbour.cost = cell.cost + 10;

                        ListPath(neighbour);

                        return;
                    }

                    if (closedList.Contains(neighbour))
                        continue;

                    if (openList.Contains(neighbour))
                    {
                        int newCost = 0;
                        if ((k == -1 && j == -1) || (k == 1 && j == -1) || (k == -1 && j == 1) || (k == 1 && j == 1))
                            newCost = cell.cost + 14;
                        else
                            newCost = cell.cost + 10;

                        if(newCost < neighbour.cost)
                        {
                            neighbour.cellParent = cell;
                            neighbour.cost = newCost;
                        }
                    }
                    else
                    {
                        openList.Add(neighbour);

                        neighbour.cellParent = cell;

                        if ((k == -1 && j == -1) || (k == 1 && j == -1) || (k == -1 && j == 1) || (k == 1 && j == 1))
                            neighbour.cost = cell.cost + 14;
                        else
                            neighbour.cost = cell.cost + 10;

                    }

                    neighbour.manhattanDistance = 10 * CalculateManhattanDistance((int)neighbour.myPosition.x, (int)targetPoint.x, (int)neighbour.myPosition.y, (int)targetPoint.y);

                    neighbour.Weight = neighbour.cost + neighbour.manhattanDistance;

                }
            }
        }

        closedList.Add(cell);
        Debug.Log("ClosedList add cell: " + cell.myPosition);

        //Select cell with min weight
        var minWeightCells = openList.OrderBy(x => x.Weight);
        Cell minWeightCell = null;
        foreach (var minWCell in minWeightCells)
        {
            if (closedList.Contains(minWCell))
                continue;

            minWeightCell = minWCell;
            break;
        }

        if (minWeightCell != null)
            startCell = minWeightCell;
        else
            Debug.Log("MinWeightCell is NULL");

    }

    void ListPath(Cell targetCell)
    {
        Cell iteratorCell = targetCell;
        int closedCount = GetCountOfNodes(iteratorCell);
        path.Add(new Vector2(iteratorCell.myPosition.x, iteratorCell.myPosition.y));

        for (int i = 1; i < closedCount; i++)
        {
            path.Add(new Vector2(iteratorCell.cellParent.myPosition.x, iteratorCell.cellParent.myPosition.y));
            if (iteratorCell.cellParent == null)
                break;
            else
                iteratorCell = iteratorCell.cellParent;
        }
        Debug.Log("PathFinded!");
        pathFinded = true;
        findPath = false;

        if (OnPathListed != null)
            OnPathListed();
    }

    public int GetCountOfNodes(Cell targetCell)
    {
        int count = 1;
        Cell iterator = targetCell;
        for (int i = 0; i < closedList.Count+1; i++)
        {
            iterator = iterator.cellParent;
            if (iterator == null)
                break;

            count++;
        }

        return count;
    }

    public int CalculateManhattanDistance(int x1, int x2, int y1, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }
}
