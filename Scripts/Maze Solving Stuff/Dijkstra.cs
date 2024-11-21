using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dijkstra 
{
    //Dijkstra uses nodes (our cells) to find their connections and go about the maze with the lowest weighted path
    public class NodeRecord : IComparable<NodeRecord>
    {
        public Node node;
        public Connection connection;
        public int costSoFar;

        public int CompareTo(NodeRecord other)
        {
            //CompareTo returns a value that indicates the relative order of the objects being compared
            //if negative - this instance precedes other in sort order
            //if zero - occurs in the same position in sort order as other
            //if positive - follows other in sort order

            if (other == null)
            {
                return 1;
            }

            //we want to sort from lowest to highest cost
            //if our costSoFar < other, return negative value
            //if costSoFar == other, return 0
            //if costSoFar > other, return positive
            return (costSoFar - other.costSoFar);
        }
    }

    public class PathFindingList
    {
        // based on Millington "AI For Games" section 4.2.4, pp. 211-212, 
        //PathFinding List provides our:
        //Add,
        //Remove,
        //findSmallestElement,
        //and finds corresponding elements to a particular node
        //(Though Lists already have these functions built in...)
        List<NodeRecord> nodeRecords = new List<NodeRecord>();

        public void add(NodeRecord n)
        {
            nodeRecords.Add(n);
        }

        public void remove(NodeRecord n)
        {
            nodeRecords.Remove(n);
        }

        public NodeRecord smallestElement()
        {
            nodeRecords.Sort();
            return nodeRecords[0];
        }

        public int length()
        {
            return nodeRecords.Count;
        }

        public bool contains(Node node)
        {
            foreach (NodeRecord n in nodeRecords)
            {
                if (n.node == node)
                {
                    return true;
                }
            }
            return false;
        }

        public NodeRecord find(Node node)
        {
            foreach (NodeRecord n in nodeRecords)
            {
                if (n.node == node)
                {
                    return n;
                }
            }
            return null;
        }
    }

    public static List<Connection> pathfind(Graph g, Node start, Node goal)
    {
        //Initializing the nodeRecord for the startNode
        NodeRecord startRecord = new NodeRecord();
        startRecord.node = start;
        startRecord.connection = null;
        startRecord.costSoFar = 0;

        //Initializing open/closed lists, then adding our startRecord to the open list
        PathFindingList open = new PathFindingList();
        PathFindingList closed = new PathFindingList();
        open.add(startRecord);

        
        NodeRecord current = new NodeRecord();
        
        //Iterate through processing each node
        //Big O(n^2), I hate it here
        while (open.length() > 0)
        {
            current = open.smallestElement();

            //breaking if it is the goal node
            if (current.node == goal)
            {
                break;
            }

            //Otherwise, get its outgoing connections
            List<Connection> connections = g.getConnections(current.node);

            //Then loop through each connecion
            foreach (Connection c in connections)
            {
                Node endNode = c.getToNode();
                int endNodeCost = (int)current.costSoFar + (int)c.getCost();

                NodeRecord endNodeRecord = new NodeRecord();

                //skipping node if it is closed
                if (closed.contains(endNode))
                {
                    continue;
                } 
                //If node is open and we've found a worse route
                else if (open.contains(endNode))
                {
                    //Find record in open list corresponding to the endNode
                    endNodeRecord = open.find(endNode);

                    if (endNodeRecord != null && endNodeRecord.costSoFar < endNodeCost)
                    {
                        continue;
                    }
                }
                //If all that fails, we now know we've hit an unvisited node
                //So we'll make a record for it
                else
                {
                    endNodeRecord = new NodeRecord();
                    endNodeRecord.node = endNode;
                }

                //No matter what, we're here to update node records
                //Update cost and connection
                endNodeRecord.costSoFar = endNodeCost;
                endNodeRecord.connection = c;

                //Then add it to the open list
                if (!open.contains(endNode))
                {
                    open.add(endNodeRecord);
                }
            }
            //Outside of the foreach loop, we're done looking at connections for current
            //Add it to the closed list, remove it from open
            open.remove(current);
            closed.add(current);
        }
        //Outside the while loop, two possibilities exist:
        //we've found the goal, or
        //no more nodes exist to search
        //Find out which one it is
        if (current.node != goal)
        {
            //If we're here, it means we've run out of nodes without finding the goal
            //Which means there are no solutions :( Big sad, git gud 
            Debug.Log("No Path exists connecting start and end");
            return null;
        }
        else
        {
            //Compile the list of connections in this path
            List<Connection> path = new List<Connection>();

            //work back along the path, accumulating connections
            while (current.node != start)
            {
                path.Add(current.connection);

                Node fromNode = current.connection.getFromNode();
                current = closed.find(fromNode);
            }

            //reverse the path and return it
            path.Reverse();
            return path;
        }
    }
}
