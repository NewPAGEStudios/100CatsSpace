using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLimiter : MonoBehaviour
{
    // Editör üzerinden deðiþtirebilmek için public deðiþken
    public int targetFPS = 60;

    void Awake()
    {
        // VSync kapalý olmalý, aksi halde bu kod çalýþmaz
        QualitySettings.vSyncCount = 0;

        // Hedeflenen FPS deðeri
        Application.targetFrameRate = targetFPS;
    }
}
