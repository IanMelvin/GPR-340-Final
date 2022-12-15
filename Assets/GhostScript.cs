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
    [SerializeField] Color fleeColor;
    [SerializeField] Color[] baseColors;
    [SerializeField] public int colorIndex;
    [SerializeField] int scoreValue = 200;

    public int timeBeforeStart = 0;

    public Vector2 mazeStartPosition;
    public Vector2 ghostStartPosition;
    List<PathNode> path = new List<PathNode>();
    PathNode currentNode;
    int count = 0;
    bool activateFleeState = false;
    bool stillFleeing = false;

    public static Action<int> ghostAddToScore;

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
        GetComponent<SpriteRenderer>().color = baseColors[colorIndex];

        StartCoroutine("StartTimer");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(activateFleeState)
        {
            StopCoroutine("Movement");
            StopCoroutine("TimeBetweenPathUpdates");

            RunFleeSearch();
            StartCoroutine("Movement");
            
            activateFleeState = false;
            stillFleeing = true;
        }

        if(count > path.Count - 2)
        {
            if(stillFleeing)
            {
                RunFleeSearch();
            }
            else
            {
                RunPlayerSearch();
            }
        }
    }

    void DebugAstar(AStarSearch search, PathNode start, PathNode goal)
    {
        if(!search.endPath.Any())
        {
            Debug.Log("Error: No Path Calculated");
            Debug.Log("StartNode: " + start.ToString());
            Debug.Log("GoalNode: " + goal.ToString());
        }

    }

    IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeBeforeStart);
        RunPlayerSearch();
        StartCoroutine("Movement");
        StartCoroutine("TimeBetweenPathUpdates");
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
            yield return new WaitForSeconds(0.5f);
        }
        
    }
    IEnumerator TimeBetweenPathUpdates()
    {
        while(player.activeSelf)
        {
            yield return new WaitForSeconds(5.0f);
            RunPlayerSearch();
            count = 1;
        }    
    }
    IEnumerator FleeTimer()
    {
        yield return new WaitForSeconds(10f);
        StopAllCoroutines();
        GetComponent<SpriteRenderer>().color = baseColors[colorIndex];

        RunPlayerSearch();
        StartCoroutine("Movement");
        StartCoroutine("TimeBetweenPathUpdates");
        Debug.Log("EndTimer");
    }
    IEnumerator Respawn()
    {
        transform.position = ghostStartPosition;
        GetComponent<SpriteRenderer>().color = baseColors[colorIndex];
        stillFleeing = false;
        yield return new WaitForSeconds(10f);
        RunPlayerSearch();
        Debug.Log("EndRespawn");
        StartCoroutine("Movement");
        StartCoroutine("TimeBetweenPathUpdates");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if (GetComponent<SpriteRenderer>().color == fleeColor)
            {
                ghostAddToScore?.Invoke(scoreValue);
                StopAllCoroutines();
                StartCoroutine("Respawn");
            }
            else 
            {
                Debug.Log("Caught");
                Application.Quit();
            }
        }
    }

    public void ActivateFlee()
    {
        Debug.Log("Flee");
        activateFleeState = true;
        GetComponent<SpriteRenderer>().color = fleeColor;
        StartCoroutine("FleeTimer");
    }

    void RunPlayerSearch()
    {
        PathNode playerMazePos = new PathNode((int)((player.transform.position.x - mazeStartPosition.x) * 2), (int)((player.transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);
        PathNode enemyMazePos = new PathNode((int)((transform.position.x - mazeStartPosition.x) * 2), (int)((transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);
        AStarSearch search = new AStarSearch(RecursiveBackTracking.map, enemyMazePos, playerMazePos);
        path = search.endPath;
        count = 0;
        DebugAstar(search, enemyMazePos, playerMazePos);
    }

    void RunFleeSearch()
    {
        Vector2 fleelocation = RecursiveBackTracking.GetPositionAwayFromPlayer(new Vector2((int)((transform.position.x - mazeStartPosition.x) * 2), (int)((transform.position.y - mazeStartPosition.y) * 2)), new Vector2((int)((player.transform.position.x - mazeStartPosition.x) * 2), (int)((player.transform.position.y - mazeStartPosition.y) * 2)));
        PathNode FleeNode = new PathNode((int)fleelocation.x, (int)fleelocation.y, RecursiveBackTracking.map);
        PathNode enemyMazePos = new PathNode((int)((transform.position.x - mazeStartPosition.x) * 2), (int)((transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);
        AStarSearch search = new AStarSearch(RecursiveBackTracking.map, enemyMazePos, FleeNode);
        path = search.endPath;
        count = 0;
        DebugAstar(search, enemyMazePos, FleeNode);
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