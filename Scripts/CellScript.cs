using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellScript : MonoBehaviour
{
    //I feel like this is almost a waste to have, but it aint supposed to do much ig
    public List<Transform> adj = new List<Transform>();
    public List<Transform> frontiers = new List<Transform>();
    public List<Transform> neighbors = new List<Transform>();
    public Vector3 pos = new Vector3(0, 0, 0);
    public int weight = 0;
    public bool isWall = true;

    void Awake()
    {
  
    }
}
