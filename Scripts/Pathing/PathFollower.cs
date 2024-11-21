using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : Kinematic
{
    private Seek myMoveType; 
    private LookWhereGoing myRotateType;
    private ObstacleAvoidance avoid;

    public GameObject[] targets;

    [SerializeField]
    private int targetIndex = 0;

    public float waypointDetectRange;

    public Graph graph = new Graph();
    public Node startNode;
    public Node endNode;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new Seek();
        myMoveType.character = this;

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;

        avoid = new ObstacleAvoidance();
        avoid.character = this;
    }

    void FindPaths()
    {
        graph.Build();
        targets = getTargetList();
        Debug.Log("Targets: " + targets.Length);
        myMoveType.target = targets[targetIndex];
        avoid.target = targets[targetIndex];
        myTarget = targets[targetIndex];
        
    }

    // Update is called once per frame
    protected override void Update()
    {   
        if (Input.GetKeyDown("1"))
        {
            FindPaths();
        }

        if (targets.Length != 0 && Vector3.Distance(this.transform.position, targets[targetIndex].transform.position) > waypointDetectRange)
        {
            if (targetIndex < targets.Length - 1)
            {
                ++targetIndex;
            }

            myMoveType.target = targets[targetIndex];
            avoid.target = targets[targetIndex];
            myTarget = targets[targetIndex];
            steeringUpdate = new SteeringOutput();
            steeringUpdate.linear = avoid.getSteering().linear;

            if (avoid.bonkWall == false)
            {
                steeringUpdate.linear = myMoveType.getSteering().linear;
            }
        }

        base.Update();
    }

    private GameObject[] getTargetList()
    {
        GameObject[] targetList;
        List<Connection> connections = Dijkstra.pathfind(graph, startNode, endNode);
        Debug.Log("Connections: " + connections.Count); 
        targetList = new GameObject[connections.Count + 1];


        for (int i = 0; i < connections.Count; ++i)
        {
            targetList[i] = connections[i].getFromNode().gameObject;
        }

        targetList[0] = startNode.gameObject;
        targetList[connections.Count] = endNode.gameObject;

        return targetList;
    }
}
