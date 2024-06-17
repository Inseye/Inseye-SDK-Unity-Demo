using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Inseye.Samples.Calibration.Tests
{
    public static class CalibrationTest
    {
        [Test]
        public static void TestSceneInProject()
        {
            var index = SceneUtility.GetBuildIndexByScenePath(InseyeCalibrationArguments.SampleCalibrationScenePath);
            Assert.True(index > 0,
                $"Missing scene default scene in project or build settings, scene name: {InseyeCalibrationArguments.SampleCalibrationScenePath}");
        }

        [UnityTest]
        public static IEnumerator TestObjectInDefaultScene()
        {
            // load scene
            SceneManager.LoadScene(InseyeCalibrationArguments.SampleCalibrationScenePath, LoadSceneMode.Single);
            yield return null;
            var calibration = Object.FindObjectOfType<InseyeCalibration>(true);
            Assert.IsNotNull(calibration,
                $"Missing game object with {nameof(InseyeCalibration)} mono behavior attached to id in scene: {InseyeCalibrationArguments.SampleCalibrationScenePath}");
        }
    }
}