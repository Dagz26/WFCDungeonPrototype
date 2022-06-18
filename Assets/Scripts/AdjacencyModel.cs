using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacencyModel
{
    public List<HashSet<int>[]> model;
    private int size;

    public AdjacencyModel()
    {
        model = new List<HashSet<int>[]>();
        size = 0;
    }

    public void addTile()
    {
        model.Add(new HashSet<int>[4]);
        for (int i = 0; i < 4; i++)
        {
            model[model.Count - 1][i] = new HashSet<int>();
        }
        
    }

    public void addAdjacency(int from, int dir, int to)
    {
        if (from >= model.Count || to >= model.Count || dir >= model[from].Length)
        {
            return;
        }
        model[from][dir].Add(to);
        size++;
    }
    
    public bool compatible(int from, int dir, int to)
    {
        return false;
    }

    public void printAdjacencies()
    {
        for (int i = 0; i < model.Count; i++)
        {
           
            for (int j = 0; j < 4; j++)
            {
                //Debug.Log("Here Ok, " + model[i][j].Count);
                foreach (int k in model[i][j])
                {
                    Debug.Log("Ad: from " + i + ", in direction " + j + ", to " + k);
                }
            }
        }
    }

    public int GetSize()
    {
        return size;
    }

    public HashSet<int> GetApplicableRules(int from, int dir)
    {
        return model[from][dir];
    }

    public bool checkValid(int from, int dir, int to)
    {
        return model[from][dir].Contains(to);
    }

    public  void RemoveAdjacency(int from, int dir, int to)
    {
        if (model[from][dir].Contains(to))
        {
            model[from][dir].Remove(to);
        }
    }

}
