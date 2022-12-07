using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

static public class RecursiveBackTracking
{
    static List<Vector2> stack = new List<Vector2>();
    static public List<List<TypesOfSpaces>> map = new List<List<TypesOfSpaces>>();
    
    public enum TypesOfSpaces{
        Empty = 0,
        Border = 1,
        PuckleDibble = 2,
        PowerPellet = 3,
        WarpTunnel = 4,
        Unknown = 5
    };

    static public Vector2 StartPoint(Vector2 dimensions)
    {
        for(int x = 0; x < dimensions.x / 2; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                if (map[x][y] == TypesOfSpaces.Unknown)
                {
                    return new Vector2(x,y);
                }
            }
        }
        return new Vector2(-1,-1);
    }

    static public bool Step(Vector2 dimensions)
    {
        Vector2 current;
        if (stack.Count == 0)
        {
            stack.Add(StartPoint(dimensions));
            //current = stack[stack.Count - 1];
            if (stack[0].x < 0) return false;

            /*if (CheckIfMazeBorder(current, dimensions))
            {
                map[(int)current.x][(int)current.y] = TypesOfSpaces.Border;
            }
            else
            {
                map[(int)stack[0].x][(int)stack[0].y] = TypesOfSpaces.Empty;
            }*/
            
        }
        //else
        //{
            current = stack[stack.Count - 1];
            if (CheckIfMazeBorder(current, dimensions))
            {
                map[(int)current.x][(int)current.y] = TypesOfSpaces.Border;
            }
            else
            {
                //if (map[(int)current.x][(int)current.y] != TypesOfSpaces.Unknown) Debug.Log("Override");
                //map[(int)current.x][(int)current.y] = TypesOfSpaces.PuckleDibble;
            }
            
        //}
        stack.RemoveAt(stack.Count - 1);
        List<Vector2> visitables = GetVisitables(current, dimensions);

        if (isNeighborInEnemySpawn(visitables, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.PowerPellet;
        }
        else
        {
            if (map[(int)current.x][(int)current.y] == TypesOfSpaces.Unknown)
            {
                map[(int)current.x][(int)current.y] = TypesOfSpaces.PuckleDibble;
            }
        }

        if (visitables.Count <= 0)
        {
            return true;
        }
        else
        {
            int r = Random.Range(0, visitables.Count);
            Vector2 next = visitables[r];
            stack.Add(next);

            return true;
        }
    }

    static public void Clear(Vector2 dimensions)
    {
        stack.Clear();
        map.Clear();

        for(int x = 0; x < dimensions.x; x++)
        {
            map.Add(new List<TypesOfSpaces>());
            for(int y = 0; y < dimensions.y; y++)
            {
                map[x].Add(TypesOfSpaces.Unknown);
            }
        }

        Vector2 PlayerSpawn = DeterminePlayerSpawn(dimensions);
        map[(int)PlayerSpawn.x][(int)PlayerSpawn.y] = TypesOfSpaces.Empty;

        foreach(var point in DetermineEnemySpawnZone(dimensions))
        {
            map[(int)point.x][(int)point.y] = TypesOfSpaces.Empty;
        }
    }

    static public void Fold(Vector2 dimensions)
    {
        for (int x = (int)dimensions.x/2; x < dimensions.x; x++)
        {
            int invX = (int)(dimensions.x / 2 - 1) - (x - (int)(dimensions.x / 2));
            for (int y = 0; y < dimensions.y; y++)
            {
                map[x][y] = map[invX][y];
            }
        }
    }

    static List<Vector2> GetVisitables(Vector2 point, Vector2 dimensions)
    {
        List<Vector2> list = new List<Vector2>();
        //Debug.Log(point);

        //North
        if (point.x < dimensions.x / 2 && point.y - 1 < dimensions.y && point.y - 1 >= 0 && map[(int)point.x][(int)point.y - 1] == TypesOfSpaces.Unknown)
        {
            list.Add(new Vector2(point.x, point.y - 1));
        }

        //west
        else if (point.x - 1 < dimensions.x / 2 && point.x - 1 >= 0 && point.y < dimensions.y && map[(int)point.x - 1][(int)point.y] == TypesOfSpaces.Unknown)
        {
            list.Add(new Vector2(point.x - 1, point.y));
        }

        //east
        else if (point.x + 1 < dimensions.x / 2 && point.x >= 0 && point.y < dimensions.y && map[(int)point.x + 1][(int)point.y] == TypesOfSpaces.Unknown)
        {
            list.Add(new Vector2(point.x + 1, point.y));
        }

        //south
        else if (point.x < dimensions.x / 2 && point.y + 1 < dimensions.y && point.y + 1 >= 0 && map[(int)point.x][(int)point.y + 1] == TypesOfSpaces.Unknown)
        {
            list.Add(new Vector2(point.x, point.y + 1));
        }

        return list;
    }

    static bool CheckIfMazeBorder(Vector2 point, Vector2 dimensions)
    {
        if (point.x == 0)
        {
            return true;
        }
        else if (point.y == dimensions.y - 1)
        {
            return true;
        }
        else if (point.y == 0)
        {
            return true;
        }

        return false;
    }

    static public Vector2 DeterminePlayerSpawn(Vector2 dimensions)
    {
        return new Vector2((int)(dimensions.x / 2) - 1, (int)(dimensions.y / 4) + 1);
    }

    static public List<Vector2> DetermineEnemySpawnZone(Vector2 dimensions)
    {
        List<Vector2> list = new List<Vector2>();
        Vector3 enemySpawnRange = EnemySpawnDimensions(DeterminePlayerSpawn(dimensions));

        for(int x = (int)enemySpawnRange.x; x < dimensions.x /2; x++)
        {
            for(int y = (int)enemySpawnRange.y; y < (int)enemySpawnRange.z; y++)
            {
                list.Add(new Vector2(x,y));
            }
        }

        return list;
    }

    static Vector3 EnemySpawnDimensions(Vector2 playerSpawn)
    {
        int lowestPointY = (int)playerSpawn.y + 6;
        int highestPointY = (int)playerSpawn.y + 13;
        int lowestPointX = (int)playerSpawn.x - 4;

        return new Vector3(lowestPointX, lowestPointY, highestPointY);
    }

    //1 reason it's not working is the neighbors vector is not passing in any neighbors that in the enemy spawn range
    //Don't make it dependent on the neightbors but instead the point itself
    static bool isNeighborInEnemySpawn(List<Vector2> neighbors, Vector2 dimensions)
    {
        Vector3 enemySpawnRange = EnemySpawnDimensions(DeterminePlayerSpawn(dimensions));

        foreach(var point in neighbors)
        {
            if (point.x >= enemySpawnRange.x && point.y >= enemySpawnRange.y && point.y <= enemySpawnRange.z)
            {
                Debug.Log("true");
                return true;
            }
        }

        return false;
    }
}
