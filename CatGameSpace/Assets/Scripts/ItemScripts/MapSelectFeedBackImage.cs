using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelectFeedBackImage : MonoBehaviour
{
    public int opennedBy = -1;

    public void Close()
    {
        opennedBy = -1;
        gameObject.SetActive(false);
    }
}
