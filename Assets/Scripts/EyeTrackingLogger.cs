using UnityEngine;
using LSL;

using Tobii.XR;
using Pvr_UnitySDKAPI;

using ViveSR.anipal.Eye;

/// <summary>
/// Class: EyeTrackingLogger
/// Implements: MonoBehaviour
/// Author: Carlos Tirado   <Carlos.A.TiradoCortes@student.uts.edu.au>
///     
/// Description: This script is in charge of receiving all the data from the eyetracking different sources, and store it in one place. 
/// Once the experiment has finished, this will store the data somewhere.
///                     
///           __,.__
///          /  ||  \               
///   ::::::| .-'`-. |::::::
///   :::::/.'  ||  `,\:::::
///   ::::/ |`--'`--'| \::::
///   :::/   \`/++\'/   \::: 
/// </summary>
/// 
//Reworked from Carlos' logger to work with the universal logger - Alex
//Also updated for the 2.0 SDK
public class EyeTrackingLogger : MonoBehaviour
{
    private float timeCounter;
    string LogFile;

    [Tooltip("Frequency of the aquisition of eye tracking data")]
    public float frequency = 120.0f;

    private float time_frequency;

    // Start is called before the first frame update
    void Start()
    {
        timeCounter = 0.0f;
        time_frequency = 1 / frequency;
        LogFile = Application.persistentDataPath + "/Experiment-" + System.DateTime.Now.ToShortDateString().Replace('/', '-') + "-" + System.DateTime.Now.ToShortTimeString().Replace(':', '-') + ".json";
        Debug.Log(string.Format("EyeTrackingLogger - Start tracking"));
    }

    // Update is called once per frame
    void Update()
    {
        AcquireEyeTrackingData();
        EyeBlinkingEvent();
    }


    // <summary>
    // Every time it will check if the user blinked, if so, will send an event for blinking
    // </summary>
    private void EyeBlinkingEvent()
    {
        if (TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World).IsLeftEyeBlinking)
        {
            ExperimentController.markerStream.Write("LEFT_BLINK");
        }

        if (TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World).IsRightEyeBlinking)
        {
            ExperimentController.markerStream.Write("RIGHT_BLINK");
        }
    }

    private void AcquireEyeTrackingData()
    {
        timeCounter += Time.deltaTime;
        TobiiXR_EyeTrackingData eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

        if (timeCounter >= time_frequency)
        {
            timeCounter = 0.0f;

            var gazeData = new EyeGazeData
            {
                gaze_location = eyeTrackingData.GazeRay.Origin,
                gaze_direction = eyeTrackingData.GazeRay.Direction,
                isLeftEyeBlinking = eyeTrackingData.IsLeftEyeBlinking,
                isRightEyeBlinking = eyeTrackingData.IsRightEyeBlinking,
                timestamp = eyeTrackingData.Timestamp,
                time = Time.time
            };

            if (TobiiXR.FocusedObjects.Count > 0)
            {
                gazeData.objName = TobiiXR.FocusedObjects[0].GameObject.transform.parent.name + "_" + TobiiXR.FocusedObjects[0].GameObject.name;
            }

            gazeData.LogToFile(LogFile);

        }
    }

    [System.Serializable]
    public class EyeGazeData
    {
        public Vector3 gaze_location;
        public Vector3 gaze_direction;
        public float timestamp;
        public float time;
        public bool isLeftEyeBlinking;
        public bool isRightEyeBlinking;
        public string objName;
        private bool logged = false;

        public void LogToFile(string fileName)
        {
            if (!logged) ExperimentController.logWrapper.AddToLog(this);
            logged = true;
        }

        public EyeGazeData() { }
    }
}
