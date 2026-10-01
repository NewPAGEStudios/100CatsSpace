using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLimiter : MonoBehaviour
{
    // Editör üzerinden değiştirebilmek için public değişken
    public int targetFPS = 60;

    void Awake()
    {
        // VSync kapalı olmalı, aksi halde bu kod çalışmaz
        QualitySettings.vSyncCount = 0;

        // Hedeflenen FPS değeri
        Application.targetFrameRate = targetFPS;
    }
}
