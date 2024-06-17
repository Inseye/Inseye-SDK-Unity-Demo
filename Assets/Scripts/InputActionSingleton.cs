// Module name: Assembly-CSharp
// File name: InputActionSingleton.cs
// Last edit: 2024-06-17 by Mateusz Chojnowski mateusz.chojnowski@inseye.com
// Copyright (c) Inseye Inc.
// 
// This file is part of Inseye Software Development Kit subject to Inseye SDK License
// See  https://github.com/Inseye/Licenses/blob/master/SDKLicense.txt.
// All other rights reserved.

using UnityEngine;

public class InputActionSingleton : MonoBehaviour
{
    private InputActionSingleton _instance;

    [SerializeField]
    private GameObject inputActionManager;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this);
        inputActionManager.gameObject.SetActive(true);
    }
}