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
            currentGaze.stimulusType = GameObject.Find("ExperimentController").GetComponent<ExperimentController>().currentTrial.stimulusType;
        }
        //If this object lost focus, LSL SAGAT-off and set end time and log.
        else
        {
            currentGaze.symbol = GameObject.Find("ExperimentController").GetComponent<ExperimentController>().currentTrial.symbol;
            currentGaze.gridType = GameObject.Find("ExperimentController").GetComponent<ExperimentController>().currentTrial.gridType;
            if (this.gameObject.name.Contains("_square") || this.gameObject.name.Contains("_diamond"))
            {
                if (currentGaze.gridType == "square") currentGaze.objName = this.gameObject.name.Replace("_diamond", "");
                else if (currentGaze.gridType == "diamond") currentGaze.objName = this.gameObject.name.Replace("_square", "");
                else currentGaze.objName = this.gameObject.name.Replace("_square_diamond", "");
            }
            else currentGaze.objName = this.gameObject.transform.parent.name + "_" + this.gameObject.name;
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
        public string symbol;
        public string stimulusType;
        public string gridType;
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


