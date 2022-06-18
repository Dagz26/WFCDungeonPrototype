using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ***TODO***


//Steps to consider:
//Write the actual paper
//Read and parse actual graphs as inputs
public class WFCManager : MonoBehaviour
{
    CustomGrid<int> grid;
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    private Camera camera;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    public List<Tile> prefabSet; // The list of all tile prefabs
    private List<TileInstance> tileSet;
    public List<Tile> prefabDecoSet;
    private List<TileInstance> tileDecoSet; //WORK WITH THESE NEXT
    private int[,,] weights;
    private int[] weightsDeco;
    private AdjacencyModel model; //General model holding pixel matched adjacencies
    //Every grid cell has their own of these models
    private AdjacencyModel[,] exclusionModel; //Makes it so that tiles can't be placed together even if they match with pixels
    private AdjacencyModel[,] inclusionModel; // Extra model for adding Adjacencies 
    private bool[,,] wave;
    private bool[,,] waveDeco;
    //amount of probabilities remaining for that tile
    private int[,] unobserved;
    private int[,] unobservedDeco;
    private int[,] set; //set to -1, replaced with the index of the tile when the cell is fully observed 
    private int[,] setDeco;
    private bool finished;
    private bool finishedDeco;
    private int setTiles;
    private int setTilesDeco;
    public int width = 5;
    public int height = 5;
    private HiGraph hiGraph;
    private bool done = false;
    
    List<int>[,] localTileSet;
    List<bool>[,] newWave;
    private List<TileInstance>[] tileInstances;
    private int areas;
    private int exclusionValue;
    //private List<TileInstance>[] tileInstances;

    //Stuff for graph reading
    public List<Vector2Int> edges;


