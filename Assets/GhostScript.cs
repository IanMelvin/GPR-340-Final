using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static RecursiveBackTracking;

public class GhostScript : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] Vector3 state = Vector3.zero;
    [SerializeField] int LOS;

    public Vector2 mazeStartPosition;
    List<PathNode> path = new List<PathNode>();
    PathNode currentNode;
    int count = 0;

    private void OnEnable()
    {
        PuckDibleScript.powerPelletActivated += ActivateFlee;
    }

    private void OnDisable()
    {
        PuckDibleScript.powerPelletActivated -= ActivateFlee;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        state.x = 0;

        PathNode playerMazePos = new PathNode((int)((player.transform.position.x - mazeStartPosition.x) * 2), (int)((player.transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);
        PathNode enemyMazePos = new PathNode((int)((transform.position.x - mazeStartPosition.x) * 2), (int)((transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);

        
        AStarSearch search = new AStarSearch(RecursiveBackTracking.map, enemyMazePos, playerMazePos);
        path = search.endPath;
        
        DebugAstar(search, enemyMazePos, playerMazePos);
        StartCoroutine("Movement");
        //StartCoroutine("TimeBetweenPathUpdates");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    void DebugAstar(AStarSearch search, PathNode start, PathNode goal)
    {
        //Debug.Log(search.endPath.Any());
    }

    IEnumerator Movement()
    {
        while (count < path.Count)
        {
            Vector2 gridPoint = path[count].XY;
            currentNode = path[count];
            gridPoint /= 2;
            transform.position = new Vector2(gridPoint.x + mazeStartPosition.x, gridPoint.y + mazeStartPosition.y);
            count++;
            yield return new WaitForSeconds(.75f);
        }
        
    }
    IEnumerator TimeBetweenPathUpdates()
    {
        while(player.activeSelf)
        {
            yield return new WaitForSeconds(2.0f);
            PathNode playerMazePos = new PathNode((int)((player.transform.position.x - mazeStartPosition.x) * 2), (int)((player.transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);
            PathNode enemyMazePos = new PathNode((int)((transform.position.x - mazeStartPosition.x) * 2), (int)((transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);
            AStarSearch search = new AStarSearch(RecursiveBackTracking.map, enemyMazePos, playerMazePos);
            path = search.endPath;
            count = 0;
            if (path.Contains(currentNode))
            {
                count = path.IndexOf(currentNode);
            }
        }    
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Caught");
            Application.Quit();
        }
    }

    public void ActivateFlee()
    {
        Debug.Log("Flee");
    }
}

public class PathNode
{
    private List<List<TypesOfSpaces>> map;
    private int X;
    private int Y;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode cameFromNode;

    public PathNode(int x, int y, List<List<TypesOfSpaces>> map)
    {
        this.X = x;
        this.Y = y;
        this.map = map;
    }

    public int x
    {
        get { return X; }
        set { X = value; }
    }

    public int y
    {
        get { return Y; }
        set { Y = value; }
    }

    public List<List<TypesOfSpaces>> Map
    {
        get { return map; }
    }

    public override string ToString()
    {
        return x + "," + y;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public Vector2 XY
    {
        get { return new Vector2(x, y); }   
    }

}

public class AStarSearch
{
    List<PathNode> openList;
    List<PathNode> closedList;
    List<PathNode> grid = new List<PathNode>();
    public List<PathNode> endPath = new List<PathNode>();

    List<PathNode> GetNeighbors(PathNode point, List<List<TypesOfSpaces>> map)
    {
        List<PathNode> neighbors = new List<PathNode>();

        //North
        if (point.x < map.Count && point.y - 1 < map[0].Count && point.y - 1 >= 0 && map[(int)point.x][(int)point.y - 1] != TypesOfSpaces.Border)
        {
            neighbors.Add(grid[point.x * map[0].Count + (point.y - 1)]);
        }

        //West
        if (point.x - 1 < map.Count && point.x - 1 >= 0 && point.y < map[0].Count && map[(int)point.x - 1][(int)point.y] != TypesOfSpaces.Border)
        {
            neighbors.Add(grid[(point.x - 1) * map[0].Count + point.y]);
        }

        //east
        if (point.x + 1 < map.Count && point.x >= 0 && point.y < map[0].Count && map[(int)point.x + 1][(int)point.y] != TypesOfSpaces.Border)
        {
            neighbors.Add(grid[(point.x + 1) * map[0].Count + point.y]);
        }

        //south
        if (point.x < map.Count && point.y + 1 < map[0].Count && point.y + 1 >= 0 && map[(int)point.x][(int)point.y + 1] != TypesOfSpaces.Border)
        {
            neighbors.Add(grid[(point.x * map[0].Count + (point.y + 1))]);
        }

        return neighbors;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);

        return xDistance + yDistance;
    }

    private void ResetGrid(PathNode start, PathNode goal)
    {
        for (int x = 0; x < map.Count; x++)
        {
            for (int y = 0; y < map[0].Count; y++)
            {
                if(x == start.x && y == start.y)
                {
                    start.gCost = int.MaxValue;
                    start.CalculateFCost();
                    start.cameFromNode = null;
                    grid.Add(start);
                    continue;
                }
                else if(x == goal.x && y == goal.y)
                {
                    goal.gCost = int.MaxValue;
                    goal.CalculateFCost();
                    goal.cameFromNode = null;
                    grid.Add(goal);
                    continue;
                }
                PathNode pathNode = new PathNode(x, y, map);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
                grid.Add(pathNode);
            }
        }
    }

    public AStarSearch(List<List<TypesOfSpaces>> map, PathNode start, PathNode goal)
    {
        openList = new List<PathNode> { start };
        closedList = new List<PathNode>();

        ResetGrid(start, goal);

        start.gCost = 0;
        start.hCost = CalculateDistanceCost(start, goal);
        start.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if(currentNode == goal)
            {
                CalculatePath(goal);
                break;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach(PathNode neighborNode in GetNeighbors(currentNode, map))
            {
                if (closedList.Contains(neighborNode)) continue;

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);
                if(tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateDistanceCost(neighborNode, goal);
                    neighborNode.CalculateFCost();

                    if(!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }
    }

    PathNode GetLowestFCostNode(List<PathNode> listOfNodes)
    {
        PathNode lowestFCostNode = listOfNodes[0];
        for(int i = 1; i < listOfNodes.Count; i++)
        {
            if (listOfNodes[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = listOfNodes[i];
            }
        }
        return lowestFCostNode;
    }

    void CalculatePath(PathNode goal)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(goal);
        PathNode currentNode = goal;
        while(currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }

        endPath = path;
        endPath.Reverse();
    }
}