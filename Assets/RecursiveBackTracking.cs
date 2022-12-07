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
        return new Vector2((int)(dimensions.x / 2) - 1, (int)(dimensions.y / 4) + 1);
    }

    static public bool Step(Vector2 dimensions)
    {
        Vector2 current;
        if (stack.Count == 0)
        {
            stack.Add(StartPoint(dimensions));
            if (map[(int)stack[0].x][(int)stack[0].y] != TypesOfSpaces.Unknown) return false;
            map[(int)stack[0].x][(int)stack[0].y] = TypesOfSpaces.Empty;
            current = stack[stack.Count - 1];
        }
        else
        {
            current = stack[stack.Count - 1];
            if (CheckIfMazeBorder(current, dimensions))
            {
                map[(int)current.x][(int)current.y] = TypesOfSpaces.Border;
            }
            else
            {
                //if (map[(int)current.x][(int)current.y] != TypesOfSpaces.Unknown) Debug.Log("Override");
                map[(int)current.x][(int)current.y] = TypesOfSpaces.PuckleDibble;
            }
        }
        stack.RemoveAt(stack.Count - 1);

        List<Vector2> visitables = GetVisitables(current, dimensions);
        
        if(visitables.Count <= 0)
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
        if (point.x < dimensions.x / 2 && point.y - 1 < dimensions.y && point.y - 1 >= 0 && map[(int)point.x][(int)point.y - 1] == TypesOfSpaces.Unknown)
        {
            list.Add(new Vector2(point.x, point.y - 1));
        }
        else if(point.x - 1 < dimensions.x / 2 && point.x - 1 >= 0 && point.y < dimensions.y && map[(int)point.x - 1][(int)point.y] == TypesOfSpaces.Unknown)
        {
            list.Add(new Vector2(point.x - 1, point.y));
        }
        else if(point.x + 1 < dimensions.x / 2 && point.x >= 0 && point.y < dimensions.y && map[(int)point.x + 1][(int)point.y] == TypesOfSpaces.Unknown)
        {
            list.Add(new Vector2(point.x + 1, point.y));
        }
        else if(point.x < dimensions.x / 2 && point.y + 1 < dimensions.y && point.y + 1 >= 0 && map[(int)point.x][(int)point.y + 1] == TypesOfSpaces.Unknown)
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
}
