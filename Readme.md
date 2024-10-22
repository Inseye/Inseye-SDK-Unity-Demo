# SDK Usage
*created by [Mateusz Chojnowski](mailto:mateusz.chojnowski@remmed.vision)*

[Inseye SDK Usage](https://github.com/Inseye/Inseye-Unity-SDK-Usage) is a sample project that can be used as a reference for integration with the Inseye Journey eye tracker.

It presents all the features included in the SDK, makes use of all of the samples delivered with the SDK, and is configured to work on all VR headset compatible with the Inseye Journey eye tracker.

# License 

This repository is part of Inseye Software Development Kit.
By using content of this repository you agree to SDK [License](./LICENSE).

# Requirements
- Unity 2021.3.45f1 w with Android build support
- (optionally) VR Headset
- (optionally) Inseye Loomi

# Project tour
After loading the project, open the entry scene in `Assets/Scene/EntryScene.unity`.

[<image src="./docs/usage_entry.png" width="600"/>](./docs/usage_entry.png)

Elements in the scene hierarchy highlighted in yellow are considered important and, thus, will be described briefly:

`MockSource` - game object with the mock eye tracker implementation. Useful for prototyping and development in the Unity editor. Check [the article about eye tracker mock](./Packages/Inseye-SDK-Unity/Documentation~/articles/samples/mock.md) to get better understanding on how it is implemented in the Inseye SDK. By default, it should always be active during development in editor and builds for PC.

`Canvas` - contains `MenuButton` script - main controller of this scene.

`Grid` - object which can be used to validate calibration visually.

`LeftHand Controller` and `RightHand Controller` - game objects used as controller-based input source.

`Adapter` - object with `InseyeGazeInputAdapter` that injects Inseye gaze data into Unity Input System

# Scene runtime functionalities 

Each button in the scene invokes a callback from `Scripts/ButtonMenu.cs` in behavior attached to the `Canvas` game object.

- **Star Calibration (animations)** button invokes `ButtonMenu.StartCalibrationAnimations` which will load a scene from [Calibration sample](./Packages/Inseye-SDK-Unity/Documentation~/articles/samples/calibration.md) to perform calibration and load back the `EntryScene`.
- **Start Calibration (no animations)** - button invokes `ButtonMenu.StartCalibrationNoAnimations` which will start calibration defined in [Calibration sample](./Packages/Inseye-SDK-Unity/Documentation~/articles/samples/calibration.md)  using a custom calibration scene located at `Assets/Scenes/NoAnimationCalibrationScene`. 
- **Start Calibration and abort (7s)** - shows how to cancel ongoing calibration after 7 seconds.
- **Show gaze ray** button invokes `ButtonMenu.ShowGazeRay` that spawns a ray following the gaze position. Gaze ray is obtained using `ToGazeRay` extension function.
- **Show gaze data** button invokes `ButtonMenu.ShowGazeData` which will display the current gaze data recordings at the top of the menu.
- **Subscribe to events** button invokes `ButtonMenu.SubscribeToEvents`. Any further events related to changes in the eye tracker state will be logged in the message log (on left to the button menu).
- **Get eye tracker availability** button invokes `ButtonMenu.GetEyetrackerAvailability`, immediately pooling the current eye tracker state and logging it in the message log.
- **Get diagnostic data** button invokes `ButtonMenu.GetDiagnosticData`, logging selected data about the SDK and the camera configuration.
- **Keep initialized** button invokes `ButtonMenu.KeepInitialized` which keeps SDK connected to the eye tracker even if nothing requests any SDK functionality.
- **Switch implementation to mock** switches current SDK implementation to mock eye tracker.

# Usage scenario
Common scenario from application lifecycle perspective is to:
- put on and adjust the headset by the user,
- calibrate the eye tracker,
- provide calibrated gaze data to various consumers within the application.

# Building the project for an Android VR device
1. Make sure that current project build target is Android.
2. Turn off the mock eye tracker implementation by disabling `MockSource` game object.
3. Build and install the application on the Android device equipped with the Inseye eye tracker. 
