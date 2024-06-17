// Module name: Assembly-CSharp
// File name: LoggerDisplay.cs
// Last edit: 2024-06-17 by Mateusz Chojnowski mateusz.chojnowski@inseye.com
// Copyright (c) Inseye Inc.
// 
// This file is part of Inseye Software Development Kit subject to Inseye SDK License
// See  https://github.com/Inseye/Licenses/blob/master/SDKLicense.txt.
// All other rights reserved.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoggerDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject logTextsParent = null!;

    private readonly Queue<TextMeshProUGUI> _textSlots = new();

    private void Awake()
    {
        for (var i = 0; i < logTextsParent.transform.childCount; i++)
        {
            if (logTextsParent.transform.GetChild(i).TryGetComponent<TextMeshProUGUI>(out var text))
            {
                _textSlots.Enqueue(text);
                text.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        foreach (var logs in Logger.GetRelevantLogs())
        {
            HandleLog(logs);
        }

        Logger.OnNewLog += HandleLog;
    }

    private void OnDestroy()
    {
        Logger.OnNewLog -= HandleLog;
    }


    private void HandleLog(Logger.SavedLog log)
    {
        WriteMessage(log.Message, log.LogType);
    }

    public void WriteMessage(string message, LogType level = LogType.Log)
    {
        var currentText = _textSlots.Dequeue();
        _textSlots.Enqueue(currentText);
        currentText.transform.SetAsFirstSibling();
        currentText.gameObject.SetActive(true);
        var now = DateTimeOffset.Now;
        currentText.text = $"{now.Hour:00}:{now.Minute:00}:{now.Second:00} {message}";
        currentText.color = level switch
        {
            LogType.Log => Color.white,
            LogType.Assert => Color.yellow,
            LogType.Warning => Color.yellow,
            LogType.Error => Color.red,
            LogType.Exception => Color.red,
            _ => Color.white
        };
    }
}