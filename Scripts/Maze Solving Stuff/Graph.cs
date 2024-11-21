using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph 
{
    //Graph is for Dijkstra's to use, CreateMaze only takes care of creating the maze (like its name implies)
    List<Connection> mConnections;

    public void Build()
    {
        mConnections = new List<Connection>();

        //Finds all nodes in the scene (Which are our cells we made in CreateMaze, just now using their Node script)
        Node[] nodes = GameObject.FindObjectsOfType<Node>();
        Debug.Log("Nodes: " + nodes.Length);

        //iterating through the nodes to:
        //grab their cost/weight,
        //create their connections
        //add them to our connections list
        foreach (Node fromNode in nodes)
        {
            Debug.Log("fromNode: " + fromNode.name);
            fromNode.AddConnectTo();

            foreach (Node toNode in fromNode.ConnectsTo)
            {
                Debug.Log("ToName: " + toNode.name);
                //Gets weight from CellScript
                int cost = toNode.GetComponentInParent<CellScript>().weight;
                Connection c = new Connection(cost, fromNode, toNode);
                mConnections.Add(c);
                Debug.Log("Connection " + toNode.name + "Added");
            }
        }
    }

    public List<Connection> getConnections(Node fromNode)
    {
        List<Connection> connections = new List<Connection>();
        foreach (Connection c in mConnections)
        {
            if (c.getFromNode() == fromNode)
            {
                connections.Add(c);
            }
        }
        return connections;
    }
}

public class Connection
{
    float cost;
    Node fromNode;
    Node toNode;

    public Connection(float cost, Node fromNode, Node toNode)
    {
        this.cost = cost;
        this.fromNode = fromNode;
        this.toNode = toNode;
    }

    public float getCost()
    {
        return cost;
    }

    public Node getFromNode()
    {
        return fromNode;
    }

    public Node getToNode()
    {
        return toNode;
    }
}
