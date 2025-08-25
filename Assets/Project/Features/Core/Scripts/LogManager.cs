using UnityEngine;
using System;

public static class LogManager
{

    private static string GetTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    public static void LogDevelopment(string message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log($"[{GetTimestamp()}] [DEV] {message}");
#endif
    }

    public static void Log(string message)
    {
        Debug.Log($"[{GetTimestamp()}] {message}");
    }

    public static void LogError(string message)
    {
        Debug.LogError($"[{GetTimestamp()}] [ERROR] {message}");
    }
    
    public static void LogWarning(string message)
    {
        Debug.LogWarning($"[{GetTimestamp()}] [WARNING] {message}");
    }

    public static void LogServer(string message)
    {
        // TODO: 추후 서버 구현시 서버 로그 전송 로직 추가
        Debug.LogWarning($"[{GetTimestamp()}] [SERVER LOG] {message}");
    }
}
