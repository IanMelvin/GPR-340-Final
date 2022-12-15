using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

static public class RecursiveBackTracking
{
    static List<Vector2> stack = new List<Vector2>();
    static public List<List<TypesOfSpaces>> map = new List<List<TypesOfSpaces>>();

    //Warp Tunnel Values
    static bool placeSecondWarpTunnel = false;
    static int powerPelletLoc = 0;
    static int warpTunnelRange = 0; //Determines how many spaces in line with the warp tunnel should be empty 
    static List<int> warpTunnels = new List<int>(); //stores the y value of warp tunnels
    static List<Vector3> wtBorderThickness = new List<Vector3>(); //stores the values representing how the border shifts near the Warp Tunnel(s)
    
    public enum TypesOfSpaces{
        Empty = 0,
        Border = 1,
        PuckleDibble = 2,
        PowerPellet = 3,
        WarpTunnel = 4,
        Unknown = 5
    };

    static public Vector2 StartPointStep(Vector2 dimensions, TypesOfSpaces startType)
    {
        for(int x = 0; x < dimensions.x / 2; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                if (map[x][y] == startType)
                {
                    return new Vector2(x,y);
                }
            }
        }
        return new Vector2(-1,-1);
    }

    static public Vector2 StartPointInsert(Vector2 dimensions, TypesOfSpaces startType)
    {
        for (int x = 0; x < dimensions.x / 2; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                if (Check2x2(new Vector2(x,y), dimensions))
                {
                    return new Vector2(x, y);
                }
            }
        }
        return new Vector2(-1, -1);
    }

    static public bool Step(Vector2 dimensions)
    {
        Vector2 current;
        if (stack.Count == 0)
        {
            stack.Add(StartPointStep(dimensions, TypesOfSpaces.Unknown));
            if (stack[0].x < 0)
            {
                stack.Clear();
                return false;
            }
        }

        current = stack[stack.Count - 1];
        
            
        stack.RemoveAt(stack.Count - 1);
        List<Vector2> visitables = GetVisitables(current, dimensions);

        if (CheckIfMazeBorder(current, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.Border;
        }
        else if (IsNextToEnemySpawnRange(current, dimensions))
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
        else if(IsPowerPelletLoc(current, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.PowerPellet;
        }
        else if(IsInWarpTunnelXRange(current, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.Empty;
        }
        else if(IsInWarpTunnelBorderRange(current, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.Border;
        }
        else if(CheckIfMazeBorderNeighbor(current, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.PuckleDibble;
        }
        else if (Check2x2(current, dimensions))
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.Border;
            map[(int)current.x][(int)current.y + 1] = TypesOfSpaces.Border;
            map[(int)current.x + 1][(int)current.y + 1] = TypesOfSpaces.Border;
            map[(int)current.x + 1][(int)current.y] = TypesOfSpaces.Border;
        }
        else if (map[(int)current.x][(int)current.y] == TypesOfSpaces.Unknown)
        {
            map[(int)current.x][(int)current.y] = TypesOfSpaces.PuckleDibble;
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
        warpTunnels.Clear();
        wtBorderThickness.Clear();

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
        CalcWarpTunnelValues();
        powerPelletLoc = Random.Range(1, 4);
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
            placeSecondWarpTunnel = true;
        }
        else
        {
            //Debug.Log("true");
            placeSecondWarpTunnel = true;
        }

        int random1 = Random.Range(5, (int)dimensions.y / 2 - 1);
        int random2 = Random.Range((int)dimensions.y / 2 - 1, (int)dimensions.y - 5);

        map[0][random1] = TypesOfSpaces.WarpTunnel;
        warpTunnels.Add(random1);

        if (placeSecondWarpTunnel)
        {
            map[0][random2] = TypesOfSpaces.WarpTunnel;
            warpTunnels.Add(random2);
        }
    }

    static void CalcWarpTunnelValues()
    {
        warpTunnelRange = Random.Range(1, 5);
        //Debug.Log(warpTunnelRange);

        for (int i = 0; i < warpTunnels.Count; i++)
        {
            int rand = Random.Range(1, 5);
            wtBorderThickness.Add(new Vector3(warpTunnelRange, rand, rand));
        }
        
        if(placeSecondWarpTunnel)
        {
            if (Mathf.Abs((warpTunnels[0] + wtBorderThickness[0].z) - (warpTunnels[1] - wtBorderThickness[1].z)) <= 3)
            {
                // Math to calculate above bool
                // y1 = 10, y2 = 17, rand1 = 2, rand2 = 2
                // y1 + rand1 = 12, y2 - rand2 = 15
                // if(Mathf.Abs((y1 + rand1) - (y2 - rand2)) <= 2)
                wtBorderThickness[0] = new Vector3(wtBorderThickness[0].x, wtBorderThickness[0].y, wtBorderThickness[0].z + 2);
            }
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

    static bool CheckIfMazeBorderNeighbor(Vector2 point, Vector2 dimensions)
    {
        if (point.x == 1)
        {
            return true;
        }
        else if (point.y == dimensions.y - 2)
        {
            return true;
        }
        else if (point.y == 1)
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

    static public Vector2 DetermineCenterEnemySpawnZone(Vector2 dimensions)
    {
        Vector3 enemySpawn = EnemySpawnDimensions(DeterminePlayerSpawn(dimensions));
        int center = (int)enemySpawn.y + ((int)enemySpawn.z - (int)enemySpawn.y) / 2;
        return new Vector2(dimensions.x /2 - 1, center);
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

    static bool IsInWarpTunnelXRange(Vector2 point, Vector2 dimensions)
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

    static bool IsInWarpTunnelBorderRange(Vector2 point, Vector2 dimensions)
    {
        bool yRange = point.y > 3 && point.y <= dimensions.y - 5;
        
        if (point.x <= warpTunnelRange && yRange)
        {
            for (int i = 0; i < warpTunnels.Count; i++)
            {
                bool yRangeWT = Mathf.Abs(point.y - warpTunnels[i]) <= wtBorderThickness[i].y;
                if (yRangeWT)
                {
                    return true;
                }

                yRangeWT = Mathf.Abs(point.y - warpTunnels[i]) <= wtBorderThickness[i].z;
                if (point.y > warpTunnels[i] && yRangeWT)
                {
                    return true;
                }
            }
        }

        return false;
    }

    static bool Check2x2(Vector2 point, Vector2 dimensions)
    {
        int count = 0;
        
        if (point.x < dimensions.x / 2 && point.y + 1 < dimensions.y && point.y + 1 >= 0 && map[(int)point.x][(int)point.y + 1] == TypesOfSpaces.Unknown)
        {
            count++;
        }
        if(point.x + 1 < dimensions.x / 2 && point.x >= 0 && point.y < dimensions.y && map[(int)point.x + 1][(int)point.y] == TypesOfSpaces.Unknown)
        {
            count++;
        }
        if (point.x + 1 < dimensions.x / 2 && point.x >= 0 && point.y + 1 < dimensions.y && point.y + 1 >= 0 && map[(int)point.x + 1][(int)point.y + 1] == TypesOfSpaces.Unknown)
        {
            count++;
        }

        if (count == 3)
        {
            if(point.x + 2 < dimensions.x / 2 && point.y + 2 < dimensions.y)
            {
                bool neighborCheck = map[(int)point.x - 1][(int)point.y] == TypesOfSpaces.Border || map[(int)point.x][(int)point.y - 1] == TypesOfSpaces.Border;
                bool neighborCheck1 = map[(int)point.x + 2][(int)point.y] == TypesOfSpaces.Border || map[(int)point.x + 1][(int)point.y - 1] == TypesOfSpaces.Border;
                bool neighborCheck2 = map[(int)point.x + 2][(int)point.y + 1] == TypesOfSpaces.Border || map[(int)point.x + 1][(int)point.y + 2] == TypesOfSpaces.Border;
                bool neighborCheck3 = map[(int)point.x][(int)point.y + 2] == TypesOfSpaces.Border || map[(int)point.x - 1][(int)point.y + 1] == TypesOfSpaces.Border;
                bool neighborCheck4 = map[(int)point.x - 1][(int)point.y - 1] == TypesOfSpaces.Border || map[(int)point.x + 2][(int)point.y - 1] == TypesOfSpaces.Border;
                bool neighborCheck5 = map[(int)point.x - 1][(int)point.y + 2] == TypesOfSpaces.Border || map[(int)point.x + 2][(int)point.y + 2] == TypesOfSpaces.Border;

                if (neighborCheck || neighborCheck1 || neighborCheck2 || neighborCheck3 || neighborCheck4 || neighborCheck5)
                {
                    return false;
                }

                return true;
            }

            if (point.x + 1 < dimensions.x / 2 && point.y + 1 < dimensions.y)
            {
                return true;
            }
        }
        return false;
    }

    static bool IsPowerPelletLoc(Vector2 point, Vector2 dimensions)
    {
        if(point.x == 1 && (point.y == powerPelletLoc || point.y == dimensions.y - powerPelletLoc - 1))
        {
            return true;
        }
        return false;
    }

    static public Vector2 GetPositionAwayFromPlayer(Vector2 point, Vector2 playerPosition)
    {
        int quadLocation = 0;
        if(playerPosition.x > map.Count / 2)
        {
            quadLocation += 2;
        }
        if(playerPosition.y > map[0].Count / 2)
        {
            quadLocation += 1;
        }

        Debug.Log(quadLocation);
        return moveablePositionInAQuad(quadLocation);
    }

    static Vector2 moveablePositionInAQuad(int quad)
    { 
        Vector2 xRange = new Vector2();
        Vector2 yRange = new Vector2();

        switch(quad)
        {
            case 0:
                xRange = new Vector2(map.Count / 2, map.Count);
                yRange = new Vector2(map[0].Count / 2, map[0].Count);
                break;
            case 1:
                xRange = new Vector2(0, map.Count / 2);
                yRange = new Vector2(map[0].Count / 2, map[0].Count);
                break;
            case 2:
                xRange = new Vector2(map.Count / 2, map.Count);
                yRange = new Vector2(0, map[0].Count / 2);
                break;
            case 3:
                xRange = new Vector2(0, map.Count / 2);
                yRange = new Vector2(0, map[0].Count / 2);
                break;
            default:
                xRange = Vector2.zero;
                yRange = Vector2.zero;
                break;
        }

        Vector2 newPos = new Vector2(Random.Range(xRange.x, xRange.y), Random.Range(yRange.x, yRange.y));

        while (map[(int)newPos.x][(int)newPos.y] != TypesOfSpaces.PuckleDibble)
        {
            newPos = new Vector2(Random.Range(xRange.x, xRange.y), Random.Range(yRange.x, yRange.y));
        }


        return newPos;
    }
}