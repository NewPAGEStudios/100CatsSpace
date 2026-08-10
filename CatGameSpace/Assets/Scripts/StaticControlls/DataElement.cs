using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DataElement
{

    public string paintID;
    public bool[] paint_regionColorData;
    public bool[] paint_catFindData;
    public bool paint_finished;
    public float paintTimer;
    public int paint_hint;
    public bool solo_Corrupted;
    public DataElement(string paintID,bool[] paintData, bool[] paintDataCat, bool paint_finished, float paintTimer, int paint_hint, bool solo_Corrupted)
    {
        this.paintID = paintID;
        this.paint_regionColorData = paintData;
        this.paint_catFindData = paintDataCat;
        this.paint_finished = paint_finished;
        this.paintTimer = paintTimer;
        this.paint_hint = paint_hint;
        this.solo_Corrupted = solo_Corrupted;
    }


}
