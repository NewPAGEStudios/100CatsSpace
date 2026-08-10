using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Intro : MonoBehaviour
{
    VideoPlayer player;
    bool ended = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<VideoPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!ended && !player.isPlaying)
        {
            ended = true;
            StartCoroutine(End());
        }
    }
    IEnumerator End()
    {
        yield return new WaitForSeconds(.2f);
        SceneManager.LoadScene("Menu");
    }

}
