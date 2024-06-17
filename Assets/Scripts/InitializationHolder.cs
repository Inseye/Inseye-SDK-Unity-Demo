// Module name: Assembly-CSharp
// File name: InitializationHolder.cs
// Last edit: 2024-06-17 by Mateusz Chojnowski mateusz.chojnowski@inseye.com
// Copyright (c) Inseye Inc.
// 
// This file is part of Inseye Software Development Kit subject to Inseye SDK License
// See  https://github.com/Inseye/Licenses/blob/master/SDKLicense.txt.
// All other rights reserved.

#nullable enable
using System;
using Inseye;

public static class InitializationHolder
{
    private static IDisposable? _initializationKeeper;

    public static void KeepInitialized()
    {
        if (InseyeSDK.InseyeSDKState == InseyeSDKState.Uninitialized)
        {
            _initializationKeeper?.Dispose();
            _initializationKeeper = InseyeSDK.KeepEyeTrackerInitialized();
        }
        else if (_initializationKeeper is null)
            _initializationKeeper = InseyeSDK.KeepEyeTrackerInitialized();
    }
}