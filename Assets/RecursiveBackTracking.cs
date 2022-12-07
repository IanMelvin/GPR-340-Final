using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

static public class RecursiveBackTracking
{
    static List<Vector2> stack = new List<Vector2>();
    static public List<List<TypesOfSpaces>> map = new List<List<TypesOfSpaces>>();

    //Warp Tunnel Values
    static bool placeSecondWarpTunnel = false;
    static int warpTunnelRange = 0; //Determines how many spaces in line with the warp tunnel should be empty 
    static List<int> warpTunnels = new List<int>(); //stores the y value of warp tunnels
    static List<Vector2> wtBorderThickness = new List<Vector2>(); //stores the values representing how the border shifts near the Warp Tunnel(s)
    
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
            if (stack[0].x < 0) return false;
        }

        current = stack[stack.Count - 1];
        if (CheckIfMazeBorder(current, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.Border;
        }
            
        stack.RemoveAt(stack.Count - 1);
        List<Vector2> visitables = GetVisitables(current, dimensions);

        if (IsNextToEnemySpawnRange(current, dimensions))
        {
            switch (Random.Range(0, 2)){
                case 0:
                    map[(int)current.x][(int)current.y] = TypesOfSpaces.Border;
                    break;
                case 1:
                    map[(int)current.x][(int)current.y] = TypesOfSpaces.Empty;
                    break;
            }
        }
        else if(IsInWarpTunnelRange(current, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.Empty;
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

        ResetToDefaults(dimensions);
        PlaceWarpTunnels(dimensions);
        CalcWarpTunnelValues(dimensions);
    }

    static void ResetToDefaults(Vector2 dimensions)
    {
        Vector2 PlayerSpawn = DeterminePlayerSpawn(dimensions);
        map[(int)PlayerSpawn.x][(int)PlayerSpawn.y] = TypesOfSpaces.Empty;

        foreach (var point in DetermineEnemySpawnZone(dimensions))
        {
            if (IsEnemySpawnBox(point, dimensions))
            {
                //Debug.Log("here");
                map[(int)point.x][(int)point.y] = TypesOfSpaces.Border;
            }
            else
            {
                map[(int)point.x][(int)point.y] = TypesOfSpaces.Empty;
            }
        }
    }

    static void PlaceWarpTunnels(Vector2 dimensions)
    {
        int random = Random.Range(0, 4);
        if (random == 0)
        {
            //Debug.Log("false");
            placeSecondWarpTunnel = false;
        }
        else
        {
            //Debug.Log("true");
            placeSecondWarpTunnel = true;
        }

        int random1 = Random.Range(5, (int)dimensions.y / 2 - 1);
        int random2 = Random.Range((int)dimensions.y / 2 - 1, (int)dimensions.y - 1);

        map[0][random1] = TypesOfSpaces.WarpTunnel;
        warpTunnels.Add(random1);

        if (placeSecondWarpTunnel)
        {
            map[0][random2] = TypesOfSpaces.WarpTunnel;
            warpTunnels.Add(random2);
        }
    }

    //Working on how to handle the border changes near warp tunnels
    static void CalcWarpTunnelValues(Vector2 dimensions)
    {
        warpTunnelRange = Random.Range(1, 5);
        wtBorderThickness.Add(new Vector2(warpTunnelRange, Random.Range(2, 5)));
        wtBorderThickness.Add(new Vector2(warpTunnelRange, Random.Range(2, 5)));
        //Debug.Log(warpTunnelRange);
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

    static bool IsEnemySpawnBox(Vector2 point, Vector2 dimensions)
    {
        Vector3 enemySpawn = EnemySpawnDimensions(DeterminePlayerSpawn(dimensions));
        int center = (int)enemySpawn.y + ((int)enemySpawn.z - (int)enemySpawn.y)/2;
        //Debug.Log(center);
        if(point.y == center + 1 && point.x == (int)dimensions.x / 2 - 1)
        {
            return false;
        }
        else if((point.y == center + 1 || point.y == center - 1) && point.x >= (int)dimensions.x/2 - 3)
        {
            return true;
        }
        else if(point.y == center && point.x == (int)dimensions.x / 2 - 3)
        {
            return true;
        }

        return false;
    }

    static Vector3 EnemySpawnDimensions(Vector2 playerSpawn)
    {
        int lowestPointY = (int)playerSpawn.y + 7;
        int highestPointY = (int)playerSpawn.y + 12;
        int lowestPointX = (int)playerSpawn.x - 3;

        return new Vector3(lowestPointX, lowestPointY, highestPointY);
    }

    static bool IsNextToEnemySpawnRange(Vector2 point, Vector2 dimensions)
    {
        Vector3 enemySpawnRange = EnemySpawnDimensions(DeterminePlayerSpawn(dimensions));

        if (point.x >= enemySpawnRange.x - 1 && point.y >= enemySpawnRange.y - 1 && point.y <= enemySpawnRange.z)
        {
            //Debug.Log("true");
            return true;
        }

        return false;
    }

    static bool IsInWarpTunnelRange(Vector2 point, Vector2 dimensions)
    {
        foreach(var warpTunnel in warpTunnels)
        {
            if (point.y == warpTunnel && point.x <= warpTunnelRange)
            {
                return true;
            }
        }
        
        return false;
    }
}