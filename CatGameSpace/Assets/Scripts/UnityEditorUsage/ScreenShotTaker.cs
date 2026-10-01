using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotTaker : MonoBehaviour
{
    void Update()
    {
        // "K" tuşuna basıldığında ekran görüntüsü al
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string fileName = $"screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            ScreenCapture.CaptureScreenshot(fileName);
            Debug.Log("Screenshot saved to: " + fileName);
        }
    }
}