    // Start is called before the first frame update
    void Start()
    {
        exclusionValue = 1;
        //Create test graph
        GenerateGraph(10, 10);
        //hiGraph.DebugLog();
        Vector2Int start = ReadGraph();

        finished = false;
        setTiles = 0;

        tileSet = new List<TileInstance>();
        tileDecoSet = new List<TileInstance>();
        


        //newWave = new bool[width, height][];
        //newWave[0, 0] = new bool[2];

        //Init TileInstances
        foreach (Tile tile in prefabSet) //Define all others with directions in model, then simply add rotation with same adjacency but inverse direction
                                         //Also adjaceny between rotations
        {
            
            tile.Initialize();
            
            tile.SetRotation(0);
            if (tile.rotatable.Length > 0)
            {
                //Debug.Log(tile.rotatable.Length);
                for (int i = 0; i < 4; i++)
                {
                    //Debug.Log(tile.IsRotationUnique(i));
                    if (tile.IsRotationUnique(i))
                    {

                        TileInstance tileI = new TileInstance(tile, i);
                        tileSet.Add(tileI);
                        //Debug.Log("Instance of Tile " + prefabSet.IndexOf(tile) + " added with rotation " + i);
                    }
                }
            }
        }


        foreach (Tile tile in prefabDecoSet)
        {
            tile.Initialize();
            tile.SetRotation(0);

            if (tile.rotatable.Length > 0)
            {
                //Debug.Log(tile.rotatable.Length);
                for (int i = 0; i < 4; i++)
                {
                    //Debug.Log(tile.IsRotationUnique(i));
                    if (tile.IsRotationUnique(i))
                    {

                        TileInstance tileI = new TileInstance(tile, i);
                        tileDecoSet.Add(tileI);
                        //Debug.Log("Instance of Tile " + prefabSet.IndexOf(tile) + " added with rotation " + i);
                    }
                }
            }
        }


        model = new AdjacencyModel();
        exclusionModel = new AdjacencyModel[width, height];
        inclusionModel = new AdjacencyModel[width, height];
        grid = new CustomGrid<int>(width, height, prefabSet[0].GetSizeSingle());
        camera = Camera.main;
        wave = new bool[width, height, tileSet.Count];
        waveDeco = new bool[width, height, tileDecoSet.Count];
        unobserved = new int[width, height];
        unobservedDeco = new int[width, height];
        set = new int[width, height];
        setDeco = new int[width, height];
        weights = new int[width, height, tileSet.Count]; //Continue fixing
        weightsDeco = new int[tileDecoSet.Count];

        //Debug.Log("Amount of tiles possible: " + tileSet.Count);
        //prefabSet.IndexOf
        if (tileSet.Count > 0)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //Tile tile = testSet[counter++ % testSet.Count];
                    //grid.setTile(i, j, tile);
                    //tile.rotateCounterClockwise();
                    //initialize the wave
                    
                    for (int k = 0; k < tileSet.Count; k++)
                    {
                        wave[i, j, k] = false;
                        weights[i, j, k] = 100;
                    }
                    //unobserved[i, j] = tileSet.Count;
                    unobserved[i, j] = 0;
                    grid.setValue(i, j, unobserved[i, j]);
                    set[i, j] = -1;
                }
            }

            //Add rotations to the tile set
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    exclusionModel[x, y] = new AdjacencyModel();
                    inclusionModel[x, y] = new AdjacencyModel();
                    for (int i = 0; i < tileSet.Count; i++)
                    {
                        if (x == 0 && y == 0)
                            model.addTile();

                        exclusionModel[x,y].addTile();
                        inclusionModel[x,y].addTile();
                    }
                }
            }


            //Add Adjacency rules to the model
            for (int i = 0; i < tileSet.Count; i++)
            {
                
                for (int j = i; j < tileSet.Count; j++)
                {

                    //Check every direction
                    for (int k = 0; k < 4; k++)
                    {
                        if (tileSet[i].extra || tileSet[j].extra)
                        {
                            break; ;
                        }
                        if (CompareConnectorRGB(tileSet[i].GetConnectorRGB(k), tileSet[j].GetConnectorRGBReversed((k + 2) % 4)))
                        {
                            //Debug.Log("Adjacency succesfully added");
                            model.addAdjacency(i, k, j);

                            if (i != j)
                            {
                                model.addAdjacency(j, (k + 2) % 4, i);
                            }
                        }
                    }
                }
            }

            if (tileDecoSet.Count > 0)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        //Tile tile = testSet[counter++ % testSet.Count];
                        //grid.setTile(i, j, tile);
                        //tile.rotateCounterClockwise();
                        //initialize the wave
                        for (int k = 0; k < tileDecoSet.Count; k++)
                        {
                            waveDeco[i, j, k] = true;
                        }
                        unobservedDeco[i, j] = tileSet.Count;
                        //grid.setValue(i, j, unobserved[i, j]);
                        setDeco[i, j] = -1;
                    }
                }

                
                //Add rotations to the tile set
                for (int i = 0; i < tileDecoSet.Count; i++)
                {
                    model.addTile();
                }

                //Add Adjacency rules to the model
                for (int i = 0; i < tileDecoSet.Count; i++)
                {
                    weightsDeco[i] = 10;
                    for (int j = i; j < tileDecoSet.Count; j++)
                    {

                        //Check every direction
                        for (int k = 0; k < 4; k++)
                        {
                            
                            if (CompareConnectorRGB(tileDecoSet[i].GetConnectorRGB(k), tileDecoSet[j].GetConnectorRGBReversed((k + 2) % 4)))
                            {
                                //Debug.Log("Adjacency succesfully added");
                                model.addAdjacency(i + tileSet.Count, k, j + tileSet.Count);
                                if (i != j)
                                {
                                    model.addAdjacency(j + tileSet.Count, (k + 2) % 4, i + tileSet.Count);
                                }
                            }
                        }
                    }
                }
            }
            
            localTileSet = new List<int>[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    localTileSet[i, j] = new List<int>();
                }
            }
            
            DrawGraph(start);
            
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (localTileSet[i, j].Count <= 1)
                    {
                        AddWeight("Separator", 1, new Vector2Int(i, j), new Vector2Int(i + 1, j + 1), -1, true);
                        Propagate(i, j);
                        UpdateGrid();
                    }
                }
            }
            StartCoroutine(ObserveAnimation());



        }
    }

    /*
    public void AddExclusionAdjacency(string name1, string name2, int dir, int rot, Vector2Int start, Vector2Int end)
    {

        bool diff = !name1.Equals(name2);
        int i1 = getTileIndex(name1, 0);
        int i2, rotAmount2;
        int rotAmount1 = tileSet[i1].prefab.rotAmount;
        if (diff)
        {
            i2 = getTileIndex(name2, 0);
            rotAmount2 = tileSet[i1].prefab.rotAmount;
        } else
        {
            i2 = i1;
            rotAmount2 = rotAmount1;
        }
        Debug.Log("Tile 1 " + tileSet[i1].prefab.gameObject.name);

        
        for (int x = start.x; x < end.x; x++)
        {
            for (int y = start.y; y < end.y; y++)
            {
                for (int i = 0; i < rotAmount1; i++)
                {
                    exclusionModel[x, y].addAdjacency(i1 + i, (dir + i) % 4, i2 + ((rot + i) % 4));
                    
                    if (!exclusionModel[x, y].checkValid(i2 + ((rot + i) % 4), (dir + i + 2) % 4, i1 + i))
                    {
                        exclusionModel[x, y].addAdjacency(i2 + ((rot + i) % 4), (dir + i + 2) % 4, i1 + i);
                    }
                    
                }
            }
        }
        
    }
    */
    public void AddExclusionAdjacency(string name1, string name2, int dir, int rot, Vector2Int start, Vector2Int end)
    {

        bool diff = !name1.Equals(name2);
        int i1 = getTileIndex(name1, 0);
        int i2, rotAmount2;
        int rotAmount1 = tileSet[i1].prefab.rotAmount;
        if (diff)
        {
            i2 = getTileIndex(name2, 0);
            rotAmount2 = tileSet[i2].prefab.rotAmount;
        }
        else
        {
            i2 = i1;
            rotAmount2 = rotAmount1;
        }
        //Debug.Log("Tile 1 " + tileSet[i1].prefab.gameObject.name);
        //Debug.Log("i1 " + i1 + ", rotAmount1 " + rotAmount1);
        //Debug.Log("i2 " + i2 + ", rotAmount2 " + rotAmount2);

        for (int x = start.x; x < end.x; x++)
        {
            for (int y = start.y; y < end.y; y++)
            {
                for (int i = 0; i < 4; i++)
                {
                    //if (!inclusionModel[x, y].checkValid(i1 + i % rotAmount1, (dir + i) % 4, i2 + ((rot + i) % rotAmount2)))
                    //{
                    //Debug.Log("From: " + (i1 + (i % rotAmount1)) + ", dir: " + ((dir + i) % 4) + ", To: " + (i2 + ((rot + i) % rotAmount2)));
                    exclusionModel[x, y].addAdjacency(i1 + (i % rotAmount1), (dir + i) % 4, i2 + ((rot + i) % rotAmount2));
                    //}

                    //if (!inclusionModel[x, y].checkValid(i2 + ((rot + i) % rotAmount2), (dir + i + 2) % 4, i1 + i % rotAmount1))
                    //{
                    exclusionModel[x, y].addAdjacency(i2 + ((rot + i) % rotAmount2), (dir + i + 2) % 4, i1 + i % rotAmount1);
                    //}

                }
            }
        }

    }


    public void AddInclusionAdjacency(string name1, string name2, int dir, int rot, Vector2Int start, Vector2Int end)
    {

        bool diff = !name1.Equals(name2);
        int i1 = getTileIndex(name1, 0);
        int i2, rotAmount2;
        int rotAmount1 = tileSet[i1].prefab.rotAmount;
        if (diff)
        {
            i2 = getTileIndex(name2, 0);
            rotAmount2 = tileSet[i2].prefab.rotAmount;
        }
        else
        {
            i2 = i1;
            rotAmount2 = rotAmount1;
        }
        Debug.Log("Tile 1 " + tileSet[i1].prefab.gameObject.name);
        Debug.Log("Tile 2 " + tileSet[i2].prefab.gameObject.name);
        //Debug.Log("i1 " + i1 + ", rotAmount1 " + rotAmount1);
        //Debug.Log("i2 " + i2 + ", rotAmount2 " + rotAmount2);

        for (int x = start.x; x < end.x; x++)
        {
            for (int y = start.y; y < end.y; y++)
            {
                for (int i = 0; i < 4; i++)
                {
                    //if (!inclusionModel[x, y].checkValid(i1 + i % rotAmount1, (dir + i) % 4, i2 + ((rot + i) % rotAmount2)))
                    //{
                    //Debug.Log("From: " + (i1 + (i % rotAmount1)) + ", dir: " + ((dir + i) % 4) + ", To: " + (i2 + ((rot + i) % rotAmount2)));
                    inclusionModel[x, y].addAdjacency(i1 + (i % rotAmount1), (dir + i) % 4, i2 + ((rot + i) % rotAmount2));
                    //}

                    //if (!inclusionModel[x, y].checkValid(i2 + ((rot + i) % rotAmount2), (dir + i + 2) % 4, i1 + i % rotAmount1))
                    //{
                    inclusionModel[x, y].addAdjacency(i2 + ((rot + i) % rotAmount2), (dir + i + 2) % 4, i1 + i % rotAmount1);
                    //}

                }
            }
        }

    }


    public void AddWeight(string name, int weight, Vector2Int start, Vector2Int end, int rot, bool wave)
    {
        
        foreach (TileInstance tile in tileSet)
        {
            if (tile.prefab.gameObject.name.Contains(name))
            {
                int index = tileSet.IndexOf(tile);
                if (rot == -1 || rot == tileSet[index].rotation)
                {
                    AddWeight(index, weight, start, end, wave);

                }
                //Debug.Log(name);
            }
        }
    }

    public int GetFirstIndexOfTile(string name)
    {
        foreach (TileInstance tile in tileSet)
        {
            if (tile.prefab.gameObject.name.Contains(name))
            {
                int index = tileSet.IndexOf(tile);
                return index;
            }
        }
        return -1;
    }

    //Add check for out of bounds of grid
    private void AddBorders(int startX, int startY, int bWidth, int bHeight, bool[] borders, bool[] doors, int area)
    {
        int index = GetFirstIndexOfTile(Area.GetAreaType(area).ToString() + "Basic");
        //Debug.Log("index: " + index);
        AddWeight(Area.GetAreaType(area).ToString() + "Basic", 100, new Vector2Int(startX , startY ), 
            new Vector2Int(startX + bWidth , startY + bHeight), -1, true);
        AddWeight(Area.GetAreaType(area).ToString() + "BorderCorner2", 100, new Vector2Int(startX, startY),
            new Vector2Int(startX + 1, startY + 1), -1, true);
        AddWeight(Area.GetAreaType(area).ToString() + "BorderCorner2", 100, new Vector2Int(startX + bWidth- 1, startY),
            new Vector2Int(startX + bWidth, startY + 1), -1, true);
        AddWeight(Area.GetAreaType(area).ToString() + "BorderCorner2", 100, new Vector2Int(startX, startY + bHeight- 1),
            new Vector2Int(startX + 1, startY + bHeight), -1, true);
        AddWeight(Area.GetAreaType(area).ToString() + "BorderCorner2", 100, new Vector2Int(startX + bWidth - 1, startY + bHeight -1),
            new Vector2Int(startX + bWidth, startY + bHeight), -1, true);


        string areaName = Area.GetAreaType(area).ToString();
        

        if (borders[0])
        {
            AddWeight("Door", 100, new Vector2Int(startX , startY),
                new Vector2Int(startX + 1, startY + bHeight), -1, true);
            AddWeight("Border", 100, new Vector2Int(startX , startY),
                new Vector2Int(startX + 1, startY + bHeight), -1, true);
        } if (borders[1])
        {
            AddWeight("Door", 100, new Vector2Int(startX, startY + bHeight- 1),
                new Vector2Int(startX + bWidth, startY + bHeight), -1, true);
            AddWeight("Border", 100, new Vector2Int(startX, startY + bHeight - 1),
                new Vector2Int(startX + bWidth, startY + bHeight), -1, true);
        } if (borders[2])
        {
            AddWeight("Door", 100, new Vector2Int(startX + bWidth - 1, startY),
                new Vector2Int(startX + bWidth, startY + bHeight), -1, true);
            AddWeight("Border", 100, new Vector2Int(startX + bWidth - 1, startY),
                new Vector2Int(startX + bWidth, startY + bHeight), -1, true);
        } if (borders[3])
        {
            AddWeight("Door", 100, new Vector2Int(startX, startY),
                new Vector2Int(startX + bWidth, startY + 1), -1, true);
            AddWeight("Border", 100, new Vector2Int(startX, startY),
                new Vector2Int(startX + bWidth, startY + 1), -1, true);
        }



        //Debug.Log("Unobserved:" + unobserved[startX, startY]);

        for (int i = 0; i < bWidth; i++)
        {
            
            if (i == 0)
            {
                if (borders[0] && borders[3])
                {
                } else if (borders[0])
                { //Wall 
                    Observe(i + startX, 0 + startY, getTileIndex(areaName + "BorderSide", 0));
                } else if (borders[3])
                {
                    Observe(i + startX, 0 + startY, getTileIndex(areaName + "BorderSide", 3));
                }

                if (borders[0] && borders[1])
                {
                    //Observe(i + startX, bHeight - 1 + startY, 0 + 8 * area);
                }
                else if (borders[0])
                { //Wall 
                    Observe(i + startX, bHeight - 1 + startY, getTileIndex(areaName + "BorderSide", 0));
                }
                else if (borders[1])
                {
                    Observe(i + startX, bHeight - 1 + startY, getTileIndex(areaName + "BorderSide", 1));
                }
            }
            else if (i == bWidth - 1)
            {
                if (borders[2] && borders[1])
                {
                    //Observe(i + startX, bHeight - 1 + startY, 1 + 8 * area);
                }
                else if (borders[2])
                { //Wall 
                    Observe(i + startX, bHeight - 1 + startY, getTileIndex(areaName + "BorderSide", 2));
                }
                else if (borders[1])
                {
                    Observe(i + startX, bHeight - 1 + startY, getTileIndex(areaName + "BorderSide", 1));
                }

                if (borders[2] && borders[3])
                {
                    //Observe(i + startX, 0 + startY, 2 + 8 * area);
                }
                else if (borders[2])
                { //Wall 
                    Observe(i + startX, 0 + startY, getTileIndex(areaName + "BorderSide", 2));
                }
                else if (borders[3])
                {
                    Observe(i + startX, 0 + startY, getTileIndex(areaName + "BorderSide", 3));
                }


            }
            else
            {
                if (borders[3])
                {
                    if (doors[3] && i == (bWidth - 1) / 2)
                    {
                        Observe(i + startX, 0 + startY, getTileIndex(areaName + "Door", 3));
                    }
                    else
                    {
                        Observe(i + startX, 0 + startY, getTileIndex(areaName + "BorderSide", 3));
                    }
                }
                if (borders[1])
                {
                    if (doors[1] && i == (bWidth - 1) / 2)
                    {
                        Observe(i + startX, bHeight - 1 + startY, getTileIndex(areaName + "Door", 1));
                    } else {
                        Observe(i + startX, bHeight - 1 + startY, getTileIndex(areaName + "BorderSide", 1));
                    }
                }
            }
            
            

            
        }
        for (int i = 1; i < bHeight - 1; i++)
        {

            if (borders[0])
            {
                if (doors[0] && i == (bHeight - 1) / 2)
                {
                    Observe(0 + startX, i + startY, getTileIndex(areaName + "Door", 0));
                } else
                {
                    Observe(0 + startX, i + startY, getTileIndex(areaName + "BorderSide", 0));
                }
            }
            if (borders[2])
            {
                if (doors[2] && i == (bHeight - 1) / 2)
                {
                    Observe(bWidth - 1 + startX, i + startY, getTileIndex(areaName + "Door", 2));
                } else
                {
                    Observe(bWidth - 1 + startX, i + startY, getTileIndex(areaName + "BorderSide", 2));
                }
            }
        }

    }

    //Modifies how likely a tile is to be randomly selected in relation to the other tiles
    public void AddWeight(int index, int weight, Vector2Int start, Vector2Int end, bool _wave)
    {
        if (index < 0 || index >= tileSet.Count)
        {
            return;
        }
        for (int i = start.x; i < end.x; i++)
        {
            for (int j = start.y; j < end.y; j++)
            {
                
                weights[i, j, index] = weight;
                if (_wave == false && weight <= 0)
                {
                    wave[i, j, index] = false;
                    unobserved[i, j]--;
                    if (localTileSet[i, j].Contains(index))
                        localTileSet[i, j].Remove(index);
                    //Propagate(i, j);
                    UpdateGrid();
                } else if (_wave == true && weight > 0 && !wave[i, j, index])
                {
                    wave[i, j, index] = _wave;
                    unobserved[i, j]++;
                    if (!localTileSet[i, j].Contains(index))
                        localTileSet[i, j].Add(index);
                    
                }
                else if (unobserved[i, j] <= 1)
                {
                    continue;
                }
            }
        }
        
    }

    private bool CompareConnectorRGB(Color[] c1, Color[] c2)
    {
        if (c1.Length != c2.Length)
        {
            return false;
        }

        for (int i = 0; i < c1.Length; i++)
        {
            if (!c1[i].Equals(c2[i]))
            {
                return false;
            }
        }
        return true;
    }

    private void RenderTile(int x, int y)
    {
        //check tile fully observed, might check before invoking method instead
        int index = 0;
        if (unobserved[x, y] == 1)
        {
            for (int i = 0; i < tileSet.Count; i++)
            {
                if (wave[x, y, i])
                {
                    index = i;
                    break;
                }
            }

            
                //Debug.Log("Tile with index" + index + "with rotation " + tileSet[index].rotation + " instanced");
                Tile tile = tileSet[index].prefab;
                tile.SetRotation(tileSet[index].rotation);
                grid.setTile(x, y, tile);
                tile.SetRotation(0); //Potentially useless
        }
    }

    void GenerateGraph(int cellWidth, int cellHeight)
    {
        //Make Example Graph here
        hiGraph = new HiGraph(cellWidth, cellHeight);
        hiGraph.AddArea(Area.AreaType.Labyrinth);
        hiGraph.AddArea(Area.AreaType.Garden);

        hiGraph.areas[0].AddNode(23);
        hiGraph.AddEdgeToArea(0, 0, 1, 2, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 0, 2, 3, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 3, 2, 0, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 3, 1, 1, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 3, 4, 3, Atom.type.Open);
        
        hiGraph.AddEdgeToArea(0, 4, 5, 2, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 5, 6, 2, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 5, 7, 3, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 8, 6, 1, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 8, 7, 0, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 6, 9, 2, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 9, 10, 1, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 10, 11, 1, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 10, 12, 2, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 13, 12, 3, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 13, 11, 0, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 11, 14, 1, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 14, 15, 0, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 15, 16, 0, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 15, 17, 1, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 18, 17, 2, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 18, 16, 3, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 16, 19, 0, Atom.type.Open);
        hiGraph.AddEdgeToArea(0, 19, 1, 3, Atom.type.Open);

        hiGraph.areas[0].AddChild(new int[] { 4, 9, 14, 19 });


        hiGraph.areas[1].AddNode(3);
        hiGraph.AddEdgeToArea(1, 0, 1, 2, Atom.type.Open);
        hiGraph.AddEdgeToArea(1, 0, 2, 1, Atom.type.Open);
        hiGraph.AddEdgeToArea(1, 3, 2, 0, Atom.type.Open);
        hiGraph.AddEdgeToArea(1, 3, 1, 3, Atom.type.Open);

        hiGraph.areas[0].AddAttribute(Area.AttrType.TileProb, new string[] { "ExtraEnemy" }, new int[] { 500, -1 }, true);
        hiGraph.areas[0].AddAttribute(Area.AttrType.TileProb, new string[] { "LabyrinthReverseCorner" }, new int[] { 50, -1 }, true);
        hiGraph.areas[0].children[0].AddAttribute(Area.AttrType.TileProb, new string[] { "LabyrinthPath" }, new int[] { 100, -1 }, true);
        hiGraph.areas[1].AddAttribute(Area.AttrType.TileProb, new string[] { "GardenWater" }, new int[] { 100, -1 }, true);
        hiGraph.areas[1].AddAttribute(Area.AttrType.TileProb, new string[] { "GardenPath" }, new int[] { 100, -1 }, true);
        hiGraph.areas[1].AddAttribute(Area.AttrType.TileProb, new string[] { "RuinsStructure" }, new int[] { 100, -1 }, true);
        hiGraph.areas[1].AddAttribute(Area.AttrType.TileProb, new string[] { "GardenDock" }, new int[] { 100, -1 }, true);
        hiGraph.areas[0].AddAttribute(Area.AttrType.TileAdjacency, new string[] { "ExtraEnemy", "LabyrinthBasic1" },
            new int[] { 1, 0, 0 }, true);
        hiGraph.areas[0].AddAttribute(Area.AttrType.TileAdjacency, new string[] { "ExtraEnemy", "LabyrinthBasic2" },
            new int[] { 1, 0, 2 }, true);
        hiGraph.areas[0].AddAttribute(Area.AttrType.TileAdjacency, new string[] { "ExtraEnemy", "ExtraEnemy" },
            new int[] { 1, 0, 0 }, false);
        hiGraph.areas[0].AddAttribute(Area.AttrType.TileAdjacency, new string[] { "LabyrinthBasic2", "LabyrinthBasic3" },
            new int[] { 1, 3, 0 }, false);
        hiGraph.areas[0].AddAttribute(Area.AttrType.TileAdjacency, new string[] { "LabyrinthBasic4", "LabyrinthBasic4" },
            new int[] { 1, 1, 3 }, false);
        hiGraph.areas[1].AddAttribute(Area.AttrType.TileAdjacency, new string[] { "GardenPath2", "GardenDoor" },
            new int[] { 1, 0, 0 }, true);
        

        hiGraph.ConnectAreas(0, 1, 3, 0, 2, Atom.type.Door);
        hiGraph.ConnectAreas(0, 1, 6, 1, 1, Atom.type.Door);
        hiGraph.ConnectAreas(0, 1, 11, 3, 0, Atom.type.Door);
        hiGraph.ConnectAreas(0, 1, 16, 2, 3, Atom.type.Door);
    }

    private Vector2Int ReadGraph()
    {
        //Determine the size of the grid
        Debug.Log("Read Graph");
        Vector2Int coord = new Vector2Int(0, 0);
        Vector2Int max = new Vector2Int(0, 0);
        Vector2Int min = new Vector2Int(0, 0);

        HashSet<Atom> visited = new HashSet<Atom>();
        if (hiGraph.IsEmpty())
        {
            Debug.Log("Graph is Empty :(");
            return new Vector2Int();
        }
        Atom current = hiGraph.areas[0].nodes[0];
        //Explore
        Explore(current, ref visited, coord, ref max, ref min);
        Debug.Log("Max:" + max);
        Debug.Log("Min:" + min);
        width = (max.x - min.x + 1) * hiGraph.nodeWidth;
        height = (max.y - min.y + 1) * hiGraph.nodeHeight;
        return new Vector2Int(Mathf.Abs(min.x), Mathf.Abs(min.y));
    }

    //Potentially check for validity here
    private void Explore(Atom current, ref HashSet<Atom> visited, Vector2Int coord, ref Vector2Int max, ref Vector2Int min)
    {
        visited.Add(current);
        for (int i = 0; i < 4; i++)
        {
            Atom next = current.GetConnection(i);
            if (next != null && !visited.Contains(next))
            {
                Vector2Int temp = coord;
                //new direction has been picked
                switch (i)
                {

                    case 0:
                        coord.x--;
                        if (coord.x < min.x)
                            min.x = coord.x;
                        break;
                    case 1:
                        coord.y++;
                        if (coord.y > max.y)
                            max.y = coord.y;
                        break;
                    case 2:
                        coord.x++;
                        if (coord.x > max.x)
                            max.x = coord.x;
                        break;
                    case 3:
                        coord.y--;
                        if (coord.y < min.y)
                            min.y = coord.y;
                        break;
                }
                Explore(next, ref visited, coord, ref max, ref min);
                coord = temp;
            }
        }
    }

    private void DrawGraph(Vector2Int start)
    {
        start.x *= hiGraph.nodeWidth;
        start.y *= hiGraph.nodeHeight;
        hiGraph.AssignTileAttributes();
        Atom origin = hiGraph.areas[0].nodes[0];
        HashSet<Atom> visited = new HashSet<Atom>();
        DrawNode(origin, ref visited, start);

    }

    private void DrawNode(Atom current, ref HashSet<Atom> visited, Vector2Int coordinates)
    {
        visited.Add(current);
        bool[] borders, doors;
        current.getBordersAndTypes(out borders, out doors);
        //Debug.Log("Current coordinates: " + coordinates);
        //Debug.Log("Current Type [" + current.GetConnectionType(0) + "," + current.GetConnectionType(1) + "," + current.GetConnectionType(2)
        //    + "," + current.GetConnectionType(3) + "]");

        //AddBorders(coordinates.x, coordinates.y, hiGraph.nodeWidth, hiGraph.nodeHeight, borders, doors, (int)current.GetAreaType());
        //Get and apply all Attributes from parent areas here??
        
        List<Area.TileAttribute> attributes = current.GetAttributes();
        /*
        AddInclusionAdjacency("ExtraEnemy", "LabyrinthBasic", 0, 0, new Vector2Int(coordinates.x, coordinates.y),
        new Vector2Int(coordinates.x + hiGraph.nodeWidth, coordinates.y + hiGraph.nodeHeight));
        AddInclusionAdjacency("ExtraEnemy", "LabyrinthBasic3", 1, 0, new Vector2Int(coordinates.x, coordinates.y),
        new Vector2Int(coordinates.x + hiGraph.nodeWidth, coordinates.y + hiGraph.nodeHeight));
        AddWeight("ExtraEnemy", 100, new Vector2Int(coordinates.x, coordinates.y),
        new Vector2Int(coordinates.x + hiGraph.nodeWidth, coordinates.y + hiGraph.nodeHeight), 0, true);
        */

        foreach (Area.TileAttribute attr in attributes)
        {
            switch (attr.type)
            {
                //No need to account for tile guarantee
                //rotation -1 means all rotations!
                case Area.AttrType.TileProb:
                    AddWeight(attr.names[0], attr.values[0], new Vector2Int(coordinates.x, coordinates.y),
                        new Vector2Int(coordinates.x + hiGraph.nodeWidth, coordinates.y + hiGraph.nodeHeight), attr.values[1], attr.flag);
                    break;

                case Area.AttrType.TileAdjacency:
                    string from = attr.names[0];
                    for (int i = 0; i < attr.values[0]; i++)
                    {
                        int rot = attr.values[(i * 2) + 1];
                        int dir = attr.values[(i * 2) + 2];
                        string to = attr.names[i + 1];
                        if (attr.flag)
                        {
                            AddInclusionAdjacency(from, to, dir, rot, new Vector2Int(coordinates.x, coordinates.y),
                                new Vector2Int(coordinates.x + hiGraph.nodeWidth, coordinates.y + hiGraph.nodeHeight));
                        }
                        else
                        {
                            //need to adapt it so that it works the same as inclusion!
                            AddExclusionAdjacency(from, to, dir, rot, new Vector2Int(coordinates.x, coordinates.y),
                                new Vector2Int(coordinates.x + hiGraph.nodeWidth, coordinates.y + hiGraph.nodeHeight));
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        AddBorders(coordinates.x, coordinates.y, hiGraph.nodeWidth, hiGraph.nodeHeight, borders, doors, (int)current.GetAreaType());

        foreach (Area.TileAttribute attr in attributes)
        {
            switch (attr.type)
            {
                //No need to account for tile guarantee
                //rotation -1 means all rotations!
                case Area.AttrType.TilePlace: //Random coordinates and try to observe that tile in the area
                    int counter = 0;
                    while (counter < 1) //10 tries to find an unobserved tile that can place the given value
                    {
                        //Is -1 correct?
                        int valueX;
                        int valueY;
                        if (attr.values.Length >= 4 && !(attr.values[2] >= hiGraph.nodeWidth || attr.values[3] >= hiGraph.nodeHeight))
                        {
                            valueX = attr.values[2] + coordinates.x;
                            valueY = attr.values[3] + coordinates.y;
                        }
                        else
                        {
                            valueX = Random.Range(coordinates.x + 1, coordinates.x + hiGraph.nodeWidth - 1);
                            valueY = Random.Range(coordinates.y + 1, coordinates.y + hiGraph.nodeHeight - 1);
                        }
                        if (attr.flag)
                        {
                            Debug.Log("Test succesful :)");
                            AddWeight(attr.names[0], 100, new Vector2Int(valueX, valueY), new Vector2Int(valueX + 1, valueY + 1), -1, true);
                        }
                        if (unobserved[valueX, valueY] > 1)
                        {
                            bool breaker = false;
                            List<int> indexes = new List<int>();
                            for (int i = 0; i < tileSet.Count; i++)
                            {
                                if (tileSet[i].prefab.name.Equals(attr.names[0]))
                                {
                                    if (attr.values[1] == -1 || attr.values[1] == tileSet[i].rotation)
                                    {
                                        indexes.Add(i);
                                        breaker = true;
                                    }
                                }
                                else if (breaker)
                                {
                                    break;
                                }
                            }
                            int count = indexes.Count;
                            for (int i = 0; i < count; i++)
                            {
                                int random = Random.Range(0, indexes.Count);
                                if (wave[valueX, valueY, indexes[random]])
                                {
                                    Observe(valueX, valueY, indexes[random]);
                                    break;
                                }
                                else
                                {
                                    indexes.RemoveAt(random);
                                }
                            }
                        }
                        counter++;
                    }
                    break;
                default:
                    break;
            }
        }


        for (int i = 0; i < 4; i++)
        {
            Atom next = current.GetConnection(i);
            if (next != null && !visited.Contains(next))
            {
                Vector2Int temp = coordinates;
                //new direction has been picked
                switch (i)
                {

                    case 0:
                        coordinates.x -= hiGraph.nodeWidth;
                        break;
                    case 1:
                        coordinates.y += hiGraph.nodeHeight;
                        break;
                    case 2:
                        coordinates.x += hiGraph.nodeWidth;
                        break;
                    case 3:
                        coordinates.y -= hiGraph.nodeHeight;
                        break;
                }
                //Draw the new node at the new destination
                DrawNode(next, ref visited, coordinates);
                coordinates = temp;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;
            grid.setValue(worldPosition, 1);
            for (int i = 0; i < 5; i++)
                Observe();



        }
    }

    private IEnumerator ObserveAnimation()
    {
        while (!done)
        {
            for (int i = 0; i < 5; i++)
                Observe();

            yield return new WaitForSecondsRealtime(0.05f);
        }

    }

    //Observe the selected cell guaranteeing that the tile index is selected
    private void Observe(int x, int y, int index)
    {
        // Debug.Log("Index: " + index);
        //Debug.Log(unobserved[x, y]);

        for (int i = 0; i < localTileSet[x,y].Count; i++)
        {
            if (localTileSet[x, y][i] != index && wave[x, y, localTileSet[x, y][i]])
            {
                wave[x, y, localTileSet[x, y][i]] = false;
                unobserved[x, y]--;
            }
        }
        //Debug.Log(wave[x, y, index]);
        //Debug.Log(unobserved[x, y]);
        Observe(x, y);
    }

    private int getTileIndex(string name, int rotation)
    {
        for (int i = 0; i < tileSet.Count; i++)
        {
            if (rotation == tileSet[i].rotation && tileSet[i].prefab.gameObject.name.Equals(name))
            {
                return i;
            }
        }
        return -1;
    }

    private void Observe(int x, int y)
    {
        List<int> candidates = new List<int>();
        int pile = 0;
        if (localTileSet.Length < 1)
        {
            return;
        }
        for (int i = 0; i < localTileSet[x, y].Count; i++)
        {
            if (wave[x, y, localTileSet[x, y][i]] == true)
            {
                candidates.Add(localTileSet[x, y][i]);
                pile += weights[x, y, localTileSet[x, y][i]];
            }
        }
        //Debug.Log("Checkpoint1");
        int r1 = Random.Range(0, pile);
        int counter = -1;
        while (r1 >= 0)
        {
            counter++;
            if (counter >= localTileSet[x, y].Count)
            {
                Debug.Log("Counter: " + counter + ", r1: " + r1);
                break;
            }
            //Debug.Log("Checkpoint1.5. Counter: " + counter);

            r1 -= weights[x, y, candidates[counter]];
        }
        //Debug.Log("Checkpoint2");
        //int state = Random.Range(0, candidates.Count);
        for (int i = 0; i < localTileSet[x,y].Count; i++)
        {
            if (localTileSet[x, y][i] != candidates[counter] && wave[x, y, localTileSet[x, y][i]])
            {
                wave[x, y, localTileSet[x, y][i]] = false;
                unobserved[x, y]--;

            }
        }
        if (unobserved[x,y] == 1)
        {
            grid.observed[x, y] = true;
        }
        if (unobserved[x,y] < 1)
        {
            Debug.Log("x: " + x + ", y:" + y);
        }
        //Debug.Log("Checkpoint3");
        Propagate(x, y);
        UpdateGrid();

        //unobserved[x, y] = 1;


    }


    private void Observe()
    {
        int minOptions = tileSet.Count;
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (unobserved[i, j] == 0) //No possible state, algorithm needs to restart
                {
                    Debug.Log("Failed at: " + i + ", " + j);
                    done = true;
                    return;
                }
                //find the min instead, factor in probabilities of tiles
                //find the maximum value of the observed array and the amount of times it shows up
                if (unobserved[i, j] < minOptions && unobserved[i, j] > 1 && !grid.observed[i, j])
                {
                    minOptions = unobserved[i, j];
                    candidates.Clear();
                    candidates.Add(new Vector2Int(i, j));
                }
                else if (unobserved[i, j] == minOptions)
                {
                    candidates.Add(new Vector2Int(i, j));
                }
            }
        }
        int randomIndex;
        //Debug.Log(minOptions);
        if (minOptions > 1 && candidates.Count > 0)
        {

            randomIndex = Random.Range(0, candidates.Count);
            //Debug.Log(candidates[randomIndex]);
            Observe(candidates[randomIndex].x, candidates[randomIndex].y);
        }
        else
        {
            
            done = true;
            return;
        }
        if (unobserved[candidates[randomIndex].x, candidates[randomIndex].y] == 0) //No possible state, algorithm needs to restart
        {
            Debug.Log("Failed at: " + candidates[randomIndex].x + ", " + candidates[randomIndex].y);
            grid.setValue(candidates[randomIndex].x, candidates[randomIndex].y, -1);
            done = true;
            return;
        }

        //Do the next step

    }


    //int x and y are the origin of the propagation, assumed to be fully observed
    private void Propagate(int x, int y)
    {
        //Find the neighboring cells
        //Check using the origin available tiles and rules to see if there are new unavailable tiles
        //For every change in a cell, call propagate on that cell
        if (x + 1 < width)
        {
            
            bool changed = Propagate(x, y, 2);
            
            if (changed)
            {
                Propagate(x + 1, y);
            }
            //TODO: manual run on paper
        }
        if (x - 1 >= 0)
        {
            bool changed = Propagate(x, y, 0);
            if (changed)
            {
                Propagate(x - 1, y);
            }
        }
        if (y + 1 < height)
        {
            bool changed = Propagate(x, y, 1);
            if (changed)
            {
                Propagate(x, y + 1);
            }
        }
        if (y - 1 >= 0)
        {
            bool changed = Propagate(x, y, 3);
            if (changed)
            {
                Propagate(x, y - 1);
            }

        }
    }

    private bool Propagate(int x, int y, int dir)
    {
        bool changed = false;
        int xNew = x;
        int yNew = y;
        switch(dir)
        {
            case 0:
                xNew--;
                break;
            case 1:
                yNew++;
                break;
            case 2:
                xNew++;
                break;
            case 3:
                yNew--;
                break;
        }
        for (int i = 0; i < localTileSet[xNew, yNew].Count; i++)
        {
            //We find a state that is still marked as possible
            //i: index of potential tile in x + 1, j: index of tile being propagated
            if (wave[xNew, yNew, localTileSet[xNew, yNew][i]] == true)
            {
                bool valid = false;
                //Check if at least one rule in the given direction can still place this state
                for (int j = 0; j < localTileSet[x,y].Count; j++)
                {
                    
                    if (exclusionModel[x, y].checkValid(localTileSet[x, y][j], dir, localTileSet[xNew, yNew][i]))
                    {
                        /*
                        Debug.Log("Tile 1: " + tileSet[localTileSet[x, y][j]].prefab.gameObject.name + "rot" + tileSet[localTileSet[x, y][j]].rotation +
                            ", dir " + dir +
                            ", Tile 2: " + tileSet[localTileSet[xNew, yNew][i]].prefab.gameObject.name + "rot" + tileSet[localTileSet[xNew, yNew][i]].rotation);
                        */
                    }
                    
                    if (wave[x, y, localTileSet[x, y][j]] == true && 
                        (model.checkValid(localTileSet[x, y][j], dir, localTileSet[xNew, yNew][i]) 
                        || inclusionModel[x, y].checkValid(localTileSet[x, y][j], dir, localTileSet[xNew, yNew][i]))
                        && !exclusionModel[x, y].checkValid(localTileSet[x, y][j], dir, localTileSet[xNew, yNew][i]))
                    {
                        //Debug.Log("Tile at:" + (xNew) + ", " + yNew + " index " + i + ". Validated with rule: from " + j + ", dir" + dir + ", to" + i);
                        valid = true;
                        break;
                    }
                }
                if (!valid)
                {
                    wave[xNew, yNew, localTileSet[xNew, yNew][i]] = false;
                    unobserved[xNew, yNew]--;
                    changed = true;
                }
            }
        }
        return changed;
    }

    private void UpdateGrid()
    {
        //Places newly observed tiles
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (unobserved[i, j] == 1 && set[i, j] == -1)
                {
                    int counter = 0;
                    while (wave[i, j, counter] == false)
                    {
                        counter++;
                    }
                    set[i, j] = counter;
                    RenderTile(i, j); //inefficient
                    setTiles++;
                    if (setTiles >= width * height)
                    {
                        finished = true;
                    }
                }
                grid.setValue(i, j, unobserved[i, j]);
            }
        }
    }




    public static TextMesh createWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3))
    {
        Color color = Color.white;
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = 30;
        textMesh.anchor = TextAnchor.MiddleCenter;
        return textMesh;
    }
}
