using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atom
{
    Atom[] connections;
    public enum type { Wall, Open, Door, LockedDoor };
    type[] connectionType;
    List<Area> areas;
    List<Area.TileAttribute> attributes;

    public Atom(Area _area)
    {
        connections = new Atom[4];
        connectionType = new type[4];
        for (int i = 0; i < 0; i++)
        {
            connections[i] = null;
            connectionType[i] = type.Wall;
        }
        areas = new List<Area>();
        areas.Add(_area);
        attributes = new List<Area.TileAttribute>();
    }

    public void AddConnection(int direction, Atom node, type type)
    {
        direction = direction % 4;
        connections[direction] = node;
        connectionType[direction] = type;
    }

    public void AddArea(Area area)
    {
        areas.Add(area);
    }

    public int GetConnectionAmount()
    {
        int counter = 0;
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] != null)
            {
                counter++;
            }
        }
        return counter;
    }

    public Atom GetConnection(int dir)
    {
        dir = dir % 4;
        if (connections[dir] != null)
        {
            return connections[dir];
        } else
        {
            return null;
        }
    }

    public type GetConnectionType(int dir)
    {
        return connectionType[dir];
    }

    public void getBordersAndTypes(out bool[] borders, out bool[] doors)
    {
        borders = new bool[4];
        doors = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            type current = connectionType[i];
            switch(current)
            {
                case type.Door:
                    borders[i] = true;
                    doors[i] = true;
                    break;
                case type.Wall:
                    borders[i] = true;
                    doors[i] = false;
                    break;
                case type.Open:
                    borders[i] = false;
                    doors[i] = false;
                    break;
                case type.LockedDoor:
                    borders[i] = true;
                    doors[i] = true;
                    break;
            }
        }

    }

    public Area.AreaType GetAreaType()
    {
        return areas[0].GetAreaType();
    }

    public void AddAtribute(Area.TileAttribute attr)
    {
        attributes.Add(attr);
    }

    public List<Area.TileAttribute> GetAttributes()
    {
        return attributes;
    }
}
