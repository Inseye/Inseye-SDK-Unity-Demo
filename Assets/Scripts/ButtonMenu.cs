// Module name: Assembly-CSharp
// File name: ButtonMenu.cs
// Last edit: 2024-06-17 by Mateusz Chojnowski mateusz.chojnowski@inseye.com
// Copyright (c) Inseye Inc.
// 
// This file is part of Inseye Software Development Kit subject to Inseye SDK License
// See  https://github.com/Inseye/Licenses/blob/master/SDKLicense.txt.
// All other rights reserved.

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Inseye;
using Inseye.Extensions;
using Inseye.Interfaces;
using Inseye.Samples.Calibration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Task = System.Threading.Tasks.Task;


public class ButtonMenu : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer = null!;

    [SerializeField]
    private new Camera camera = null!;

    [SerializeField]
    private TextMeshProUGUI dataDisplay = null!,
        showGazeRayButtonLabel = null!,
        showGazeDataButtonLabel = null!,
        subscribeToEventsButtonLabel = null!,
        switchImplementationLabel = null!;

    [SerializeField]
    private GameObject ray = null!;

    [SerializeField]
    private LoggerDisplay loggerDisplay;


    private readonly Queue<TextMeshProUGUI> _eventsLogSlots = new();

    private bool _showGazeRay, _showGazeData, _subscribedToEvents;

    private void FocusChangeHandler(bool focusValue)
    {
        Debug.Log($"Focus: {focusValue}");
    }

    private void Awake()
    {
        Logger.Initialize();
    }

    private void Start()
    {
        Debug.Log($"State: {InseyeSDK.InseyeSDKState:G}");
        InseyeSDK.MostAccurateEyeChanged += HandleEvent;
    }

    public void StartCalibrationAnimations()
    {
        if (!DebounceCheck())
            return;
        StartCalibration();
    }

    public void StartCalibrationNoAnimations()
    {
        if (!DebounceCheck())
            return;
        StartCalibration("Scenes/NoAnimationsCalibrationScene");
    }

    public void StartCalibrationAndAbort()
    {
        if (!DebounceCheck())
            return;
        StartCalibration();
        if (SynchronizationContext.Current != null)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await Task.Delay(7000);
                    InseyeSDK.AbortCalibration();
                }
                catch (Exception exc)
                {
                    Debug.LogException(exc);
                }
            }, default, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }


    private async void StartCalibration(string? scenename = null)
    {
        ICalibrationProcedure? calibrationProcedure = null;
        try
        {
            calibrationProcedure = Inseye.InseyeSDK.StartCalibration();
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var activeSceneBuildIndex = activeScene.buildIndex;
            var args = scenename is null
                ? InseyeCalibrationArguments.StandardCalibration(calibrationProcedure)
                : new InseyeCalibrationArguments(scenename, calibrationProcedure);
            var calibrationResult = await InseyeCalibration.PerformCalibrationAsync(args);
            UnityAction<Scene, LoadSceneMode> callback = default!;
            callback = (_, _) =>
            {
                var currentButtonMenu = FindObjectOfType<ButtonMenu>(true);
                currentButtonMenu.loggerDisplay.WriteMessage($"Calibration result: {calibrationResult:G}");
                SceneManager.sceneLoaded -= callback;
            };
            SceneManager.sceneLoaded += callback;
            SceneManager.LoadScene(activeSceneBuildIndex, LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            if (calibrationProcedure is not null)
                if (calibrationProcedure.InseyeCalibrationState is not (InseyeCalibrationState.FinishedFailed
                    or InseyeCalibrationState.FinishedSuccessfully))
                {
                    Debug.LogError("Calibration was not properly disposed, ");
                }
        }
    }

    private IGazeProvider? _gazeProvider;

    public void ShowGazeRay()
    {
        if (!DebounceCheck())
            return;
        if (_showGazeRay)
        {
            try
            {
                _showGazeRay = false;
                CheckGazeProvider();
            }
            catch
            {
                _showGazeRay = false;
                throw;
            }

            showGazeRayButtonLabel.text = "Show gaze ray";
            ray.SetActive(false);
            return;
        }

        _gazeProvider ??= InseyeSDK.GetGazeProvider();
        _showGazeRay = true;
        showGazeRayButtonLabel.text = "Stop gaze ray";
        ray.SetActive(true);

        IEnumerator _()
        {
            while (_showGazeRay)
            {
                var lastFrameData = _gazeProvider.GetGazeDataFromLastFrame();
                if (!lastFrameData.IsEmpty)
                {
                    var cameraTransform = camera.transform;
                    var gazeRay = lastFrameData.GazeMeanPosition().TrackerToGazeRay(cameraTransform);
                    var origin = gazeRay.origin + cameraTransform.forward;
                    lineRenderer.SetPositions(new Vector3[2] {origin, gazeRay.origin + gazeRay.direction * 150});
                }

                yield return null;
            }
        }

        StartCoroutine(_());
    }

    public void ShowGazeData()
    {
        if (!DebounceCheck())
            return;
        if (_showGazeData)
        {
            try
            {
                _showGazeData = false;
                CheckGazeProvider();
            }
            catch
            {
                _showGazeData = true;
                throw;
            }

            showGazeDataButtonLabel.text = "Show gaze data";
            dataDisplay.text = "";
            return;
        }

        _gazeProvider ??= InseyeSDK.GetGazeProvider();
        _showGazeData = true;
        showGazeDataButtonLabel.text = "Stop gaze data";

        IEnumerator _()
        {
            while (_showGazeData)
            {
                if (_gazeProvider.GetMostRecentGazeData(out var gazeData))
                    dataDisplay.text = gazeData.ToStringDegrees();
                else
                    dataDisplay.text = "Failed to obtain fresh gaze data";
                yield return null;
            }
        }

        StartCoroutine(_());
    }

    private void CheckGazeProvider()
    {
        if (!(_showGazeData || _showGazeRay))
        {
            _gazeProvider?.Dispose();
            _gazeProvider = null;
        }
    }

    public void KeepInitialized()
    {
        if (DebounceCheck())
            InitializationHolder.KeepInitialized();
    }

    public void SubscribeToEvents()
    {
        if (!DebounceCheck())
            return;
        if (_subscribedToEvents)
        {
            InseyeSDK.EyeTrackerAvailabilityChanged -= HandleEvent;
            _subscribedToEvents = false;
            subscribeToEventsButtonLabel.text = "Subscribe to events";
            return;
        }

        InseyeSDK.EyeTrackerAvailabilityChanged += HandleEvent;
        _subscribedToEvents = true;
        subscribeToEventsButtonLabel.text = "Unsubscribe from events";
    }

    public void GetEyetrackerAvailability()
    {
        if (!DebounceCheck())
            return;
        var availability = InseyeSDK.GetEyetrackerAvailability();
        Debug.Log(availability.ToString("G"));
    }

    public void GetDiagnosticData()
    {
        if (!DebounceCheck())
            return;
        var sb = new StringBuilder();
        sb.Append("Diagnostic data\nVersions:\n");
        foreach (var (component, version) in InseyeSDK.GetVersions())
            sb.Append(component.ToString("G")).Append(' ').Append(version.ToString()).Append('\n');
        sb.Append("SDK Usage: ").Append(Application.version).Append('n');
        sb.Append("Device name: ").Append(SystemInfo.deviceName).Append('\n');
        sb.Append("Eye: ").Append(InseyeSDK.GetMostAccurateEye()).Append('\n');
        sb.Append("Camera fov: ").Append(camera.fieldOfView).Append('\n');
        sb.Append("Camera fov aspect ratio: ").Append(camera.aspect).Append('\n');
        sb.Append("Screen width: ").Append(Screen.width).Append(", height:").Append(Screen.height).Append('\n');
        sb.Append("SKD State: ").Append(InseyeSDK.InseyeSDKState.ToString("G")).Append('\n');
        loggerDisplay.WriteMessage(sb.ToString());
    }

    public void SwitchImplementation()
    {
        if (!DebounceCheck())
            return;
        var mockManager = FindObjectOfType<MockManager>();
        if (mockManager == null)
            throw new Exception("MockManager is null");
        var isMockEnabled = !mockManager.enabled;
        mockManager.enabled = isMockEnabled;
        switchImplementationLabel.text =
            isMockEnabled ? "Turn off\nmock implementation" : "Switch implementation\nto mock";
    }

    private void HandleEvent(InseyeEyeTrackerAvailability availability)
    {
        Debug.Log(availability.ToString("G"));
    }

    private void HandleEvent(Eyes eyes)
    {
        Debug.Log($"Eyes event: {eyes:G}");
    }

    private void OnDestroy()
    {
        // Application.focusChanged -= FocusChangeHandler;
        _gazeProvider?.Dispose();
        if (_subscribedToEvents)
            InseyeSDK.EyeTrackerAvailabilityChanged -= HandleEvent;
        InseyeSDK.MostAccurateEyeChanged -= HandleEvent;
    }

    private int _lastInteractionFrame;

    private bool DebounceCheck()
    {
        if (_lastInteractionFrame < Time.frameCount)
        {
            _lastInteractionFrame = Time.frameCount;
            return true;
        }

        return false;
    }
}