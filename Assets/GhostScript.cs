using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

        Vector2 playerMazePos = new Vector2(player.transform.position.x - mazeStartPosition.x, player.transform.position.y - mazeStartPosition.y) * 2;
        Debug.Log("Player: " + playerMazePos);
        Vector2 enemyMazePos = new Vector2(transform.position.x - mazeStartPosition.x, transform.position.y - mazeStartPosition.y) * 2;

        AStarSearch search = new AStarSearch(RecursiveBackTracking.map, enemyMazePos, playerMazePos);
        PrintAStar(search, enemyMazePos, playerMazePos);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*if ((player.transform.position - transform.position).magnitude < 10)
        {

        }*/
    }

    //Need to figure out how to parce through AStar, currently the goal is not a valid input to the dictionary
    //might have to rework some of the Astar foreach loop if it turns out to be the issue
    void PrintAStar(AStarSearch search, Vector2 start, Vector2 goal)
    {
        Debug.Log(search.cameFrom.Count);
        foreach(var value in search.cameFrom)
        {
            Debug.Log(value);
        }

        List<Vector2> Path = new List<Vector2>();
        Vector2 next = goal;
        Path.Add(goal);
        while(true)
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
            Debug.Log(Path[i]);
        }
        
        
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

public class AStarSearch
{
    [SerializeField] public Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
    [SerializeField] public Dictionary<Vector2, double> costSoFar = new Dictionary<Vector2, double>();

    static public double Heuristic(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x-b.x) + Mathf.Abs(a.y-b.y);
    }

    List<Vector2> GetNeighbors(Vector2 point, List<List<TypesOfSpaces>> map)
    {
        List<Vector2> neighbors = new List<Vector2>();

        //North
        if (point.x < map.Count && point.y - 1 < map[0].Count && point.y - 1 >= 0 && map[(int)point.x][(int)point.y - 1] != TypesOfSpaces.Border)
        {
            neighbors.Add(new Vector2(point.x, point.y - 1));
        }

        //West
        if (point.x - 1 < map.Count && point.x - 1 >= 0 && point.y < map[0].Count && map[(int)point.x - 1][(int)point.y] != TypesOfSpaces.Border)
        {
            neighbors.Add(new Vector2(point.x - 1, point.y));
        }

        //east
        if (point.x + 1 < map.Count && point.x >= 0 && point.y < map[0].Count && map[(int)point.x + 1][(int)point.y] != TypesOfSpaces.Border)
        {
            neighbors.Add(new Vector2(point.x + 1, point.y));
        }

        //south
        if (point.x < map.Count && point.y + 1 < map[0].Count && point.y + 1 >= 0 && map[(int)point.x][(int)point.y + 1] != TypesOfSpaces.Border)
        {
            neighbors.Add(new Vector2(point.x, point.y + 1));
        }

        return neighbors;
    }

    double Cost(Vector2 a, Vector2 b, List<List<TypesOfSpaces>> map)
    {
        return map[(int)b.x][(int)b.y] != TypesOfSpaces.Border ? 5 : 1;
    }

    public AStarSearch(List<List<TypesOfSpaces>> map, Vector2 start, Vector2 goal)
    {
        var frontier = new PriorityQueue<Vector2, double>();
        frontier.Enqueue(start,0);
        cameFrom[start] = start;
        costSoFar[start] = 0;
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            //Debug.Log("Current: " + current);
            if((int)current.x == (int)goal.x && (int)current.y == (int)goal.y)
            {
                Debug.Log("current: " + current + " goal: " + goal);
                break;
            }

            foreach (var next in GetNeighbors(current, map))
            {
                double newCost = costSoFar[current] + Cost(current, next, map);

                if(!costSoFar.ContainsKey(next) || newCost < costSoFar[next]) //There might be an issue with this, as vector comparisons are funky
                {
                    costSoFar[next] = newCost;
                    double priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        Debug.Log("Finished");
    }
}
