// Module name: Assembly-CSharp
// File name: Logger.cs
// Last edit: 2024-06-17 by Mateusz Chojnowski mateusz.chojnowski@inseye.com
// Copyright (c) Inseye Inc.
// 
// This file is part of Inseye Software Development Kit subject to Inseye SDK License
// See  https://github.com/Inseye/Licenses/blob/master/SDKLicense.txt.
// All other rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Logger : ILogHandler
{
    private const int LogsLimit = 100;
    private readonly ILogHandler _default;

    public struct SavedLog
    {
        public LogType LogType;
        public string Message;
    }

    private static readonly Queue<SavedLog> RelevantLogs = new();
    public static event Action<SavedLog> OnNewLog;
    private static bool isInitialized = false;

    private Logger()
    {
        _default = Debug.unityLogger.logHandler;
    }

    public static void Initialize()
    {
        if (isInitialized)
            return;
        isInitialized = true;
        Debug.unityLogger.logHandler = new Logger();
    }

    public static void Uninitialize()
    {
        if (!isInitialized)
            return;
        isInitialized = false;
        if (Debug.unityLogger.logHandler is Logger logger)
        {
            Debug.unityLogger.logHandler = logger._default;
        }
    }

    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        _default.LogFormat(logType, context, format, args);
        RecordLog(new SavedLog
        {
            LogType = logType,
            Message = string.Format(format, args)
        });
    }

    public void LogException(Exception exception, Object context)
    {
        if (RelevantLogs.Count >= LogsLimit)
            RelevantLogs.Dequeue();
        _default.LogException(exception, context);
        RecordLog(new SavedLog
        {
            LogType = LogType.Exception,
            Message = $"{exception.Message}\n{exception.StackTrace}"
        });
    }

    private void RecordLog(SavedLog log)
    {
        if (RelevantLogs.Count >= LogsLimit)
            RelevantLogs.Dequeue();
        RelevantLogs.Enqueue(log);
        OnNewLog?.Invoke(log);
    }

    public static IEnumerable<SavedLog> GetRelevantLogs()
    {
        return RelevantLogs;
    }
}