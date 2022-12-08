using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGeneratorScript : MonoBehaviour
{
    [SerializeField] GameObject border;
    [SerializeField] GameObject puckleDible;
    [SerializeField] GameObject puckle;
    [SerializeField] GameObject powerPellet;
    [SerializeField] GameObject warpTunnel;
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
            Vector2 playerSpawn = RecursiveBackTracking.DeterminePlayerSpawn(mazeDimensions);
            Instantiate(puckle, new Vector3(startPosition.x + 0.5f * playerSpawn.x + 0.25f, startPosition.y + 0.5f * playerSpawn.y, 0.0f), Quaternion.identity);
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
        List<Vector2> warpTunnelLocs = new List<Vector2>();
        //Debug.Log(map[0][0]);
        
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
                else if(map[x][y] == RecursiveBackTracking.TypesOfSpaces.PowerPellet)
                {
                    maze[x].Add(Instantiate(powerPellet, new Vector3(startPosition.x + 0.5f * x, startPosition.y + 0.5f * y, 0.0f), Quaternion.identity));
                }
                else if (map[x][y] == RecursiveBackTracking.TypesOfSpaces.WarpTunnel)
                {
                    maze[x].Add(Instantiate(warpTunnel, new Vector3(startPosition.x + 0.5f * x, startPosition.y + 0.5f * y, 0.0f), Quaternion.identity));
                    warpTunnelLocs.Add(new Vector2(x,y));
                }
                else if (map[x][y] == RecursiveBackTracking.TypesOfSpaces.Unknown)
                {
                    GameObject obj = Instantiate(border, new Vector3(startPosition.x + 0.5f * x, startPosition.y + 0.5f * y, 0.0f), Quaternion.identity);
                    obj.GetComponent<SpriteRenderer>().color = Color.white;
                    maze[x].Add(obj);
                }
            }
        }

        if(warpTunnelLocs.Count % 2 != 0)
        {
            Debug.Log("Error: Odd Number of Warp Tunnels");
        }
        else
        {
            for(int i = 0; i < warpTunnelLocs.Count; i++)
            {
                if (warpTunnelLocs[i].x == 0)
                {
                    maze[(int)warpTunnelLocs[i].x][(int)warpTunnelLocs[i].y].GetComponent<WarpTunnel>().SetPartner(maze[(int)mazeDimensions.x - 1][(int)warpTunnelLocs[i].y]);
                    maze[(int)warpTunnelLocs[i].x][(int)warpTunnelLocs[i].y].GetComponent<WarpTunnel>().direction = -1;
                }
                else
                {
                    maze[(int)warpTunnelLocs[i].x][(int)warpTunnelLocs[i].y].GetComponent<WarpTunnel>().SetPartner(maze[0][(int)warpTunnelLocs[i].y]);
                    maze[(int)warpTunnelLocs[i].x][(int)warpTunnelLocs[i].y].GetComponent<WarpTunnel>().direction = 1;
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
