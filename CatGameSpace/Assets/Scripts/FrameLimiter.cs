using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLimiter : MonoBehaviour
{
    // Editör üzerinden değiştirebilmek için public değişken
    public int targetFPS = 60;

    // Mobilde Unity varsayılan olarak 30 FPS çalışır; sahnede FrameLimiter olmasa da 60'a çek
    public const int MobileTargetFPS = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SetMobileFrameRate()
    {
        if (UnityEngine.Device.Application.isMobilePlatform)
        {
            Application.targetFrameRate = MobileTargetFPS;
        }
    }

    void Awake()
    {
        // VSync kapalı olmalı, aksi halde bu kod çalışmaz
        QualitySettings.vSyncCount = 0;

        // Hedeflenen FPS değeri
        Application.targetFrameRate = targetFPS;
    }
}
