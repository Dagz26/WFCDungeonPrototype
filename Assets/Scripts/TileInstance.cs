using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInstance
{
    public Tile prefab;
    public int rotation;
    public Vector3[] connectors;
    public Color[,] connectorsRGB;
    public int areaID;
    public bool extra;
    //maybe add probability here as well

    public TileInstance(Tile prefab, int rotation)
    {
        this.prefab = prefab;
        this.rotation = rotation;
        connectors = new Vector3[4];
        connectorsRGB = new Color[4, 3];
        this.areaID = prefab.areaID;
        this.extra = prefab.extra;
        
        for (int i = 0; i < 4; i++)
        {
            connectors[i] = prefab.GetConnector((i + 3 * rotation) % 4);
            for (int j = 0; j < 3; j++)
            {
                connectorsRGB[i, j] = prefab.connectorsRGB[(i + 3 * rotation) % 4, j];
            }
            
        }
    }

    public Vector3 GetConnector(int direction)
    {
        direction %= 4;
        return connectors[direction];
    }

    public Vector3 GetConnectorReversed(int direction)
    {
        Vector3 result = new Vector3();
        result.x = connectors[direction % 4].z;
        result.y = connectors[direction % 4].y;
        result.z = connectors[direction % 4].x;

        return result;
    }

    public Color[] GetConnectorRGB(int direction)
    {
        Color[] result = new Color[3];
        direction = direction % 4;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = connectorsRGB[direction, i];
        }

        return result;
    }

    public Color[] GetConnectorRGBReversed(int direction)
    {
        Color[] result = new Color[3];
        direction = direction % 4;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = connectorsRGB[direction, 2 - i];
        }

        return result;
    }


}
