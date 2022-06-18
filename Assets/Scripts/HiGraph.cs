using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiGraph
{
    public List<Area> areas;
    //Also information on how big the grid should be based on the amount of areas and nodes
    public int nodeWidth;
    public int nodeHeight;

    public HiGraph(int width, int height)
    {
        nodeHeight = height;
        nodeWidth = width;
        areas = new List<Area>();
    }

    public bool IsEmpty()
    {
        if (areas.Count > 0 && !areas[0].IsEmpty())
        {
            return false;
        }
        return true;
    }

    public void AddArea(Area.AreaType type)
    {
        areas.Add(new Area(type));
    }

    //Also need potential type of connection, door, locked door, wall
    public void ConnectAreas(int a1, int a2, int index1, int index2, int dir, Atom.type type)
    {
        areas[a1].nodes[index1].AddConnection(dir, areas[a2].nodes[index2], type);
        areas[a2].nodes[index2].AddConnection((dir + 2) % 4, areas[a1].nodes[index1], type);

    }

    public void AddEdgeToArea(int area, int n1, int n2, int dir, Atom.type type)
    {
        areas[area].AddEdge(n1, n2, dir, type);
    }

    public void DebugLog()
    {
        //Debug.Log("Higraph with node size: " +  nodeSize + ".");
        for (int i = 0; i < areas.Count; i++)
        {
            string Print = "[Area type: " + areas[i].GetAreaType() + ". [";
            for (int j = 0; j < areas[i].nodes.Count; j++)
            {
                Print += "Node " + j + "with connections [" + areas[i].nodes[j].GetConnectionAmount() + "] ";
            }
            Print += "].";
            Debug.Log(Print);
        }

    }


    //Assign TileAttributes to Nodes from all areas that they are in
    public void AssignTileAttributes()
    {
        foreach (Area area in areas)
        {
            area.AssignTileAttributes();
        }
    }
}
