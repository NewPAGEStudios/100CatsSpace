using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CatSfxAutomation : MonoBehaviour
{
    public AudioClip[] meows;
    public AudioMixerGroup mixer;
    private void Start()
    {
        int i = 0;
        foreach (AudioClip clip in meows)
        {
            GameObject go = new GameObject("meow_" + i);
            go.transform.SetParent(transform);
            go.AddComponent<AudioSource>().clip = clip;
            go.GetComponent<AudioSource>().volume = .85f;
            go.GetComponent<AudioSource>().loop = false;
            go.GetComponent<AudioSource>().playOnAwake = false;
            go.GetComponent<AudioSource>().outputAudioMixerGroup = mixer;
            i++;
        }
    }
}
