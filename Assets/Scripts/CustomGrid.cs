using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrid<T> : MonoBehaviour
{
    private int height;
    private int width;
    private float cellSize;
    private T[,] gridArray; //for now int, change to sprite or custom class later
    private Tile[,] tileArray;
    private TextMesh[,] debugText;
    private bool[,,] wave;
    public bool[,] observed;


    public CustomGrid(int width, int height, float cellSize)
    {
        this.height = height;
        this.width = width;
        this.cellSize = cellSize;
        gridArray = new T[width, height];
        debugText = new TextMesh[width, height];
        tileArray = new Tile[width, height];
        observed = new bool[width, height];
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //Debug.Log("Tile: " + i + ", " + j);
                //debugText[i, j] = WFCManager.createWorldText(gridArray[i, j].ToString(), null, getWorldPosition(i, j) + new Vector3(cellSize, cellSize) * 0.5f);
                Debug.DrawLine(getWorldPosition(i, j), getWorldPosition(i, j + 1), Color.white, 100f);
                Debug.DrawLine(getWorldPosition(i, j), getWorldPosition(i + 1, j), Color.white, 100f);
                observed[i, j] = false;
            }
        }
        //setValue(1, 1, 42);
        Debug.DrawLine(getWorldPosition(width, 0), getWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(getWorldPosition(0, height), getWorldPosition(width, height), Color.white, 100f);


    }

    private Vector3 getWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

    public void setValue(int x, int y, T value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            //debugText[x, y].text = value.ToString();
        }
    }

    public void setTile(int x, int y, Tile value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            if (tileArray[x, y] == null )
            {
                tileArray[x, y] = value;
                Instantiate(tileArray[x, y], getWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, tileArray[x, y].GetRotation());
                
            } else
            {
                Destroy(tileArray[x, y]);
                tileArray[x, y] = value;
                Instantiate(tileArray[x, y], getWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, tileArray[x, y].GetRotation());
            }

        }
    }

    public void setValue(Vector3 worldPosition, T value)
    {
        int x, y;
        getGridPosition(worldPosition, out x, out y);
        setValue(x, y, value);
    }

    private void getGridPosition(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y / cellSize);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
