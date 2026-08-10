using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "SCPObjects/RadioObj")]
public class RadioObject : ScriptableObject
{
    public string id;

    public string RadName;

    public AudioClip[] clips;


    public void RandomizeClips()
    {
        List<AudioClip> clipList = new List<AudioClip>();
        clipList = clips.ToList<AudioClip>();

        List<int> posID = Enumerable.Range(0, clips.Length).ToList();
        Array.Fill<AudioClip>(clips, null);

        int c = 0;
        while(posID.Count > 0)
        {
            int i = UnityEngine.Random.Range(0, posID.Count);
            clips[c] = clipList[posID[i]];
            posID.RemoveAt(i);
            c++;
        }

    }

}
