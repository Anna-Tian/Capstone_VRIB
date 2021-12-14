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
    private Pvr_UnitySDKAPI.EyeTrackingData eyeTrackingData;
    private Pvr_UnitySDKAPI.EyeTrackingGazeRay gazeRay;

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
        // EyeBlinkingEvent();
    }


    /// <summary>
    /// Every time it will check if the user blinked, if so, will send an event for blinking
    /// </summary>
    // private void EyeBlinkingEvent()
    // {
    //   if (TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).IsLeftEyeBlinking)
    //   {
    //     Logger.markerStream.Write("LEFT_BLINK");
    //   }

    //   if (TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).IsRightEyeBlinking)
    //   {
    //     Logger.markerStream.Write("RIGHT_BLINK");
    //   }
    // }

    private void AcquireEyeTrackingData()
    {
        timeCounter += Time.deltaTime;
        // EyeTrackingData eyeTrackingData = new EyeTrackingData();
        // EyeTrackingGazeRay gazeRay = new EyeTrackingGazeRay();
        bool isEyeTrackingData = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref eyeTrackingData);
        bool isGazeRay = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingGazeRay(ref gazeRay);
        if (timeCounter >= time_frequency && isEyeTrackingData && isGazeRay)
        {
            timeCounter = 0.0f;

            var gazeData = new EyeGazeData
            {
                gaze_location = gazeRay.Origin,
                gaze_direction = gazeRay.Direction,
                left_blink = eyeTrackingData.leftEyeOpenness,
                right_blink = eyeTrackingData.rightEyeOpenness,
                // isLeftEyeBlinking = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).IsLeftEyeBlinking,
                // isRightEyeBlinking = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).IsRightEyeBlinking,
                timestamp = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local).Timestamp,
                time = Time.time
            };
            Debug.Log(string.Format("AcquireEyeTrackingData - gazeData: {0}", gazeData.gaze_location));

            // if (eyeTrackingData.leftEyeOpenness == 0) ExperimentController.markerStream.Write("LEFT_BLINK");
            // if (eyeTrackingData.rightEyeOpenness == 0) ExperimentController.markerStream.Write("RIGHT_BLINK");

            // var eyeData = new EyeData();

            // var error = SRanipal_Eye_API.GetEyeData(ref eyeData);

            // if (error == ViveSR.Error.WORK)
            // {
            //   gazeData.pupil_dilatation_left = eyeData.verbose_data.left.pupil_diameter_mm;
            //   gazeData.pupil_dilatation_right = eyeData.verbose_data.right.pupil_diameter_mm;
            // }

            if (TobiiXR.FocusedObjects.Count > 0)
            {
                // Do something with the focused game object
                gazeData.objName = TobiiXR.FocusedObjects[0].GameObject.name;
            }

            // Logger.log.eyeGazeData.Add(gazeData);
            gazeData.LogToFile(LogFile);
            //dataManager.AddGazeData(gazeData);

        }
    }

    [System.Serializable]
    public class EyeGazeData
    {
        public Vector3 gaze_location;
        public Vector3 gaze_direction;
        public float left_blink;
        public float right_blink;
        public float timestamp;
        public float time;
        // public bool isLeftEyeBlinking;
        // public bool isRightEyeBlinking;
        public string objName;
        private bool logged = false;

        public void LogToFile(string fileName)
        {
            //  string thisJson = JsonUtility.ToJson(this);
            //  System.IO.File.AppendAllText(fileName, thisJson + "\n");
            if (!logged) ExperimentController.logWrapper.AddToLog(this);
            logged = true;
        }

        public EyeGazeData() { }
    }
}
