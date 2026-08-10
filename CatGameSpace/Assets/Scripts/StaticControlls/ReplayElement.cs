using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReplayElement
{
    public string paintID;

    public float[] xs;
    public float[] ys;


    public int[] ItemID;

    public ReplayElement(string paintID, Vector2[] pos,int[] itemID)
    {
        this.paintID = paintID;
        xs = new float[pos.Length];
        ys = new float[pos.Length];

        for(int i = 0; i < pos.Length; i++)
        {
            xs[i] = pos[i].x;
            ys[i] = pos[i].y;
        }


        ItemID = itemID;
    }
}
