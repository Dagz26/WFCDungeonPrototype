using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isRotatable;
    private Quaternion rotationQuat = Quaternion.identity;
    private int rotation = 0; //0 is default, 1 is 90, ...
    //public int[,] connectors;
    public Vector3[] connectors = new Vector3[4];
    public bool[] rotatable; //if entry set to true, then a index * 90 rotation will make a unique tile
                             //false means the new rotation is identical to a previous 
    public int rotAmount;

    public int decoID; //Used for determining which decoration tiles can be placed on top of it
    public bool isInitiated;
    public int areaID;
    public bool extra;

    public Color[,] connectorsRGB; //Make it so that EVERYTHING works with RGB instead


    public Tile DuplicateTile(int direction)
    {
        Tile duplicate = this;
        
        return duplicate;
    }

    
    public bool TestHash()
    {
        SpriteRenderer spriteR = this.gameObject.GetComponent<SpriteRenderer>();
        //Texture2D tex = spriteR.sprite.texture;
        //Color color = tex.GetPixel(0, 0);
       

        int hash1 = spriteR.GetHashCode();
        spriteR.flipX = false;
        int hash2 = spriteR.GetHashCode();
        return hash1 == hash2;
    }


    public float GetSizeSingle()
    {
        SpriteRenderer spriteR = this.gameObject.GetComponent<SpriteRenderer>();
        float size = spriteR.bounds.size.x;
        return size;
    }

    public float GetSizeSliced()
    {
        SpriteRenderer spriteR = this.gameObject.GetComponent<SpriteRenderer>();
        float size = spriteR.size.x * transform.localScale.x;
        return size;
    }

    public void SetRotation(int direction)
    {
        direction = direction % 4;
        while(rotation != direction)
        {
            rotation = (rotation + 1) % 4;
            connectors = ShiftConnectors(connectors);
        }
        rotationQuat = Quaternion.Euler(0, 0, -90 * rotation);

    }

    public void RotateCounterClockwise()
    {
       
        rotation = (rotation + 1) % 4;
        rotationQuat = Quaternion.Euler(0, 0, 90 * rotation);
        //May need testing
        connectors = ShiftConnectors(connectors);
    }

    public Quaternion GetRotation()
    {
        return rotationQuat;
    }

    public Vector3 GetConnector(int direction)
    {
        return connectors[direction % 4];
    }

    public Vector3 GetConnectorReversed(int direction)
    {
        Vector3 result = new Vector3();
        result.x = connectors[direction % 4].z;
        result.y = connectors[direction % 4].y;
        result.z = connectors[direction % 4].x;

        return result;
    }

    public bool IsRotationUnique(int rotation)
    {
        //rotation = rotation % rotatable.Length;
        return rotatable[rotation];
    }

    private Vector3[] ShiftConnectors(Vector3[] connectors)
    {
        Vector3[] newConnectors = new Vector3[connectors.Length];
        for (int i = 0; i < connectors.Length; i++)
        {
            newConnectors[i] = connectors[(i + 1) % connectors.Length];
        }
        return newConnectors;
    }

    private Color[,] ShiftConnectorsRGB(Color[,] oldConnectorsRGB)
    {
        Color[,] newConnectorsRGB = new Color[4, 3];
        for (int i = 0; i < 4; i++)
        {

            for (int j = 0; j < 3; j++)
            {
                newConnectorsRGB[i, j] = oldConnectorsRGB[(i + 1) % 4, j];
            }
        }
        return newConnectorsRGB;
    }

    private bool ConnectorsEquals(Vector3[] c1, Vector3[] c2)
    {
        if (c1.Length != c2.Length)
        {
            return false;
        }
        for (int i = 0; i < c1.Length; i++)
        {
            if (c1[i] != c2[i])
            {
                return false;
            }
        }
        return true;
    }

    public static bool ConnectorsRGBEquals(Color[,] c1, Color[,] c2)
    {
        if (c1.Length != c2.Length)
        {
            return false;
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (!c1[i, j].Equals(c2[i, j]))
                {
                    return false;
                }
            }
        }
        return true;
    }


    
    private void InitConnectors()
    {
        SpriteRenderer renderer = this.gameObject.GetComponent<SpriteRenderer>();
        Texture2D tex = renderer.sprite.texture;
        Color color1 = tex.GetPixel(0, 0);
        Color color2 = tex.GetPixel(0, tex.height / 2);
        Color color3 = tex.GetPixel(0, tex.height);
        Color color4 = tex.GetPixel(tex.width / 2, tex.height);
        Color color5 = tex.GetPixel(tex.width, tex.height);
        Color color6 = tex.GetPixel(tex.width, tex.height / 2);
        Color color7 = tex.GetPixel(tex.width, 0);
        Color color8 = tex.GetPixel(tex.width / 2, 0);

        connectorsRGB[0, 0] = color1;
        connectorsRGB[0, 1] = color2;
        connectorsRGB[0, 2] = color3;

        connectorsRGB[1, 0] = color3;
        connectorsRGB[1, 1] = color4;
        connectorsRGB[1, 2] = color5;

        connectorsRGB[2, 0] = color5;
        connectorsRGB[2, 1] = color6;
        connectorsRGB[2, 2] = color7;

        connectorsRGB[3, 0] = color7;
        connectorsRGB[3, 1] = color8;
        connectorsRGB[3, 2] = color1;
        isInitiated = false;
    }




    public void Initialize()
    {
        connectorsRGB = new Color[4, 3];
        InitConnectors();
        rotation = 0;
        //Calculate rotations
        rotatable = new bool[4];
        //0 angle rotation is always unique
        rotatable[0] = true;
        //List<Vector3[]> unique = new List<Vector3[]>();
        //unique.Add(connectors);
        List<Color[,]> uniqueRGB = new List<Color[,]>();
        uniqueRGB.Add(connectorsRGB);
        rotAmount = 1;
        //Vector3[] current = ShiftConnectors(connectors);
        Color[,] currentRGB = ShiftConnectorsRGB(connectorsRGB);
        for (int i = 1; i < connectors.Length; i++)
        {
            bool isUnique = true;


            for(int j = 0; j < uniqueRGB.Count; j++)
            {
                if (ConnectorsRGBEquals(currentRGB, uniqueRGB[j]))
                {
                    isUnique = false;
                }
            }

            if (isUnique)
            {
                //unique.Add(current);
                uniqueRGB.Add(currentRGB);
                rotatable[i] = true;
                rotAmount++;
            }
            else
            {
                rotatable[i] = false;
            }
            //current = ShiftConnectors(current);
            currentRGB = ShiftConnectorsRGB(currentRGB);
        }
    }

    // Update is called once per framei
    void Update()
    {
        
    }


    
}
