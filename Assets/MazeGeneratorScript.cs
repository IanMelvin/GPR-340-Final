using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGeneratorScript : MonoBehaviour
{
    [SerializeField] GameObject border;
    [SerializeField] Vector2 mazeDimensions;
    [SerializeField] Vector2 startPosition;
    List<List<GameObject>> maze;

    enum Directions
    {
        North = 0,
        South = 1,
        East = 2,
        West = 3
    }

    struct Node
    {
        Vector2 location;
        Directions dir;
    };

    // Start is called before the first frame update
    void Start()
    {
        maze = new List<List<GameObject>>();
        startPosition = Vector2.zero + ((mazeDimensions / 4) * -1);
        FillMaze();
        CarveMaze();
    }

    void FillMaze()
    {
        for(int i = 0; i < mazeDimensions.y; i++)
        {
            maze.Add(new List<GameObject>());
        }
        
        Debug.Log(maze.Count);
        for(int x = 0; x < maze.Count; x++)
        {
            Debug.Log(x);
            for(int y = 0; y < mazeDimensions.x; y ++)
            {
                maze[x].Add(Instantiate(border, new Vector3(startPosition.x + 0.5f * x, startPosition.y + 0.5f * y, 0.0f), Quaternion.identity));
            }
        }
    }

    void CarveMaze()
    {

    }
}
