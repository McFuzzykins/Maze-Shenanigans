using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridScript : MonoBehaviour
{
    public Transform cellPrefab;
    public Vector3 size = new Vector3(20, 0, 20);
    public Transform[,] grid;
    public List<Transform> passages = new List<Transform>();
    public List<Transform> walls = new List<Transform>();
    public bool isOn = true;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        RandomNumbers();
        SetAdj();
        PrimThisAlgorithm();
    }

    void Update()
    {
        //new maze button go brrr
        if (Input.GetKeyDown("`"))
        {
            RandomNumbers();
            SetAdj();
            PrimThisAlgorithm();
        }

        if (Input.GetKeyDown("0"))
        {
            foreach (Transform cell in transform)
            {
                if (cell.GetComponent<CellScript>().isWall == true)
                {
                    cell.GetComponentInChildren<TMP_Text>().enabled = !isOn;
                }
            }
            isOn = !isOn;
        }
    }

    //Big O(n^2), gotta love it...
    void CreateGrid()
    {
        grid = new Transform[(int)size.x, (int)size.z];

        for (int i = 0; i < size.x; ++i)
        {
            for (int j = 0; j < size.z; ++j)
            {
                Transform newCell;

                //Making the grid an actual thing in Unity with the defined Prefab object 
                newCell = (Transform)Instantiate(cellPrefab, new Vector3(i, 0, j), Quaternion.identity);

                //setting each cell's position within itself
                newCell.GetComponent<CellScript>().pos = new Vector3(i, 0, j);

                //naming each cell cause it might come in handy later
                //Note: It really came in handy when debugging things, highly recommend this naming scheme
                newCell.name = string.Format("({0}, 0, {1})", i, j);

                //parenting the cell to the grid GameObject
                newCell.parent = transform;

                //Adding each cell into the grid array
                grid[i, j] = newCell;
            }     
        }
    }

    //Big O(n), not as bad as the others
    void RandomNumbers()
    {
        //Looping through each cell in our grid
        foreach (Transform child in transform)
        {
            //check to determine if we restarted while running the program
            if (child.GetComponentInChildren<MeshRenderer>().enabled == false)
            {
                //resetting the things that need to be reset
                child.GetComponent<CellScript>().isWall = true;
                child.GetComponentInChildren<MeshRenderer>().enabled = true;
                child.GetComponentInChildren<TMP_Text>().enabled = true;
            }

            int weight = Random.Range(0, 100);

            //setting the weight on each cell so it actually means something
            child.GetComponent<CellScript>().weight = weight;

            //visualizing the weights so I don't go insane later
            child.GetComponentInChildren<TMP_Text>().text = child.GetComponent<CellScript>().weight.ToString();
        }
    }

    void PrimThisAlgorithm()
    {
        //some randomization to help choose a starting cell
        int randX = (int)Random.Range(0, size.x);
        int randZ = (int)Random.Range(0, size.z);

        //Choose random starting cell
        Transform start = grid[randX, randZ];
        Debug.Log("Start is cell: " + start.name);
        
        //frontier cell similar to cursors in LinkedLists 
        //It's a surprise tool that'll help us later
        Transform frontier;

        //A few lists that will:
        //1. store all frontiers we find - Frontiers
        //2. store the frontiers of the frontiers we find (except the 2nd set of frontiers are Passages, not Walls) - frontierNeighbors
        //3. store the frontiers we've visited so we don't endlessly keep visiting cells. 
        //Note: I could probably just mark the cell in CellScript as Visited with a bool, but I was running on Day 2 of nothing but this code. 
        List<Transform> Frontiers = new List<Transform>();
        List<Transform> frontierNeighbors = new List<Transform>();
        List<Transform> visitedCells = new List<Transform>();

        //Add that cell to the Passage list, then make it a passage in Unity (Essentially turning it all off)
        passages.Add(start);
        start.GetComponent<CellScript>().isWall = false;
        grid[randX, randZ].GetComponentInChildren<MeshRenderer>().enabled = false;
        grid[randX, randZ].GetComponentInChildren<TMP_Text>().enabled = false;

        //Add all distance 2 cells to start's frontiers list and our master Frontier list
        //if statements check first if x + 2 is outside the index range of our grid, if so, don't even try to do the rest. It doesn't exist, no need to try
        if ((int)(start.transform.position.x + 2) <= size.x - 1 && 
            grid[(int)start.transform.position.x + 2, (int)start.transform.position.z].GetComponent<CellScript>().isWall == true)
        {
            //I added the cells to each list by referencing the grid placement of it instead of using start
            //because I didn't want to risk referencing start and having a reference to the transform and not the actual thing
            start.GetComponent<CellScript>().frontiers.Add(grid[(int)start.transform.position.x + 2, (int)start.transform.position.z]);
            Frontiers.Add(grid[(int)start.transform.position.x + 2, (int)start.transform.position.z]);

            //Adding to walls for the Maze Solver to reference later
            walls.Add(grid[(int)start.transform.position.x + 2, (int)start.transform.position.z]);
        }

        if ((int)(start.transform.position.x - 2) >= 0 &&
            grid[(int)start.transform.position.x - 2, (int)start.transform.position.z].GetComponent<CellScript>().isWall == true)
        {
            start.GetComponent<CellScript>().frontiers.Add(grid[(int)start.transform.position.x - 2, (int)start.transform.position.z]);
            Frontiers.Add(grid[(int)start.transform.position.x - 2, (int)start.transform.position.z]);
            walls.Add(grid[(int)start.transform.position.x - 2, (int)start.transform.position.z]);
        }

        if ((int)(start.transform.position.z + 2) <= size.z - 1 &&
            grid[(int)start.transform.position.x, (int)start.transform.position.z + 2].GetComponent<CellScript>().isWall == true)
        {
            start.GetComponent<CellScript>().frontiers.Add(grid[(int)start.transform.position.x, (int)start.transform.position.z + 2]);
            Frontiers.Add(grid[(int)start.transform.position.x, (int)start.transform.position.z + 2]);
            walls.Add(grid[(int)start.transform.position.x, (int)start.transform.position.z + 2]);
        }

        if ((int)(start.transform.position.z - 2) >= 0 &&
            grid[(int)start.transform.position.x, (int)start.transform.position.z - 2].GetComponent<CellScript>().isWall == true)
        {
            start.GetComponent<CellScript>().frontiers.Add(grid[(int)start.transform.position.x, (int)start.transform.position.z - 2]);
            Frontiers.Add(grid[(int)start.transform.position.x, (int)start.transform.position.z - 2]);
            walls.Add(grid[(int)start.transform.position.x, (int)start.transform.position.z - 2]);
        }

        //Original attempt at grabbing frontiers, but I didn't like the O(n^2) time complexity of it. Also wasn't needed
        /*
        foreach (Transform adj in start.GetComponent<CellScript>().adj)
        {
            foreach (Transform adj2 in adj.GetComponent<CellScript>().adj)
            {
                if (adj2.transform.position.x == (start.transform.position.x + 2) && adj2.GetComponent<CellScript>().isWall == true)
                {
                    startFrontiers.Add(adj2);
                    Debug.Log("Added to startFrontiers: " + adj2 + "list Count: " + startFrontiers.Count);
                }

                if (adj2.transform.position.x == (start.transform.position.x - 2) && adj2.GetComponent<CellScript>().isWall == true)
                {
                    startFrontiers.Add(adj2);
                    Debug.Log("Added to startFrontiers: " + adj2 + "list Count: " + startFrontiers.Count);
                }

                if (adj2.transform.position.z == (start.transform.position.z + 2) && adj2.GetComponent<CellScript>().isWall == true)
                {
                    startFrontiers.Add(adj2);
                    Debug.Log("Added to startFrontiers: " + adj2 + "list Count: " + startFrontiers.Count);
                }

                if (adj2.transform.position.z == (start.transform.position.z - 2) && adj2.GetComponent<CellScript>().isWall == true)
                {
                    startFrontiers.Add(adj2);
                    Debug.Log("Added to startFrontiers: " + adj2 + "list Count: " + startFrontiers.Count);
                }
            }
        }
        */

        //I really want to try recursively calling this part of the function to help with the Big O complexity
        //While there are frontiers, choose a random one
        while (Frontiers.Count > 0)
        {
            //setting frontier cursor to random frontier in our master list
            frontier = Frontiers[Random.Range(0, Frontiers.Count)];
            
            //setting frontierNeighbors to be the neighbors of our current frontier cell
            frontierNeighbors = frontier.GetComponent<CellScript>().neighbors;
            Debug.Log("frontier is: " + frontier);

            //check neighbors (frontiers of chosen frontier cell) to see if any of them are passages
            //if any are passages, add them to our frontierNeighbors list
            if ((int)frontier.transform.position.x + 2 <= size.x - 1 &&
                grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z].GetComponent<CellScript>().isWall == false)
            {
                frontierNeighbors.Add(grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z]);
            }

            if ((int)frontier.transform.position.x - 2 >= 0 &&
                grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z].GetComponent<CellScript>().isWall == false)
            {
                frontierNeighbors.Add(grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z]);
            }

            if ((int)frontier.transform.position.z + 2 <= size.z - 1 &&
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2].GetComponent<CellScript>().isWall == false)
            {
                frontierNeighbors.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2]);
            }

            if ((int)frontier.transform.position.z - 2 >= 0 &&
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2].GetComponent<CellScript>().isWall == false)
            {
                frontierNeighbors.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2]);
            }

            Debug.Log("frontierNeighbors Count: " + frontierNeighbors.Count);

            //Also didn't like the Big O(n^3) here since it's also in a while loop. Made me want to vomit
            /*
            foreach (Transform adj in startFrontiers)
            {
                Debug.Log("Checking adjacents to: " + adj);
                foreach (Transform adj2 in adj.GetComponent<CellScript>().adj)
                {
                    Debug.Log("Adjecent: " + adj2);

                    if (adj2.transform.position.x == (frontier.transform.position.x + 2) && adj2.GetComponent<CellScript>().isWall == false)
                    {
                        frontierNeighbors.Add(adj2);
                        Debug.Log("Added to frontierNeighbors: " + adj2 + "list Count: " + frontierNeighbors.Count);
                    }

                    if (adj2.transform.position.x == (frontier.transform.position.x - 2) && adj2.GetComponent<CellScript>().isWall == false)
                    {
                        frontierNeighbors.Add(adj2);
                        Debug.Log("Added to frontierNeighbors: " + adj2 + "list Count: " + frontierNeighbors.Count);
                    }

                    if (adj2.transform.position.z == (frontier.transform.position.z + 2) && adj2.GetComponent<CellScript>().isWall == false)
                    {
                        frontierNeighbors.Add(adj2);
                        Debug.Log("Added to frontierNeighbors: " + adj2 + "list Count: " + frontierNeighbors.Count);
                    }

                    if (adj2.transform.position.z == (frontier.transform.position.z - 2) && adj2.GetComponent<CellScript>().isWall == false)
                    {
                        frontierNeighbors.Add(adj2);
                        Debug.Log("Added to frontierNeighbors: " + adj2 + "list Count: " + frontierNeighbors.Count);
                    }
                }
            }
            */

            //Choose a random neighbor to our current frontier from list
            Transform randNeighbor;
            Transform connection;
            randNeighbor = frontierNeighbors[Random.Range(0, frontierNeighbors.Count)];

            //Deciphering which cell is the adj of both randNeighbor && frontier (which one is inbetween them), then making it a passage
            if ((int)randNeighbor.transform.position.x + 2 <= size.x - 1 &&
                grid[(int)randNeighbor.transform.position.x + 2, (int)randNeighbor.transform.position.z] ==
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z] &&
                grid[(int)randNeighbor.transform.position.x + 2, (int)randNeighbor.transform.position.z].GetComponent<CellScript>().isWall == true)
            {
                connection = grid[(int)randNeighbor.transform.position.x + 1, (int)randNeighbor.transform.position.z];
                connection.GetComponent<CellScript>().isWall = false;
                connection.GetComponent<MeshRenderer>().enabled = false;
                connection.GetComponentInChildren<TMP_Text>().enabled = false;
            }
            else if ((int)randNeighbor.transform.position.x - 2 >= 0 &&
                grid[(int)randNeighbor.transform.position.x - 2, (int)randNeighbor.transform.position.z] ==
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z] &&
                grid[(int)randNeighbor.transform.position.x - 2, (int)randNeighbor.transform.position.z].GetComponent<CellScript>().isWall == true)
            {
                connection = grid[(int)randNeighbor.transform.position.x - 1, (int)randNeighbor.transform.position.z];
                connection.GetComponent<CellScript>().isWall = false;
                connection.GetComponent<MeshRenderer>().enabled = false;
                connection.GetComponentInChildren<TMP_Text>().enabled = false;
            }
            else if ((int)randNeighbor.transform.position.z + 2 <= size.z - 1 &&
                grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z + 2] ==
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z] &&
                grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z + 2].GetComponent<CellScript>().isWall == true)
            {
                connection = grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z + 1];
                connection.GetComponent<CellScript>().isWall = false;
                connection.GetComponent<MeshRenderer>().enabled = false;
                connection.GetComponentInChildren<TMP_Text>().enabled = false;
            }
            else if ((int)randNeighbor.transform.position.z - 2 >= 0 &&
                grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z - 2] ==
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z] &&
                grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z - 2].GetComponent<CellScript>().isWall == true)
            {
                connection = grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z - 1];
                connection.GetComponent<CellScript>().isWall = false;
                connection.GetComponent<MeshRenderer>().enabled = false;
                connection.GetComponentInChildren<TMP_Text>().enabled = false;
            }

            //adding the frontiers of the current frontier to our Frontier list
            if ((int)frontier.transform.position.x + 2 <= size.x - 1 && !visitedCells.Contains(grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z]) &&
                grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z].GetComponent<CellScript>().isWall == true)
            {
                frontier.GetComponent<CellScript>().frontiers.Add(grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z]);
                Frontiers.Add(grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z]);
            }

            if ((int)frontier.transform.position.x - 2 >= 0 && !visitedCells.Contains(grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z]) &&
                grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z].GetComponent<CellScript>().isWall == true)
            {
                frontier.GetComponent<CellScript>().frontiers.Add(grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z]);
                Frontiers.Add(grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z]);
            }

            if ((int)frontier.transform.position.z + 2 <= size.x - 1 && !visitedCells.Contains(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2]) &&
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2].GetComponent<CellScript>().isWall == true)
            {
                frontier.GetComponent<CellScript>().frontiers.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2]);
                Frontiers.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2]);
            }

            if ((int)frontier.transform.position.z - 2 >= 0 && !visitedCells.Contains(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2]) &&
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2].GetComponent<CellScript>().isWall == true)
            {
                frontier.GetComponent<CellScript>().frontiers.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2]);
                Frontiers.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2]);
            }

            //removing current frontier since we have done what we need with it
            Debug.Log("Frontiers List Count: " + Frontiers.Count);
            Frontiers.Remove(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z]);

            //Nothing I found says this, but I found that if you don't set the removed frontier as a passage
            //then you'll only get the start cell and its adjacent cells as passages, at most
            //so I add the frontier we just used as a passage (maybe I should add a coin flip to determine if we do that?)
            passages.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z]);
            frontier.GetComponent<CellScript>().isWall = false;
            frontier.GetComponent<MeshRenderer>().enabled = false;
            frontier.GetComponentInChildren<TMP_Text>().enabled = false;

            //add the frontier we just got rid of to the visitedCells list (The Trashcan)
            visitedCells.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z]);
            Debug.Log("Frontiers List Count: " + Frontiers.Count);
        }

        //This was my bs way of ensuring the outer border of the grid is guaranteed to be there
        //100% don't want to keep it, currently working on fixing the algorithm to not be able to select the border
        for (int i = 0; i < size.x; ++i)
        {
            grid[0, i].GetComponent<CellScript>().isWall = true;
            grid[0, i].GetComponent<MeshRenderer>().enabled = true;
            grid[0, i].GetComponentInChildren<TMP_Text>().enabled = true;

            grid[i, 0].GetComponent<CellScript>().isWall = true;
            grid[i, 0].GetComponent<MeshRenderer>().enabled = true;
            grid[i, 0].GetComponentInChildren<TMP_Text>().enabled = true;

            grid[(int)size.x - 1, i].GetComponent<CellScript>().isWall = true;
            grid[(int)size.x - 1, i].GetComponent<MeshRenderer>().enabled = true;
            grid[(int)size.x - 1, i].GetComponentInChildren<TMP_Text>().enabled = true;

            grid[i, (int)size.x - 1].GetComponent<CellScript>().isWall = true;
            grid[i, (int)size.x - 1].GetComponent<MeshRenderer>().enabled = true;
            grid[i, (int)size.x - 1].GetComponentInChildren<TMP_Text>().enabled = true;
        }
    }

    //I hate the Big O(n^2) complexity here, but it guarantees the cells get their adjacents
    void SetAdj()
    {
        for (int x = 0; x < size.x; ++x)
        {
            for (int z = 0; z < size.z; ++z)
            { 
                Transform cell;
                cell = grid[x, z];
                CellScript cScript = cell.GetComponent<CellScript>();

                if (cScript.adj != null)
                {
                    cScript.adj.Clear();
                }

                if (x - 1 >= 0)
                {
                    cScript.adj.Add(grid[x - 1, z]);
                }
                
                
                if (x + 1 < size.x)
                {
                    cScript.adj.Add(grid[x + 1, z]);
                }

                if (z - 1 >= 0)
                {
                    cScript.adj.Add(grid[x, z - 1]);
                }

                if (z + 1 < size.z)
                {
                    cScript.adj.Add(grid[x, z + 1]);
                } 
            }
        }
    }
}
