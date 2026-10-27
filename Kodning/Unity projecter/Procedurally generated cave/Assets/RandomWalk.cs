using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalk : MonoBehaviour
{

    public int width;
    public int height;

    int[,] map;
    public int[] agentStartingPoint = { 0, 0 };
    int[] agentGoal;

    [Range(0.01f, 1)]
    public float waitingTime = 0.5f;

    // Use this for initialization
    void Start()
    {
        //set the goal
        agentGoal = new int[] { width - 1 , height - 1};
        //start the generation process
        StartCoroutine(GenerateMap());
        //GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GenerateMap()
    {
        map = new int[width, height];
        //fill map with all walls (1s)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                map[x, y] = 1;
            }
        }
        //creates a path by randomly moving through the level
        //the agent starts at (0,0)
        int agentX = agentStartingPoint[0];
        int agentY = agentStartingPoint[1];
        //let's "make a hole" in the specified position
        map[agentX, agentY] = 0;
        //while we haven't found the exit (always in the top right corner)
        while (!(agentX == agentGoal[0] && agentY == agentGoal[1]))
        {
            //we throw a 4 sided dice (d4)
            int d4Toss = Random.Range(1, 5);
            if (d4Toss == 1) //if we roll 1 we go left
            {
                if (agentX > 0) //if the agent is not at the left border
                {
                    //we move left and make a hole
                    agentX--;
                    map[agentX, agentY] = 0;
                }
            }
            else if (d4Toss == 2) //if we roll 2 we go right
            {
                if (agentX < width - 1) //if the agent is not at the right border
                {
                    //we move right and make a hole
                    agentX++;
                    map[agentX, agentY] = 0;
                }
            }
            else if (d4Toss == 3) //if we roll 3 we go up
            {
                if (agentY < height - 1) //if the agent is not at the top border
                {
                    //we move up and make a hole
                    agentY++;
                    map[agentX, agentY] = 0;
                }
            }
            else if (d4Toss == 4) //if we roll 4 we go down
            {
                if (agentY > 0) //if the agent is not at the bottom border
                {
                    //we move down and make a hole
                    agentY--;
                    map[agentX, agentY] = 0;
                }
            }

            yield return new WaitForSeconds(waitingTime);
        }
    }

    void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}

