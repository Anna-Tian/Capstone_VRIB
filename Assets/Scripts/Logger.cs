using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using Assets.LSL4Unity.Scripts;
using Valve.Newtonsoft.Json;
using System.Linq;
using System;
using Valve.Newtonsoft.Json.Linq;

public class Logger : MonoBehaviour
{
  static public string logFileName;
  static public LogWrapper log = new LogWrapper();
  static public LSLMarkerStream markerStream;
  static public JsonSerializerSettings serializerSettings = new JsonSerializerSettings();

  [Tooltip("Frequency of the aquisition of camera data")]
  public float camSnapshotFrequency;
  public bool logCamera;

  [Tooltip("Write log in real time instead of on application quit")]
  public bool realTimeLogging;
  // Start is called before the first frame update
  void Start()
  {
    logFileName = Application.persistentDataPath + "/Experiment-" + System.DateTime.Now.ToShortDateString().Replace('/', '-') + "-" + System.DateTime.Now.ToShortTimeString().Replace(':', '-') + ".json";
    Debug.Log(string.Format("logFileName: {0}", logFileName));

    markerStream = FindObjectOfType<LSLMarkerStream>();
    markerStream.gameObject.active = false;
    markerStream.gameObject.active = true;

    serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

    // OldLogToNew("Experiment-11-10-2020-3-12 PM non stress.json");
    //OldLogToNew("Experiment-11-10-2020-3-28 PM stress.json");
    // OldLogToNew("Experiment-11-10-2020-3-42 PM stress 2.json");
  }

  // Update is called once per frame
  void Update()
  {
    if (log.needToWrite() && realTimeLogging) log.LogToFile(logFileName);
    if ((Time.time >= CameraSnapshot.lastSnapshot + 1 / camSnapshotFrequency) && logCamera)
    {
      Logger.log.cameraSnapshots.Add(new CameraSnapshot());
      CameraSnapshot.lastSnapshot = Time.time;
    }
  }

  private void OnApplicationQuit()
  {
    log.LogToFile(logFileName);
  }

  #region Classes
  [System.Serializable]
  public class TrialWrapper
  {
    public string classname;
    public object trial;
    //public string serializedObject;
    public TrialWrapper(object inTrial)
    {
      trial = inTrial;
      //serializedObject = Valve.Newtonsoft.Json.JsonConvert.SerializeObject(trial);
      if (inTrial != null) classname = inTrial.GetType().FullName;
    }
  }
  [System.Serializable]
  public class LogWrapper
  {
    public List<TrialWrapper> trials;
    public List<EyeTrackingLogger.EyeGazeData> eyeGazeData;
    public List<CameraSnapshot> cameraSnapshots;
    public LogWrapper()
    {
      trials = new List<TrialWrapper>();
      eyeGazeData = new List<EyeTrackingLogger.EyeGazeData>();
      cameraSnapshots = new List<CameraSnapshot>();
    }
    private bool writeflag = false;
    public void LogToFile(string filename)
    {
      //string thisJson = JsonUtility.ToJson(this);
      string thisJson = Valve.Newtonsoft.Json.JsonConvert.SerializeObject(this, Logger.serializerSettings);
      System.IO.File.WriteAllText(filename, thisJson);// + "\n");
      writeflag = false;
    }
    public void AddToLog(object inTrial)
    {
      trials.Add(new TrialWrapper(inTrial));
      writeflag = true;
    }
    public bool needToWrite()
    {
      return writeflag;
    }
  }

  [System.Serializable]
  public class Condition
  {
    public string conditionName;
    public float starttime;
    public float endtime;
  }

  [System.Serializable]
  public class CameraSnapshot
  {
    public static float lastSnapshot = 0;
    public Vector3 pos;
    public Vector3 rot;
    // public float[] pos;
    // public float[] rot;
    public float time;

    public CameraSnapshot()
    {
      Transform cam = Camera.main.transform;
      pos = cam.position;
      rot = cam.rotation.eulerAngles;
      //pos = new float[3]{ cam.position.x, cam.position.y, cam.position.z};
      //rot = new float[3]{ cam.rotation.eulerAngles.x, cam.rotation.eulerAngles.y, cam.rotation.eulerAngles.z };
      time = Time.time;
    }
  }
  #endregion

  void OldLogToNew(string filename)
  {
    string filepath = Application.persistentDataPath + "/" + filename;
    string fileJson = System.IO.File.ReadAllText(filepath);
    LogWrapper oldLog = Valve.Newtonsoft.Json.JsonConvert.DeserializeObject<LogWrapper>(fileJson, Logger.serializerSettings);
    List<TrialWrapper> delList = new List<TrialWrapper>();
    LogWrapper newLog = new LogWrapper();
    JsonSerializer serializer = new JsonSerializer();
    foreach (TrialWrapper wrapper in oldLog.trials)
    {
      switch (wrapper.classname)
      {
        case "EyeTrackingLogger+EyeGazeData":
          {
            EyeTrackingLogger.EyeGazeData data = (EyeTrackingLogger.EyeGazeData)serializer.Deserialize(new JTokenReader((JToken)wrapper.trial), typeof(EyeTrackingLogger.EyeGazeData));
            newLog.eyeGazeData.Add((EyeTrackingLogger.EyeGazeData)data);
            break;
          }
        case "Logger+CameraSnapshot":
          {
            CameraSnapshot data = (CameraSnapshot)serializer.Deserialize(new JTokenReader((JToken)wrapper.trial), typeof(CameraSnapshot));
            newLog.cameraSnapshots.Add((CameraSnapshot)data);
            break;
          }
        default:
          {
            newLog.trials.Add(wrapper);
            break;
          }
      }
    }
    string newJson = Valve.Newtonsoft.Json.JsonConvert.SerializeObject(newLog, Logger.serializerSettings);
    string newFileName = filename.TrimEnd(".json".ToCharArray()) + "-newFormat.json";
    string newFilePath = Application.persistentDataPath + "/" + newFileName;
    System.IO.File.WriteAllText(newFilePath, newJson);

  }
}

