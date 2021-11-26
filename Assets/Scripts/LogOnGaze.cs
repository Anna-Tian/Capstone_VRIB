using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;
using JetBrains.Annotations;

public class LogOnGaze : MonoBehaviour, IGazeFocusable
{
    ObjectGaze currentGaze = new ObjectGaze();
    // Start is called before the first frame update
    void Start()
    {
        
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
            currentGaze.objName = this.gameObject.name;
            Logger.markerStream.Write("gzOn" + gameObject.name);
            currentGaze.startTime = Time.time;
        }
        //If this object lost focus, LSL SAGAT-off and set end time and log.
        else
        {
            currentGaze.endTime = Time.time;
            Logger.markerStream.Write("gzOff" + gameObject.name);
            Logger.log.AddToLog(currentGaze);
            currentGaze = new ObjectGaze();
        }
    }
    [System.Serializable]
    public class ObjectGaze
    {
        public float startTime;
        public float endTime;
        public string objName;
    }
}


