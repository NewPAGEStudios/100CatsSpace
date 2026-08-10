using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static bool createLog = false;
    public static string logs;
    void Awake()
    {
        logs += "=== Game Started ===\n";
        Application.logMessageReceived += HandleLog;

    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {

        logs += $"[{type}] {logString}\n";

        if (type == LogType.Error) createLog = true;

    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void OnApplicationQuit()
    {
        if (!createLog) return; 
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string filePath = Path.Combine(desktopPath, "100CatLostInWorldLogs" + DateTime.Now + ".txt");


        File.CreateText(filePath);
        File.WriteAllText(filePath, logs);

    }
}