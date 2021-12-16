using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;
using JetBrains.Annotations;

public class LogOnGaze : MonoBehaviour, IGazeFocusable
{
    ObjectGaze currentGaze = new ObjectGaze();
    string LogFile;

    // Start is called before the first frame update
    void Start()
    {
        LogFile = Application.persistentDataPath + "/Experiment-" + System.DateTime.Now.ToShortDateString().Replace('/', '-') + "-" + System.DateTime.Now.ToShortTimeString().Replace(':', '-') + ".json";

    }

    // Update is called once per frame
    void Update()
    {
    }
    //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
    public void GazeFocusChanged(bool hasFocus)
    {
        //If this object received focus, LSL SAGAT-on and set start time and object name for log
        if (hasFocus)
        {
            // ExperimentController.markerStream.Write("gzOn" + gameObject.name);
            currentGaze.startTime = Time.time;
        }
        //If this object lost focus, LSL SAGAT-off and set end time and log.
        else
        {
            currentGaze.objName = this.gameObject.transform.parent.name + "_" + this.gameObject.name;
            currentGaze.endTime = Time.time;
            // ExperimentController.markerStream.Write("gzOff" + gameObject.name);
            // Logger.log.AddToLog(currentGaze);
            currentGaze.LogToFile(LogFile);
            currentGaze = new ObjectGaze();
        }
    }
    [System.Serializable]
    public class ObjectGaze
    {
        public float startTime;
        public float endTime;
        public string objName;
        private bool logged = false;

        public void LogToFile(string fileName)
        {
            //  string thisJson = JsonUtility.ToJson(this);
            //  System.IO.File.AppendAllText(fileName, thisJson + "\n");
            if (!logged) ExperimentController.logWrapper.AddToLog(this);
            logged = true;
        }

        public ObjectGaze() { }
    }
}


