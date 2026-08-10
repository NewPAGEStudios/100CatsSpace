using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Text yerine TextMeshProUGUI
    float deltaTime;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {fps:0.}";
    }
}
