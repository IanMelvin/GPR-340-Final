using System;
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

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        state.x = 0;

        PathNode playerMazePos = new PathNode((int)((player.transform.position.x - mazeStartPosition.x) * 2), (int)((player.transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);
        PathNode enemyMazePos = new PathNode((int)((transform.position.x - mazeStartPosition.x) * 2), (int)((transform.position.y - mazeStartPosition.y) * 2), RecursiveBackTracking.map);

        AStarSearch search = new AStarSearch(RecursiveBackTracking.map, enemyMazePos, playerMazePos);
        PrintAStar(search, enemyMazePos, playerMazePos);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    void PrintAStar(AStarSearch search, PathNode start, PathNode goal)
    {
        Debug.Log(search.endPath.Any());

        
        /*while(true)
        {
            if ((int)search.cameFrom[next].x == (int)start.x && (int)search.cameFrom[next].y == (int)start.y)
            {
                break;
            }

            Path.Add(search.cameFrom[next]);
            next = search.cameFrom[next];
        }

        for(int i = 0; i < Path.Count; i++)
        {
            Debug.Log(Path[i].x + ", " + Path[i].y);
        }*/
    }
}


//Based on PriorityQueue implemented https://visualstudiomagazine.com/Articles/2012/11/01/Priority-Queues-with-C.aspx?Page=1
public class PriorityQueue <P, T> where T : IComparable<T>
{
    private List<T> priority;
    private List<P> element;

    public PriorityQueue()
    {
        this.priority = new List<T>();
        this.element = new List<P>();
    }

    public void Enqueue(P item, T weight)
    {
        priority.Add(weight);
        element.Add(item);
        int ci = priority.Count - 1;

        while(ci > 0)
        {
            int pi = (ci - 1) / 2;
            if (priority[ci].CompareTo(priority[pi]) >= 0) break;
            T tmp = priority[ci]; priority[ci] = priority[pi]; priority[pi] = tmp;
            P temp = element[ci]; element[ci] = element[pi]; element[pi] = temp;
            ci = pi;
        }

    }

    public P Dequeue()
    {
        int li = priority.Count - 1;
        T front = priority[0];
        P frontItem = element[0];
        priority[0] = priority[li];
        element[0] = element[li];
        priority.RemoveAt(li);
        element.RemoveAt(li);

        --li;
        int pi = 0;
        while(true)
        {
            int ci = pi * 2 + 1;
            if (ci > li) break;
            int rc = ci + 1;
            if (rc <= li && priority[rc].CompareTo(priority[ci]) < 0) ci = rc;
            if (priority[pi].CompareTo(priority[ci]) <= 0) break;
            T tmp = priority[pi]; priority[pi] = priority[ci]; priority[ci] = tmp;
            P temp = element[pi]; element[pi] = element[ci]; element[ci] = temp;
            pi = ci;
        }

        return frontItem;
    }

    public int Count
    {
        get { return priority.Count; }
    }

    public T Peek()
    {
        T frontItem = priority[0];
        return frontItem;
    }

    public bool IsConsistent()
    {
        if (priority.Count == 0) return true;
        int li = priority.Count - 1;
        for (int pi = 0; pi < priority.Count; ++pi)
        {
            int lci = 2 * pi + 1;
            int rci = 2 * pi + 2;
            if (lci <= li && priority[pi].CompareTo(priority[lci]) > 0) return false;
            if (rci <= li && priority[pi].CompareTo(priority[rci]) > 0) return false;
        }
        return true;
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

    /*
    public static PathNode operator *(PathNode a, int b)
    {
        return new PathNode(a.x * b, a.y * b, a.Map);
    }

    public static bool operator ==(PathNode a, PathNode b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(PathNode a, PathNode b)
    {
        return a.x != b.x && a.y != b.y;
    }

    public override bool Equals(object o)
    {  
       return true;  
    } 

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ (y.GetHashCode() << 2);
    }
    */
    public override string ToString()
    {
        return x + "," + y;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

}

public class AStarSearch
{
    [SerializeField] public Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();
    [SerializeField] public Dictionary<PathNode, double> costSoFar = new Dictionary<PathNode, double>();

    List<PathNode> openList;
    List<PathNode> closedList;
    List<PathNode> grid = new List<PathNode>();
    public List<PathNode> endPath = new List<PathNode>();

    static public double Heuristic(PathNode a, PathNode b)
    {
        return Mathf.Abs(a.x-b.x) + Mathf.Abs(a.y-b.y);
    }

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

    double Cost(PathNode a, PathNode b, List<List<TypesOfSpaces>> map)
    {
        return map[(int)b.x][(int)b.y] != TypesOfSpaces.Border ? 5 : 1;
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
        //var frontier = new PriorityQueue<PathNode, double>();
        //frontier.Enqueue(start, 0);
        
        openList = new List<PathNode> { start };
        closedList = new List<PathNode>();

        //cameFrom[start] = start;
        //costSoFar[start] = 0;

        ResetGrid(start, goal);

        start.gCost = 0;
        start.hCost = CalculateDistanceCost(start, goal);
        start.CalculateFCost();
        //Debug.Log(start.gCost);
        //Debug.Log(start.hCost);
        //Debug.Log(start.fCost);

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if(currentNode == goal)
            {
                Debug.Log("Over");
                CalculatePath(goal);
                break;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach(PathNode neighborNode in GetNeighbors(currentNode, map))
            {
                Debug.Log("Running");
                if (closedList.Contains(neighborNode)) continue;

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);
                Debug.Log((tentativeGCost < neighborNode.gCost) + " " + tentativeGCost + " " + neighborNode.gCost);
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
            Debug.Log(openList.Count);
        }

        /*while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            //Debug.Log("Current: " + current);
            if(current == goal)
            {
                Debug.Log("current: " + current + " goal: " + goal);
                break;
            }

            foreach (var next in GetNeighbors(current, map)) //New issue has 
            {
                double newCost = costSoFar[current] + Cost(current, next, map);

                if(!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    double priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }*/

        Debug.Log("Finished");
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