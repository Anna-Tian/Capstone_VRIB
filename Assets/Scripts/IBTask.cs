using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pvr_UnitySDKAPI;

public class IBTask : MonoBehaviour
{
    public GameObject failedText;
    public GameObject unexpectedObject;
    Transform stimDot;
    public int unexpectedStimNum;
    int unexpStimDone = 0;
    float lastTime;
    float delay;
    int phaseNum = 1;
    int trialsDone = -1;
    public bool taskRunning = false;
    public IBStimulus currentStim = new IBStimulus(0);
    IBPhase currentPhase = new IBPhase(0, 0);
    Color initDotCol;
    int stimDuration = 2;
    int stimWaitingTime = 5;
    int[] random0 = { 0, 1, 2, 3, 4, 5, 6, 7 };
    int[] random1 = { 1, 2 };
    double[] random2 = { 1.3, 1.6, 1.9 };
    bool isSetupStimulus = false;
    double unexpDelay;
    public Logger.Condition condition;

    bool firstTimeSetup = true;
    float firstTrailTime;
    // Start is called before the first frame update
    void Start()
    {
        delay = random1[Random.Range(0, random1.Length)]; //stimulus time delay
        lastTime = Time.time;
        initDotCol = GameObject.Find("Dots").transform.GetChild(0).GetComponent<Image>().color;
        unexpectedObject.GetComponent<MeshRenderer>().enabled = false;
        Logger.log = new Logger.LogWrapper();
        condition.conditionName = "training";
        failedText = GameObject.Find("FailedText");
        failedText.active = false;
    }

    private void OnDisable()
    {
        Logger.log.LogToFile(Logger.logFileName.TrimEnd(".json".ToCharArray()) + "-" + condition.conditionName + ".json");
        // Logger.log.LogToFile("Test");
    }

    // Update is called once per frame
    void Update()
    {
        if (firstTimeSetup)
        {
            SetupControls();
            firstTimeSetup = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.B))
        {
            Application.Quit();
            Logger.log.LogToFile(Logger.logFileName.TrimEnd(".json".ToCharArray()) + "-" + condition.conditionName + ".json");
        }

