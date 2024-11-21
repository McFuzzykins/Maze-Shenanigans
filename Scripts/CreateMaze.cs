using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateMaze : MonoBehaviour
{
    public Transform cellPrefab;
    public Vector3 size = new Vector3(20, 0, 20);
    public Transform[,] grid;
    public bool isOn = true;

    public List<Transform> passages = new List<Transform>();
    public List<Transform> walls = new List<Transform>();

    public PathFollower dijkstra;

    //A few lists that will:
    //1. store all frontiers we find - Frontiers
    //2. store the frontiers of the frontiers we find (except the 2nd set of frontiers are Passages, not Walls) - frontierNeighbors
    //3. store the frontiers we've visited so we don't endlessly keep visiting cells. 
    //Note: I could probably just mark the cell in CellScript as Visited with a bool, but I was running on Day 2 of nothing but this code. 
    List<Transform> Frontiers = new List<Transform>();
    List<Transform> frontierNeighbors = new List<Transform>();
    List<Transform> visitedCells = new List<Transform>();

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
            Reset();
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
    //Creates grid of GameObjects with their respective values, then adds them to our grid array
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
    //gives each cell a weight for the pathfinding algorithm to use later
    void RandomNumbers()
    {
        //Looping through each cell in our grid
        foreach (Transform child in transform)
        {
            int weight = Random.Range(0, 100);

            //setting the weight on each cell so it actually means something
            child.GetComponent<CellScript>().weight = weight;

            //visualizing the weights so I don't go insane later
            child.GetComponentInChildren<TMP_Text>().text = child.GetComponent<CellScript>().weight.ToString();
        }
    }

    //Function to call when a cell state needs to change while creating the graph
    void SetCellState(Transform cell, List<Transform> destination)
    {
        if (destination == passages)
        {
            cell.GetComponent<CellScript>().isWall = false;
            cell.GetComponent<MeshRenderer>().enabled = false;
            cell.GetComponent<BoxCollider>().enabled = false;
            cell.GetComponentInChildren<TMP_Text>().enabled = true;
            passages.Add(cell);

            if (walls.Contains(cell))
            {
                walls.Remove(cell);
            }
        }
        else if (destination == walls)
        {
            cell.GetComponent<CellScript>().isWall = true;
            cell.GetComponent<CellScript>().weight = 101;
            cell.GetComponent<MeshRenderer>().enabled = true;
            cell.GetComponent<BoxCollider>().enabled = true;
            cell.GetComponentInChildren<TMP_Text>().enabled = false;
            walls.Add(cell);

            if (passages.Contains(cell))
            {
                passages.Remove(cell);
            }
        }
    }

    //adds frontiers || neighbors to the respective list of whatever cell we're looking at
    void FrontierOrNeighbor(Transform cell, bool wallState)
    {
        if (wallState == true)
        {
            if ((int)(cell.transform.position.x + 2) <= size.x - 1 &&
                grid[(int)cell.transform.position.x + 2, (int)cell.transform.position.z].GetComponent<CellScript>().isWall == wallState)
            {
                //I added the cells to each list by referencing the grid placement of it instead of using cell
                //because I didn't want to risk referencing cell and having a reference to the transform and not the actual thing
                //cell.GetComponent<CellScript>().frontiers.Add(grid[(int)cell.transform.position.x + 2, (int)cell.transform.position.z]);
                Frontiers.Add(grid[(int)cell.transform.position.x + 2, (int)cell.transform.position.z]);

                //Adding to walls for the Maze Solver to reference later
                SetCellState(grid[(int)cell.transform.position.x + 2, (int)cell.transform.position.z], walls);
            }

            if ((int)(cell.transform.position.x - 2) >= 0 &&
                grid[(int)cell.transform.position.x - 2, (int)cell.transform.position.z].GetComponent<CellScript>().isWall == wallState)
            {
                //cell.GetComponent<CellScript>().frontiers.Add(grid[(int)cell.transform.position.x - 2, (int)cell.transform.position.z]);
                Frontiers.Add(grid[(int)cell.transform.position.x - 2, (int)cell.transform.position.z]);
                SetCellState(grid[(int)cell.transform.position.x - 2, (int)cell.transform.position.z], walls);
            }

            if ((int)(cell.transform.position.z + 2) <= size.z - 1 &&
                grid[(int)cell.transform.position.x, (int)cell.transform.position.z + 2].GetComponent<CellScript>().isWall == wallState)
            {
                //cell.GetComponent<CellScript>().frontiers.Add(grid[(int)cell.transform.position.x, (int)cell.transform.position.z + 2]);
                Frontiers.Add(grid[(int)cell.transform.position.x, (int)cell.transform.position.z + 2]);
                SetCellState(grid[(int)cell.transform.position.x, (int)cell.transform.position.z + 2], walls);
            }

            if ((int)(cell.transform.position.z - 2) >= 0 &&
                grid[(int)cell.transform.position.x, (int)cell.transform.position.z - 2].GetComponent<CellScript>().isWall == wallState)
            {
                //cell.GetComponent<CellScript>().frontiers.Add(grid[(int)cell.transform.position.x, (int)cell.transform.position.z - 2]);
                Frontiers.Add(grid[(int)cell.transform.position.x, (int)cell.transform.position.z - 2]);
                SetCellState(grid[(int)cell.transform.position.x, (int)cell.transform.position.z - 2], walls);
            }
        }
        else
        {
            //wallState == false means it's a passage, add it to the cell's neighbor list and out current frontierNeighbor list
            if ((int)(cell.transform.position.x + 2) <= size.x - 1 &&
                grid[(int)cell.transform.position.x + 2, (int)cell.transform.position.z].GetComponent<CellScript>().isWall == wallState)
            {
                //cell.GetComponent<CellScript>().neighbors.Add(grid[(int)cell.transform.position.x + 2, (int)cell.transform.position.z]);
                frontierNeighbors.Add(grid[(int)cell.transform.position.x + 2, (int)cell.transform.position.z]);
            }

            if ((int)(cell.transform.position.x - 2) >= 0 &&
                grid[(int)cell.transform.position.x - 2, (int)cell.transform.position.z].GetComponent<CellScript>().isWall == wallState)
            {
                //cell.GetComponent<CellScript>().neighbors.Add(grid[(int)cell.transform.position.x - 2, (int)cell.transform.position.z]);
                frontierNeighbors.Add(grid[(int)cell.transform.position.x - 2, (int)cell.transform.position.z]);
            }

            if ((int)(cell.transform.position.z + 2) <= size.z - 1 &&
                grid[(int)cell.transform.position.x, (int)cell.transform.position.z + 2].GetComponent<CellScript>().isWall == wallState)
            {
                //cell.GetComponent<CellScript>().neighbors.Add(grid[(int)cell.transform.position.x, (int)cell.transform.position.z + 2]);
                frontierNeighbors.Add(grid[(int)cell.transform.position.x, (int)cell.transform.position.z + 2]);
            }

            if ((int)(cell.transform.position.z - 2) >= 0 &&
                grid[(int)cell.transform.position.x, (int)cell.transform.position.z - 2].GetComponent<CellScript>().isWall == wallState)
            {
                //cell.GetComponent<CellScript>().neighbors.Add(grid[(int)cell.transform.position.x, (int)cell.transform.position.z - 2]);
                frontierNeighbors.Add(grid[(int)cell.transform.position.x, (int)cell.transform.position.z - 2]);
            }
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

    void PrimThisAlgorithm()
    {
        //Initializing enter and exit to use later
        Transform enter = grid[0, 0];
        Transform exit = grid[(int)size.x - 1, 0];

        //some randomization to help choose a starting cell
        int randX = (int)Random.Range(0, size.x);
        int randZ = (int)Random.Range(0, size.z);

        //Choose random starting cell
        Transform start = grid[randX, randZ];
        //Debug.Log("Start is cell: " + start.name);

        //frontier cell similar to cursors in LinkedLists 
        //It's a surprise tool that'll help us later
        Transform frontier;

        //Helps with resets while the program is running
        passages.Clear();
        walls.Clear();
        visitedCells.Clear();
        Frontiers.Clear();
        frontierNeighbors.Clear();

        //Add that cell to the Passage list, then make it a passage in Unity (Essentially turning it all off)
        SetCellState(start, passages);
        

        //Add all distance 2 cells to start's frontiers list and our master Frontier list
        FrontierOrNeighbor(start, true);

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
            //Debug.Log("frontier is: " + frontier);

            //check neighbors (frontiers of chosen frontier cell) to see if any of them are passages
            //if any are passages, add them to our frontierNeighbors list
            FrontierOrNeighbor(frontier, false);

            //Debug.Log("frontierNeighbors Count: " + frontierNeighbors.Count);

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
            randNeighbor = frontierNeighbors[Random.Range(0, frontierNeighbors.Count - 1)];

            //connection = frontier.GetComponent<CellScript>().adj.Contains();
            //Deciphering which cell is the adj of both randNeighbor && frontier (which one is inbetween them), then making it a passage 
            if ((int)randNeighbor.transform.position.x + 2 <= size.x - 1 &&
                grid[(int)randNeighbor.transform.position.x + 2, (int)randNeighbor.transform.position.z] ==
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z] &&
                grid[(int)randNeighbor.transform.position.x + 2, (int)randNeighbor.transform.position.z].GetComponent<CellScript>().isWall == true)
            {
                connection = grid[(int)randNeighbor.transform.position.x + 1, (int)randNeighbor.transform.position.z];
                SetCellState(connection, passages);
            }
            else if ((int)randNeighbor.transform.position.x - 2 >= 0 &&
                grid[(int)randNeighbor.transform.position.x - 2, (int)randNeighbor.transform.position.z] ==
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z] &&
                grid[(int)randNeighbor.transform.position.x - 2, (int)randNeighbor.transform.position.z].GetComponent<CellScript>().isWall == true)
            {
                connection = grid[(int)randNeighbor.transform.position.x - 1, (int)randNeighbor.transform.position.z];
                SetCellState(connection, passages);
            }
            else if ((int)randNeighbor.transform.position.z + 2 <= size.z - 1 &&
                grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z + 2] ==
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z] &&
                grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z + 2].GetComponent<CellScript>().isWall == true)
            {
                connection = grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z + 1];
                SetCellState(connection, passages);
            }
            else if ((int)randNeighbor.transform.position.z - 2 >= 0 &&
                grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z - 2] ==
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z] &&
                grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z - 2].GetComponent<CellScript>().isWall == true)
            {
                connection = grid[(int)randNeighbor.transform.position.x, (int)randNeighbor.transform.position.z - 1];
                SetCellState(connection, passages);
            }

            //adding the frontiers of the current frontier to our Frontier list
            if ((int)frontier.transform.position.x + 2 <= size.x - 1 && !visitedCells.Contains(grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z]) &&
                grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z].GetComponent<CellScript>().isWall == true)
            {
                //frontier.GetComponent<CellScript>().frontiers.Add(grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z]);
                Frontiers.Add(grid[(int)frontier.transform.position.x + 2, (int)frontier.transform.position.z]);
            }

            if ((int)frontier.transform.position.x - 2 >= 0 && !visitedCells.Contains(grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z]) &&
                grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z].GetComponent<CellScript>().isWall == true)
            {
                //frontier.GetComponent<CellScript>().frontiers.Add(grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z]);
                Frontiers.Add(grid[(int)frontier.transform.position.x - 2, (int)frontier.transform.position.z]);
            }

            if ((int)frontier.transform.position.z + 2 <= size.x - 1 && !visitedCells.Contains(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2]) &&
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2].GetComponent<CellScript>().isWall == true)
            {
                //frontier.GetComponent<CellScript>().frontiers.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2]);
                Frontiers.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z + 2]);
            }

            if ((int)frontier.transform.position.z - 2 >= 0 && !visitedCells.Contains(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2]) &&
                grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2].GetComponent<CellScript>().isWall == true)
            {
                //frontier.GetComponent<CellScript>().frontiers.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2]);
                Frontiers.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z - 2]);
            }

            //removing current frontier since we have done what we need with it
            //Debug.Log("Frontiers List Count: " + Frontiers.Count);
            Frontiers.Remove(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z]);

            //Nothing I found says this, but I found that if you don't set the removed frontier as a passage
            //then you'll only get the start cell and its adjacent cells as passages, at most
            //so I add the frontier we just used as a passage (maybe I should add a coin flip to determine if we do that?)
            SetCellState(frontier, passages);

            //add the frontier we just got rid of to the visitedCells list (The Trashcan)
            visitedCells.Add(grid[(int)frontier.transform.position.x, (int)frontier.transform.position.z]);
            //Debug.Log("Frontiers List Count: " + Frontiers.Count);
        }

        //This was my bs way of ensuring the outer border of the grid is guaranteed to be there
        //100% don't want to keep it, currently working on fixing the algorithm to not be able to select the border

        //initializing the minWeights to 101 as a way to ensure SOMETHING lower than their initial value is found
        //as every weight is randomized from 1 - 100
        int minWeightEnter = 101;
        int minWeightExit = 101;
        for (int i = 0; i < size.x; ++i)
        {
            //Excludes corners [0, 0] && [0, 19]
            if (grid[0, i].GetComponent<CellScript>().weight < minWeightEnter &&
                i != 0 && i != size.z - 1)
            {
                minWeightEnter = grid[0, i].GetComponent<CellScript>().weight;
                enter = grid[0, i];
            }

            //left border
            SetCellState(grid[0, i], walls);

            //Bottom Border
            SetCellState(grid[i, 0], walls);

            //Excludes corners [19, 0] && [19, 19]
            if (grid[(int)size.x - 1, i].GetComponent<CellScript>().weight < minWeightExit &&
                i != size.z - 1 && i != 0)
            {
                minWeightExit = grid[(int)size.x - 1, i].GetComponent<CellScript>().weight;
                exit = grid[(int)size.x - 1, i];
            }

            //Right Border
            SetCellState(grid[(int)size.x - 1, i], walls);

            //Top Border
            SetCellState(grid[i, (int)size.z - 1], walls);
        }

        //Setting the Entrance of the Maze
        SetCellState(grid[(int)enter.transform.position.x, (int)enter.transform.position.z], passages);


        //Making sure the entrance isn't immediately blocked by a wall
        if (enter.GetComponent<CellScript>().adj.Contains(grid[(int)enter.transform.position.x + 1, (int)enter.transform.position.z]))
        {
            SetCellState(grid[(int)enter.transform.position.x + 1, (int)enter.transform.position.z], passages);
        }

        //Setting the Exit of the Maze
        SetCellState(grid[(int)exit.transform.position.x, (int)exit.transform.position.z], passages);

        //Making sure the exit isn't blocked either
        if (exit.GetComponent<CellScript>().adj.Contains(grid[(int)exit.transform.position.x - 1, (int)exit.transform.position.z]))
        {
            SetCellState(grid[(int)exit.transform.position.x - 1, (int)exit.transform.position.z], passages);
        }

        foreach (Transform c in grid)
        {
            c.GetComponent<CellScript>().WeightToggle();
        }
        dijkstra.startNode = enter.GetComponentInParent<Node>();
        dijkstra.endNode = exit.GetComponentInParent<Node>();

    }

    void Reset()
    {
        foreach (Transform cell in grid)
        {
            //resetting the things that need to be reset
            cell.GetComponent<CellScript>().isWall = true;
            cell.GetComponent<MeshRenderer>().enabled = true;
            cell.GetComponent<BoxCollider>().enabled = true;
            cell.GetComponentInChildren<TMP_Text>().enabled = true;
        }

        RandomNumbers();
        PrimThisAlgorithm();
    }
}
