using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class MazeGeneratorScript : MonoBehaviour
{
    [SerializeField] GameObject border;
    [SerializeField] GameObject puckleDible;
    [SerializeField] GameObject puckle;
    [SerializeField] Vector2 mazeDimensions;
    [SerializeField] Vector2 startPosition;
    List<List<GameObject>> maze;

    bool genMaze = true;
    bool mazeReady = false;

    // Start is called before the first frame update
    void Start()
    {
        maze = new List<List<GameObject>>();
        startPosition = Vector2.zero + ((mazeDimensions / 4) * -1);
        RecursiveBackTracking.Clear(mazeDimensions);
        StartCoroutine("generateMaze");
    }

    private void FixedUpdate()
    {
        if(!genMaze && !mazeReady)
        {
            FillMaze();
        }
    }

    bool GenerateMaze()
    {
        return RecursiveBackTracking.Step(mazeDimensions);
    }

    void Fold()
    {
        RecursiveBackTracking.Fold(mazeDimensions);
    }

    void FillMaze()
    {
        List<List<RecursiveBackTracking.TypesOfSpaces>> map = RecursiveBackTracking.map;
        
        for(int x = 0; x < map.Count; x++)
        {
            maze.Add(new List<GameObject>());
            for (int y = 0; y < map[0].Count; y ++)
            {
                if (map[x][y] == RecursiveBackTracking.TypesOfSpaces.Border)
                {
                    maze[x].Add(Instantiate(border, new Vector3(startPosition.x + 0.5f * x, startPosition.y + 0.5f * y, 0.0f), Quaternion.identity));
                }
                else if (map[x][y] == RecursiveBackTracking.TypesOfSpaces.PuckleDibble)
                {
                    maze[x].Add(Instantiate(puckleDible, new Vector3(startPosition.x + 0.5f * x, startPosition.y + 0.5f * y, 0.0f), Quaternion.identity));
                }
                else if (map[x][y] == RecursiveBackTracking.TypesOfSpaces.Unknown)
                {
                    GameObject obj = Instantiate(border, new Vector3(startPosition.x + 0.5f * x, startPosition.y + 0.5f * y, 0.0f), Quaternion.identity);
                    obj.GetComponent<SpriteRenderer>().color = Color.white;
                    maze[x].Add(obj);
                }
            }
        }

        mazeReady = true;
    }

    IEnumerator generateMaze()
    {
        while (genMaze)
        {
            genMaze = GenerateMaze();
        }
        Fold();
        Debug.Log("generated");
        yield return null;
    }
}