        if (taskRunning)
        {
            if (isSetupStimulus && (lastTime + delay < Time.time)) // stimulus appears
            {
                SetupStimulus();
                isSetupStimulus = false;
                Debug.Log(string.Format("time: {0}; dot: {1}", Time.time, stimDot.name));
                currentStim = new IBStimulus(Time.time);
                currentStim.targetDirection = stimDot.position;
                currentStim.targetEndTime = Time.time + stimDuration;
            }
            else if (Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.X)) // notice stimulus
            {
                // Change VR statement to Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(0 , Pvr_KeyCode.X)
                // Change PC statement to Input.GetKeyDown(KeyCode.Alpha1)
                currentStim.noticeTime = Time.time;
                Debug.Log(string.Format("Notice stimulus time: {0}", Time.time));
            }
            else if (Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.A)) // find stimulus
            {
                // Change VR statement to Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(1 , Pvr_KeyCode.A)
                // Change PC statement to Input.GetKeyDown(KeyCode.Alpha2)
                completeStim(true);
                Debug.Log(string.Format("currentStim: {0}", currentStim));
            }
            // failed stimulus after stimWaitingTime
            if (!isSetupStimulus)
            {
                if (failedText.active && Time.time > currentStim.targetStartTime + stimWaitingTime + 2)
                {
                    failedText.active = false;
                    completeStim(false);
                    Debug.Log(string.Format("expectTime: {0}; actualTime: {1}", currentStim.targetStartTime + stimWaitingTime, Time.time));

                    Debug.Log(string.Format("FailedText disabled"));
                }
                else if (Time.time > currentStim.targetStartTime + stimWaitingTime)
                {
                    failedText.active = true;
                    Debug.Log(string.Format("FailedText enabled"));
                }
            }
            if (trialsDone == 4)
            {
                taskRunning = false;
                unexpStimDone = 0;
                if (trialsDone >= 4 || (trialsDone == 5 && phaseNum == 0)) NextPhase();
                if (phaseNum == 4) phaseNum = 4; //Finish Task
                trialsDone = -1;
            }
            // default stimulus after the stimDuration time
            if (currentStim.targetStartTime + stimDuration < Time.time || currentStim.pointingTime != 0)
            {
                stimDot.GetComponent<AudioSource>().Stop();
                stimDot.GetComponent<Image>().color = initDotCol;
            }
            // flashing unexpected object
            if ((unexpStimDone < unexpectedStimNum) && (trialsDone == 2))
            {
                // Debug.Log(string.Format("unexpStimDone: {0}", unexpStimDone));
                // Debug.Log(string.Format("time: {0}; lastTime: {1}: unexpDelay: {2}", Time.time, lastTime, unexpDelay));

                if (unexpectedObject.GetComponent<MeshRenderer>().enabled && (currentStim.targetStartTime + unexpDelay + 3 < Time.time))
                {
                    unexpectedObject.GetComponent<MeshRenderer>().enabled = false;
                    unexpDelay = random2[Random.Range(0, random2.Length)]; //stimulus time delay
                    unexpStimDone++;
                    Debug.Log(string.Format("unexpStim - unexpStimDone: {0}", unexpStimDone));
                    Debug.Log(string.Format("unexpStim - enabled: false"));
                }
                else if ((lastTime + unexpDelay < Time.time))
                {
                    unexpectedObject.GetComponent<MeshRenderer>().enabled = true;
                    Debug.Log(string.Format("unexpStim - enabled: true"));
                }
            }
            else
            {
                unexpectedObject.GetComponent<MeshRenderer>().enabled = false;
                Debug.Log(string.Format("unexpStim else - enabled: false"));
            }
        }
        if (trialsDone == -1 && Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.Y))
        {
            // Change VR statement to Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(0 , Pvr_KeyCode.Y)
            // Change PC statement to Input.GetKeyDown(KeyCode.Space)
            taskRunning = true;
            isSetupStimulus = true;
            unexpDelay = random2[Random.Range(0, random2.Length)]; //stimulus time delay
            trialsDone++;
            // phaseNum = -1;
            firstTrailTime = Time.time;
        }
    }

    void NextPhase()
    {
        currentPhase = new IBPhase(phaseNum, firstTrailTime);
        phaseNum++;
        currentPhase.endtime = Time.time;
        Logger.log.AddToLog(currentPhase);
        //currentPhase.LogToFile(ExperimentController.instance.LogFile);
        // Logger.markerStream.Write("phase end " + phaseNum);
    }
    void completeStim(bool isFound)
    {
        isSetupStimulus = true;
        delay = random1[Random.Range(0, random1.Length)]; //stimulus time delay

        lastTime = Time.time;
        trialsDone++;
        // Debug.Log(string.Format("Find stimulus time: {0}; trialsDone: {1}", Time.time, trialsDone));

        if (isFound)
        {
            currentStim.pointingTime = Time.time;
            currentStim.pointingDirection = Pvr_UnitySDKAPI.Controller.UPvr_GetControllerPOS(1);

        }
        else
        {
            currentStim.noticeTime = 0;
            currentStim.pointingTime = 0;
            currentStim.pointingDirection = new Vector3();
        }

        Logger.log.AddToLog(currentStim);
    }

    void SetupStimulus()
    {
        int dot = random0[Random.Range(0, random0.Length)]; // 8 different dots
        stimDot = GameObject.Find("Dots").transform.GetChild(dot);
        stimDot.GetComponent<AudioSource>().Play();
        Color dotCol = stimDot.GetComponent<Image>().color;
        dotCol = Color.green;
        stimDot.GetComponent<Image>().color = dotCol;
        failedText.active = false;
    }

    #region Controller Input

    // public SteamVR_Action_Boolean NoticeStimulus;
    // void HandleNoticeTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    // {
    //     if (currentStim.noticeTime == -1)
    //     {
    //         NoticeStim();
    //     }
    // }

    void SetupControls()
    {
        // NoticeStimulus.AddOnStateDownListener(HandleNoticeTriggerDown, SteamVR_Input_Sources.LeftHand);
    }
    void RemoveControls()
    {

    }
    #endregion

    #region Classes
    [System.Serializable]
    public class IBStimulus
    {
        public float targetStartTime;
        public float targetEndTime;
        public float noticeTime;
        public float pointingTime;
        public Vector3 targetDirection;
        public Vector3 pointingDirection;

        public IBStimulus(float start)
        {
            targetStartTime = start;
        }
        public void LogToFile(string filename)
        {
            //string thisJson = JsonUtility.ToJson(this);
            //System.IO.File.AppendAllText(filename, thisJson + "\n");
            //if (!logged) ExperimentController.logWrapper.AddToLog(this);
            //logged = true;
        }
    }
    [System.Serializable]
    public class IBPhase
    {
        public int phaseNum;
        public float starttime;
        public float endtime;
        private bool logged = false;

        public IBPhase(int inPhase, float start)
        {
            phaseNum = inPhase;
            starttime = start;
        }
        public void LogToFile(string filename)
        {
            //string thisJson = JsonUtility.ToJson(this);
            //System.IO.File.AppendAllText(filename, thisJson + "\n");
            //if (!logged) ExperimentController.logWrapper.AddToLog(this);
            //logged = true;
        }
    }
    #endregion
}

/*
//generate char grid - random, then if diamond/square, alter relevant chars
//rotate ring
//set circle on ring to dim
//
//
1 run = 60 stimuli
Practice Phase: five 1-minute runs.
Phase 1-3: ten 1-minute runs
Break after every run
Big break after 5 runs

*/