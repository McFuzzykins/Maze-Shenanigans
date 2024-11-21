using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> ConnectsTo = new List<Node>();
    
    public void AddConnectTo()
    {
        CellScript cScript = this.GetComponentInParent<CellScript>();

        foreach (Transform connectsTo in cScript.adj)
        {
            if (cScript.isWall == false)
            {
                ConnectsTo.Add(connectsTo.GetComponentInParent<Node>());
                Debug.Log("Node " + connectsTo.name + "Added to " + cScript.name);
            }
        }
    }
    /*
    private void OnDrawGizmos()
    {
        foreach (Node n in ConnectsTo)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawRay(transform.position, (n.transform.position - transform.position).normalized * 2);
        }
    }
    */
}
