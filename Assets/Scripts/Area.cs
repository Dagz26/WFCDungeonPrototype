using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area
{
    public enum AttrType
    {
        TileProb, TileGuarantee, TilePlace, TileAdjacency
    };

    public class TileAttribute
    {
        public AttrType type;
        public string[] names; //name of the tile type
        public int[] values;
        public bool flag;
        public TileAttribute(AttrType _type, string[] _names, int[] _values, bool _flag)
        {
            type = _type;
            names = _names;
            values = _values;
            flag = _flag;
        }
    }
    public enum AreaType
    {
        Labyrinth, Garden, Islands, Town, Ruins //Add more when tiles available
    };
    AreaType areaType;
    public List<Atom> nodes;
    public List<Area> children;
    public Area parent;
    public List<TileAttribute> attributes;
    //More stuff here

    public Area (AreaType _type)
    {
        parent = null;
        this.areaType = _type;
        nodes = new List<Atom>();
        nodes.Add(new Atom(this));
        children = new List<Area>();
        attributes = new List<TileAttribute>();
    }

    public void AddChild(int[] _nodes)
    {
        Area child = new Area(this.areaType);
        child.parent = this;
        children.Add(child);
        child.nodes[0] = this.nodes[_nodes[0]];
        for (int i = 1; i < _nodes.Length; i++)
        {
            child.nodes.Add(this.nodes[_nodes[i]]);
            this.nodes[_nodes[i]].AddArea(child);
        }
    }

    public void AddAttribute(AttrType _type, string[] _name, bool flag)
    {
        TileAttribute attribute = new TileAttribute(_type, _name, new int[] { }, flag);

        attributes.Add(attribute);
    }

    public void AddAttribute(AttrType _type, string[] _name, int[] _values, bool flag)
    {
        TileAttribute attribute = new TileAttribute(_type, _name, _values, flag);

        attributes.Add(attribute);
    }


    public bool IsEmpty()
    {
        if (nodes.Count > 0)
        {
            return false;
        }
        return true;
    }

    public void AddNode(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            nodes.Add(new Atom(this));
        }
    }

    public void AddEdge(int n1, int n2, int dir, Atom.type type) //dir from n1 to n2
    {
        if (n1 >= nodes.Count || n2 >= nodes.Count || n1 < 0 || n2 < 0 || n1 == n2)
        {
            return;
        }
        //connections are symmetrical
        nodes[n1].AddConnection(dir, nodes[n2], type);
        nodes[n2].AddConnection((dir + 2) % 4, nodes[n1], type);
    }

    public AreaType GetAreaType()
    {
        return areaType;
    }

    public static AreaType GetAreaType(int index)
    {
        switch(index)
        {
            case 0:
                return AreaType.Labyrinth;
            case 1:
                return AreaType.Garden;
            case 2:
                return AreaType.Islands;
            case 3:
                return AreaType.Town;
            case 4:
                return AreaType.Ruins;
            default:
                return AreaType.Labyrinth;
        }
    }

    public void AssignTileAttributes()
    {
        foreach (TileAttribute attr in attributes)
        {
            switch(attr.type)
            {
                case AttrType.TileProb: //It is applied to all nodes in the area
                    foreach(Atom node in nodes)
                    {
                        node.AddAtribute(attr);
                    }
                    break;
                case AttrType.TileGuarantee: //select random nodes in the area and add 1 to each with random coordinates
                    int amount = Random.Range(attr.values[0], attr.values[1]);
                    Debug.Log("Amount: " + amount);
                    for (int i = 0; i < amount; i++)
                    {

                        int random = Random.Range(0, nodes.Count);
                        Debug.Log("Random: " + random);
                        nodes[random].AddAtribute(new TileAttribute(AttrType.TilePlace, attr.names, new int[] { 1, -1}, attr.flag));
                    }
                    break;
                case AttrType.TilePlace:
                    if (attr.values[0] < nodes.Count)
                    {
                        nodes[attr.values[0]].AddAtribute(attr);
                    }
                    break;
                case AttrType.TileAdjacency:
                    foreach (Atom node in nodes)
                    {
                        node.AddAtribute(attr);
                    }
                    break;
            }
        }
        foreach (Area child in children)
        {
            child.AssignTileAttributes();
        }
    }
}
