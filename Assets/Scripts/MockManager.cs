// Module name: Assembly-CSharp
// File name: MockManager.cs
// Last edit: 2024-06-17 by Mateusz Chojnowski mateusz.chojnowski@inseye.com
// Copyright (c) Inseye Inc.
// 
// This file is part of Inseye Software Development Kit subject to Inseye SDK License
// See  https://github.com/Inseye/Licenses/blob/master/SDKLicense.txt.
// All other rights reserved.

#nullable enable
using UnityEngine;

public class MockManager : MonoBehaviour
{
    private static MockManager _globalInstance;
    private GameObject? _mockInstance;

    [SerializeField]
    private GameObject pcMock;

    [SerializeField]
    private GameObject androidMock;


    private void Awake()
    {
        if (_globalInstance == null)
        {
            _globalInstance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void OnEnable()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var prefab = androidMock;
#else
        var prefab = pcMock;
#endif
        _mockInstance = Instantiate(prefab, transform);
    }

    private void OnDisable()
    {
        if (_mockInstance != null)
        {
            Destroy(_mockInstance);
            _mockInstance = null;
        }
    }
}