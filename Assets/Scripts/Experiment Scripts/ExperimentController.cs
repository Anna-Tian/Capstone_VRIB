using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using SickscoreGames.HUDNavigationSystem;
using System.Linq;
using Pvr_UnitySDKAPI;
using LSL;
using Assets.LSL4Unity.Scripts;
using Valve.VR;

// [ExecuteInEditMode]
public class ExperimentController : MonoBehaviour
{
    public const bool DEBUG = false;
    public bool isTesting;
    public bool isTestingDone;

    public SteamVR_Action_Boolean AAA;
    public bool isAAA;
    public SteamVR_Input_Sources handType;

    public Material materialWindowLight;
    public Material materialWindow;
    private GameObject[,] squareGrid = new GameObject[20, 20];

    public GameObject North;
    public GameObject East;
    public GameObject South;
    public GameObject West;
    public GameObject UIDesigns;
    public GameObject crosshair;
    GameObject menuScreen;
    public bool menuOpen = true;
    private int instructionMenuNum = 0;
    public bool EventRunning = false;
    bool experimentRunning = false;
    public string LogFile;
    public ExperimentTrial currentTrial;
    Experiment currentExp;
    public bool inTraining = false;
    bool inInterim = false;
    float stimDelay;
    float lastTime;
    public int trialsDone = 0;

    int trialsCount;
    int[] trialsArray;
    public GameObject HUDIcon;
    public bool isAudioTrialStart = false;
    int phaseNum = 1;
    int[] randomUnexpArray = new int[10];
    int unexpsDone = 0;
    bool isBgAudioStart = false;
    public bool isQtnDone = false;

    enum ExperimentState { Startup, SymbolTraining, Training, Experiment, Break, Complete, TrainingImage, Questionnaire };
    ExperimentState currentState = ExperimentState.Startup;

    static public LogWrapper logWrapper = new LogWrapper();
    //static public LSLMarkerStream markerStream;

    // Start is called before the first frame update
    void Start()
    {
        currentTrial = new ExperimentTrial(Time.time, 0f, "", "", "");
        Cursor.lockState = CursorLockMode.Confined;

        LogFile = Application.persistentDataPath + "/Experiment-" + System.DateTime.Now.ToShortDateString().Replace('/', '-') + "-" + System.DateTime.Now.ToShortTimeString().Replace(':', '-') + ".json";
        //GUIUtility.systemCopyBuffer = "Test";

        menuScreen = transform.Find("ExperimentObjects").Find("Menu Screen").gameObject;
        menuScreen.SetActive(true);

        Camera.main.GetComponent<CameraController>().enabled = false;
        lastTime = Time.time;
        randomUnexpArray = GenerateRandomArray(10, randomUnexpArray);
        InitGrid();

        //markerStream = FindObjectOfType<LSLMarkerStream>();
        //markerStream.gameObject.SetActive(false);
        //markerStream.gameObject.SetActive(true);
        AAA.AddOnStateDownListener(AAAMethod, handType);
    }

    // Update is called once per frame

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            // logWrapper.LogToFile(LogFile);
        }

        #region Symbol Training->Training Menu
        if ((isAAA|| Input.GetKeyDown(KeyCode.Tab)) && currentState == ExperimentState.SymbolTraining && !menuOpen && trialsDone >= 15)
        {
            trialsDone = 0;
            transform.Find("ExperimentObjects").Find("Pairing Screen").gameObject.SetActive(false);

            menuScreen.SetActive(true);
            menuScreen.transform.Find("Menu Slides").Find("Training").gameObject.SetActive(true);
            menuOpen = true;

            currentState = ExperimentState.TrainingImage;
        }
        #endregion
        #region Training Image->Training
        if ((isAAA || Input.GetKeyDown(KeyCode.Tab)) && currentState == ExperimentState.Training && !menuOpen)
        {
            EventRunning = true;
            inTraining = true;
            currentState = ExperimentState.Training;

            trialsCount = 60;
            trialsArray = new int[60];
            trialsArray = GenerateRandomArray(trialsCount, trialsArray);
            stimDelay = Random.Range(0.6f, 0.8f);

            string result = "";
            foreach (int item in trialsArray) result += item + ", ";

            transform.Find("ExperimentObjects").Find("NoticeScreen").gameObject.SetActive(true);

            currentTrial = new ExperimentTrial(Time.time, 0f, "", "", ""); // reset to stop noticeStimulus from firing
            currentTrial.endTime = Time.time;
            currentTrial.startTime = Time.time + stimDelay;
            crosshair.GetComponent<Image>().color = Color.red;
        }
        #endregion
        #region Training->Experiment
        //move from Training to Experiment
        if (!experimentRunning && !inTraining && trialsDone == trialsCount && currentState == ExperimentState.Training)
        {
            North.SetActive(false);
            South.SetActive(false);
            East.SetActive(false);
            West.SetActive(false);
            for (int type = 0; type < UIDesigns.transform.childCount; type++)
            {
                UIDesigns.transform.GetChild(type).gameObject.GetComponent<AudioSource>().Stop();
            }
            trialsDone = 0;
            logWrapper.RemoveUselessTrial();

            inInterim = false;
            EventRunning = true;
            //currentTrial.endtime = Time.time;
            currentTrial = new ExperimentTrial(Time.time, 0f, "", "", "");
            currentTrial.startTime = Time.time + stimDelay;
            //currentTrial.endtime = Time.time;
            currentExp = new Experiment(Time.time);
            currentExp.LogToFile(LogFile);
            // markerStream.Write("training end ");

            trialsCount = 600;
            trialsArray = new int[600];
            trialsArray = GenerateRandomArray(trialsCount, trialsArray);
            stimDelay = Random.Range(0.6f, 0.8f);

            menuScreen.SetActive(true);
            menuScreen.transform.Find("Menu Slides").Find("Experiment").gameObject.SetActive(true);
            menuOpen = true;

            crosshair.GetComponent<Image>().color = Color.red;
            experimentRunning = true;
            currentState = ExperimentState.Experiment;
        }
        #endregion
        #region Questionnaire->Interim/EndScreen
        if (currentState == ExperimentState.Questionnaire && isQtnDone)
        {
            isQtnDone = false;
            inInterim = true;
            currentState = ExperimentState.Break;
            menuOpen = true;
            menuScreen.transform.Find("Menu Slides").Find("Questionnaire").gameObject.SetActive(false);
            menuScreen.transform.Find("Menu Slides").Find("Interim").gameObject.SetActive(true);

            if (phaseNum < 2)
            {
                phaseNum++;
                // markerStream.Write("experiment " + phaseNum + " end");
            }
            else
            {
                inInterim = false;
                menuScreen.SetActive(false);
                menuOpen = false;
                currentState = ExperimentState.Complete;
                GUIUtility.systemCopyBuffer = System.IO.File.ReadAllText(LogFile);
                transform.Find("ExperimentObjects").Find("End Screen").gameObject.SetActive(true);
                EnableNavSystem(false);
                // markerStream.Write("experiment " + phaseNum + " end");
            }
        }
        #endregion
        #region Interim ->Next Phase
        if ((isAAA|| Input.GetKeyDown(KeyCode.Tab)) && !experimentRunning && inInterim && currentState == ExperimentState.Break)
        {
            trialsDone = 0;
            EventRunning = true;
            experimentRunning = true;
            inInterim = false;
            currentState = ExperimentState.Experiment;
            logWrapper.RemoveUselessTrial();

            trialsCount = 600;
            trialsArray = new int[600];
            trialsArray = GenerateRandomArray(trialsCount, trialsArray);
            stimDelay = Random.Range(0.6f, 0.8f);

            currentTrial = new ExperimentTrial(Time.time, 0f, "", "", ""); // reset to stop noticeStimulus from firing
            currentTrial.endTime = Time.time;
            currentTrial.startTime = Time.time + stimDelay;
            crosshair.GetComponent<Image>().color = Color.red;
            currentExp = new Experiment(Time.time);
            currentExp.LogToFile(LogFile);
        }
        #endregion

        #region Experiment Update
        if (experimentRunning && !menuOpen)
        {
            inTraining = false;
            transform.Find("ExperimentObjects").Find("NoticeScreen").gameObject.SetActive(true);
            if (EventRunning)
            {
                currentTrial.tag = phaseNum == 1 ? "experiment 1" : "experiment 2";
                playStimulus();
                if (!isBgAudioStart)
                {
                    isBgAudioStart = true;
                    transform.GetComponent<AudioSource>().Play();
                }

                // will result incorrect data for last trial in case (trialsDone == trialsCount)
                if (trialsDone == trialsCount + 1 || isAAA || Input.GetKeyDown(KeyCode.S))
                {
                    EventRunning = false;
                    trialsDone = trialsCount;
                    experimentRunning = false;
                    for (int type = 0; type < UIDesigns.transform.childCount; type++)
                    {
                        UIDesigns.transform.GetChild(type).gameObject.GetComponent<AudioSource>().Stop();
                    }
                    transform.GetComponent<AudioSource>().Stop();
                    EnableNavSystem(false);
                    isBgAudioStart = false;
                    currentExp.endtime = Time.time;
                    currentExp.LogToFile(LogFile);
                    logWrapper.RemoveUselessTrial();

                    // trialsCount = 600;
                    // trialsArray = new int[600];
                    // trialsArray = GenerateRandomArray(trialsCount, trialsArray);

                    transform.Find("ExperimentObjects").Find("NoticeScreen").gameObject.SetActive(false);
                    crosshair.GetComponent<Image>().color = Color.blue;

                    currentState = ExperimentState.Questionnaire;
                    menuScreen.SetActive(true);
                    menuScreen.transform.Find("Menu Slides").Find("Questionnaire").gameObject.SetActive(true);
                }

                if (isAAA  || Input.GetKeyDown(KeyCode.Space))
                {
                    NoticeStimulus(Time.time);
                }

            }
        }
        #endregion
        #region Training Update
        if (inTraining && !menuOpen)
        {
            if (EventRunning)
            {
                currentTrial.tag = "training";
                playStimulus();
                if (!isBgAudioStart)
                {
                    isBgAudioStart = true;
                    transform.GetComponent<AudioSource>().Play();
                }

                // will result incorrect data for last trial in case (trialsDone == trialsCount)
                // skip for debugging
                if (trialsDone == trialsCount + 1 || isAAA || Input.GetKeyDown(KeyCode.S))
                {
                    inTraining = false;
                    EventRunning = false;
                    trialsDone = trialsCount;
                    transform.GetComponent<AudioSource>().Stop();
                    EnableNavSystem(false);
                    isBgAudioStart = false;
                    transform.Find("ExperimentObjects").Find("NoticeScreen").gameObject.SetActive(false);
                }

                if (isAAA  || Input.GetKeyDown(KeyCode.Space))
                {
                    NoticeStimulus(Time.time);
                }
            }
        }
        #endregion

        if (logWrapper.needToWrite()) logWrapper.LogToFile(LogFile);

        if (trialsDone >= 15 && !experimentRunning)
        {
            transform.Find("ExperimentObjects").Find("Pairing Screen").Find("Continue").gameObject.SetActive(true);
        }
    }
    // private void Start()
    // {
    //     InitGrid();
    // }
    // private void Update()
    // {
    //     if (isTesting)
    //     {
    //         ChangeGrid(1);
    //     }
    //     if (isTestingDone)
    //     {
    //         // ChangeGrid(3);
    //         InitGrid();
    //     }
    // }

    private void AAAMethod(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (Input.GetKeyDown(KeyCode.Tab) || menuOpen)
        {
            menuScreen.SetActive(false);
            menuOpen = false;

            foreach (Transform child in menuScreen.transform.Find("Menu Slides"))
            {
                child.gameObject.SetActive(false);
            }

            if (currentState == ExperimentState.Startup)
            {
                menuScreen.SetActive(true);
                menuScreen.transform.Find("Menu Slides").Find("Symbol Training").gameObject.SetActive(true);
                menuOpen = true;
                currentState = ExperimentState.SymbolTraining;
            }

            if (currentState == ExperimentState.SymbolTraining && !menuOpen)
            {
                Camera.main.GetComponent<CameraController>().enabled = true;
                transform.Find("ExperimentObjects").Find("Pairing Screen").gameObject.SetActive(true);
            }

            if (currentState == ExperimentState.TrainingImage)
            {
                if (instructionMenuNum < 4)
                {
                    menuScreen.SetActive(true);
                    menuOpen = true;
                    menuScreen.transform.Find("Menu Slides").Find("Training Images").gameObject.SetActive(true);
                    menuScreen.transform.Find("Menu Slides").Find("Training Images").GetChild(++instructionMenuNum).gameObject.SetActive(true);
                    menuScreen.transform.Find("Menu Slides").Find("Training Images").Find("Title").GetComponent<Text>().text = string.Format("Task Instruction ({0}/3)", instructionMenuNum);
                    if (instructionMenuNum == 3) menuScreen.transform.Find("Menu Slides").Find("Training Images").Find("NextInstruction").GetComponent<Text>().text = "Press \"A\" from controller to continue to start Training";
                }
                if (instructionMenuNum == 4)
                {
                    menuScreen.SetActive(false);
                    menuOpen = false;
                    menuScreen.transform.Find("Menu Slides").Find("Training Images").gameObject.SetActive(false);
                    currentState = ExperimentState.Training;
                }
            }

            stimDelay = Random.Range(0.6f, 0.8f);
            currentTrial = new ExperimentTrial(Time.time, 0f, "", "", ""); // reset to stop noticeStimulus from firing
            currentTrial.endTime = Time.time;
            currentTrial.startTime = Time.time + stimDelay;
            //Camera.main.transform.localRotation = Quaternion.identity;
        }
    }
    private void playStimulus()
    {
        if (lastTime + stimDelay < Time.time)
        {
            currentTrial.result = currentTrial.symbol == "Human Speech" ?
                                    (currentTrial.noticeTime != 0 ?
                                        (currentTrial.stimulusType == "Visual" ? 111 :
                                         currentTrial.stimulusType == "Audio" ? 112 : 113) :
                                        (currentTrial.stimulusType == "Visual" ? 121 :
                                         currentTrial.stimulusType == "Audio" ? 122 : 123)) :
                                    (currentTrial.noticeTime != 0 ?
                                        (currentTrial.stimulusType == "Visual" ? 211 :
                                         currentTrial.stimulusType == "Audio" ? 212 : 213) :
                                        (currentTrial.stimulusType == "Visual" ? 221 :
                                         currentTrial.stimulusType == "Audio" ? 222 : 223));
            currentTrial.endTime = Time.time;

            if (stimDelay > 0.5f && stimDelay < 0.9f)
            {
                currentTrial = new ExperimentTrial(Time.time, 0f, "", "", "");
                stimDelay = 0.3f;
                if (trialsDone != trialsCount) CreateEvent();

                currentTrial.startTime = Time.time;
                trialsDone++;

                if (unexpsDone == 10)
                {
                    randomUnexpArray = GenerateRandomArray(10, randomUnexpArray);
                    unexpsDone = 0;
                }
                if (randomUnexpArray[unexpsDone] < 5) ChangeGrid(0);
                else if (randomUnexpArray[unexpsDone] >= 5 && randomUnexpArray[unexpsDone] < 9) ChangeGrid(1);
                else
                {
                    ChangeGrid(2);
                }
                unexpsDone++;
            }
            else
            {
                North.SetActive(false);
                South.SetActive(false);
                East.SetActive(false);
                West.SetActive(false);
                for (int type = 0; type < UIDesigns.transform.childCount; type++)
                {
                    UIDesigns.transform.GetChild(type).gameObject.GetComponent<AudioSource>().Stop();
                }
                EnableNavSystem(false);

                crosshair.GetComponent<Image>().color = Color.yellow;
                stimDelay = Random.Range(0.6f, 0.8f);
                ChangeGrid(3);

                transform.Find("ExperimentObjects").Find("NoticeScreen").Find("AudioName").GetComponent<Text>().text = "";
            }
            lastTime = Time.time;
            currentTrial.targetEndTime = Time.time;
            currentTrial.LogToFile(LogFile);

        }

    }

    private void ChangeGrid(int type)
    {
        // set random background
        InitGrid();
        switch (Random.Range(1, 5))
        {
            case 1:
                for (int evenRow = 0, oddRow = 2; evenRow < 20 && oddRow < 20; evenRow += 4, oddRow += 4)
                {
                    for (int evenCol = 0, oddCol = 1; evenCol < 20 && oddCol < 20; evenCol += 2, oddCol += 2)
                    {
                        squareGrid[evenRow, evenCol].GetComponent<MeshRenderer>().material = Random.Range(0, 2) == 1 ? materialWindowLight : materialWindow;
                        squareGrid[oddRow, oddCol].GetComponent<MeshRenderer>().material = Random.Range(0, 2) == 1 ? materialWindowLight : materialWindow;
                    }
                }
                break;
            case 2:
                for (int evenRow = 0, oddRow = 2; evenRow < 20 && oddRow < 20; evenRow += 4, oddRow += 4)
                {
                    for (int evenCol = 0, oddCol = 1; evenCol < 20 && oddCol < 20; evenCol += 2, oddCol += 2)
                    {
                        squareGrid[evenRow, evenCol + 1].GetComponent<MeshRenderer>().material = Random.Range(0, 2) == 1 ? materialWindowLight : materialWindow;
                        squareGrid[oddRow, oddCol - 1].GetComponent<MeshRenderer>().material = Random.Range(0, 2) == 1 ? materialWindowLight : materialWindow;
                    }
                }
                break;
            case 3:
                for (int evenRow = 1, oddRow = 3; evenRow < 20 && oddRow < 20; evenRow += 4, oddRow += 4)
                {
                    for (int evenCol = 0, oddCol = 1; evenCol < 20 && oddCol < 20; evenCol += 2, oddCol += 2)
                    {
                        squareGrid[evenRow, evenCol].GetComponent<MeshRenderer>().material = Random.Range(0, 2) == 1 ? materialWindowLight : materialWindow;
                        squareGrid[oddRow, oddCol].GetComponent<MeshRenderer>().material = Random.Range(0, 2) == 1 ? materialWindowLight : materialWindow;
                    }
                }
                break;
            case 4:
                for (int evenRow = 1, oddRow = 3; evenRow < 20 && oddRow < 20; evenRow += 4, oddRow += 4)
                {
                    for (int evenCol = 0, oddCol = 1; evenCol < 20 && oddCol < 20; evenCol += 2, oddCol += 2)
                    {
                        squareGrid[evenRow, evenCol + 1].GetComponent<MeshRenderer>().material = Random.Range(0, 2) == 1 ? materialWindowLight : materialWindow;
                        squareGrid[oddRow, oddCol - 1].GetComponent<MeshRenderer>().material = Random.Range(0, 2) == 1 ? materialWindowLight : materialWindow;
                    }
                }
                break;
            default:
                throw new System.Exception("Could not set random Background");

        }

        if (type == 0) // random
        {
            currentTrial.gridType = "random";
        }

        if (type == 1) // square
        {
            currentTrial.gridType = "square";

            //outer square
            for (int col = 3; col < 16; col++)
            {
                squareGrid[2, col].GetComponent<MeshRenderer>().material = materialWindow;
                squareGrid[17, col].GetComponent<MeshRenderer>().material = materialWindow;
            }
            for (int row = 2; row < 18; row++)
            {
                squareGrid[row, 3].GetComponent<MeshRenderer>().material = materialWindow;
                squareGrid[row, 15].GetComponent<MeshRenderer>().material = materialWindow;
            }
            //square
            for (int col = 4; col < 15; col++)
            {
                squareGrid[3, col].GetComponent<MeshRenderer>().material = materialWindowLight;
                squareGrid[16, col].GetComponent<MeshRenderer>().material = materialWindowLight;
            }
            for (int row = 3; row < 17; row++)
            {
                squareGrid[row, 4].GetComponent<MeshRenderer>().material = materialWindowLight;
                squareGrid[row, 14].GetComponent<MeshRenderer>().material = materialWindowLight;
            }
            //inner square
            for (int col = 5; col < 14; col++)
            {
                squareGrid[4, col].GetComponent<MeshRenderer>().material = materialWindow;
                squareGrid[15, col].GetComponent<MeshRenderer>().material = materialWindow;
            }
            for (int row = 4; row < 16; row++)
            {
                squareGrid[row, 5].GetComponent<MeshRenderer>().material = materialWindow;
                squareGrid[row, 13].GetComponent<MeshRenderer>().material = materialWindow;
            }
        }

        if (type == 2) // diamond
        {
            currentTrial.gridType = "diamond";

            //outer diamond
            for (int row = 0; row < 17; row++)
            {
                if (row <= 8)
                {
                    squareGrid[10 - row, 1 + row].GetComponent<MeshRenderer>().material = materialWindow;
                    squareGrid[10 + row, 1 + row].GetComponent<MeshRenderer>().material = materialWindow;

                }
                if (row > 8)
                {
                    squareGrid[-6 + row, 1 + row].GetComponent<MeshRenderer>().material = materialWindow;
                    squareGrid[26 - row, 1 + row].GetComponent<MeshRenderer>().material = materialWindow;

                }
            }
            //diamond
            for (int row = 0; row < 15; row++)
            {
                if (row <= 7)
                {
                    squareGrid[10 - row, 2 + row].GetComponent<MeshRenderer>().material = materialWindowLight;
                    squareGrid[10 + row, 2 + row].GetComponent<MeshRenderer>().material = materialWindowLight;

                }
                if (row > 7)
                {
                    squareGrid[-4 + row, 2 + row].GetComponent<MeshRenderer>().material = materialWindowLight;
                    squareGrid[24 - row, 2 + row].GetComponent<MeshRenderer>().material = materialWindowLight;

                }
            }
            //inner diamond
            for (int row = 0; row < 14; row++)
            {
                if (row <= 6)
                {
                    squareGrid[10 - row, 3 + row].GetComponent<MeshRenderer>().material = materialWindow;
                    squareGrid[10 + row, 3 + row].GetComponent<MeshRenderer>().material = materialWindow;

                }
                if (row > 6)
                {
                    squareGrid[-2 + row, 3 + row].GetComponent<MeshRenderer>().material = materialWindow;
                    squareGrid[22 - row, 3 + row].GetComponent<MeshRenderer>().material = materialWindow;

                }
            }
        }

        if (type == 3) // ISI
        {
        }
    }

    public int[] GenerateRandomArray(int count, int[] array)
    {
        System.Random random = new System.Random();
        int range = count;
        int[] index = new int[count];
        for (int i = 0; i < count; i++) index[i] = i;
        for (int i = 0; i < count; i++)
        {
            int id = random.Next(0, range - 1);
            array[i] = index[id];
            index[id] = index[range - 1];
            range--;
        }
        return array;
    }

    private void InitGrid()
    {
        GameObject[] grid = GameObject.FindGameObjectsWithTag("window");
        GameObject[] newGrid = grid.OrderBy(o => o.name).ToArray();

        for (int rowNum = 0, num = 0; rowNum < 20; rowNum++, num += 20)
        {
            for (int col = 0; col < 20; col++)
            {
                squareGrid[rowNum, col] = newGrid[num + col];
                squareGrid[rowNum, col].GetComponent<MeshRenderer>().material = materialWindow;
            }
        }
    }

    void CreateEvent()
    {
        switch (Random.Range(1, 5))
        {
            case 1:
                SetupObject(North);
                break;
            case 2:
                SetupObject(East);
                break;
            case 3:
                SetupObject(South);
                break;
            case 4:
                SetupObject(West);
                break;
            default:
                throw new System.Exception("Could not select Beacon");
        }
    }

    void SetupObject(GameObject obj)
    {
        obj.transform.parent.Rotate(Vector3.up, Random.Range(-1f, 1f) * 20);
        int stimType = trialsArray[trialsDone] / (trialsCount / 3);
        int targetStim = 3; // set targetStim as Human Speech
        int[] nonTargetStim = new int[] { 0, 2, 1, 4 };
        int nonTargetStimRandom = nonTargetStim[Random.Range(0, nonTargetStim.Length)];
        //set either target/non-target icon
        Debug.Log(string.Format("trialsDone {0}, trialsArray[trialsDone] {1}, stimType {2}", trialsDone, trialsArray[trialsDone], stimType));

        if (trialsArray[trialsDone] < (stimType * (trialsCount / 3) + (trialsCount / 30))) // target:non-targe = 1:9
        {
            HUDIcon = UIDesigns.transform.GetChild(targetStim).gameObject;
        }
        else
        {
            HUDIcon = UIDesigns.transform.GetChild(nonTargetStimRandom).gameObject;
        }

        //Set obj HUD to chosen icons from the selected Design
        if (stimType == 0 || stimType == 2)
        {
            EnableNavSystem(true);
            obj.GetComponent<HUDNavigationElement>().Prefabs.IndicatorPrefab = HUDIcon.GetComponentInChildren<HNSIndicatorPrefab>();
            obj.GetComponent<HUDNavigationElement>().Prefabs.RadarPrefab = HUDIcon.GetComponentInChildren<HNSRadarPrefab>();
            obj.GetComponent<HUDNavigationElement>().Prefabs.CompassBarPrefab = HUDIcon.GetComponentInChildren<HNSCompassBarPrefab>();
            obj.GetComponent<Renderer>().material.color = obj.GetComponent<HUDNavigationElement>().Prefabs.RadarPrefab.Icon.color;
            obj.SetActive(true);
            //Activate Audio
            if (stimType == 2) isAudioTrialStart = true;
        }
        if (stimType == 1)
        {
            isAudioTrialStart = true;
            EnableNavSystem(false);
        }
        currentTrial.symbol = HUDIcon.name;
        currentTrial.stimulusType = stimType == 0 ? "Visual" : stimType == 1 ? "Audio" : "Visual + Audio";

    }

    public void ResolveEventAngle(GameObject obj)
    {
        North.SetActive(false);
        South.SetActive(false);
        East.SetActive(false);
        West.SetActive(false);

        if (!menuOpen)
        {
            obj.transform.parent.Rotate(Vector3.up, -obj.transform.parent.eulerAngles.y);
            if (currentTrial.noticeTime == 0)
            {
                NoticeStimulus(Time.time);
            }
        }

        crosshair.GetComponent<Image>().color = Color.green;
    }

    void EnableNavSystem(bool isEnable)
    {
        HUDNavigationSystem _HUDNavigationSystem = HUDNavigationSystem.Instance;
        _HUDNavigationSystem.EnableRadar(isEnable);
        _HUDNavigationSystem.EnableIndicators(isEnable);
    }

    public void NoticeStimulus(float noticetime)
    {
        currentTrial.noticeTime = noticetime;
        crosshair.GetComponent<Image>().color = Color.green;
        //markerStream.Write("noticed");
    }

    #region WebGL Stuff
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void download(string data, string filename, string MimeType);
    [DllImport("__Internal")]
    private static extern void copyToClipboard(string text);
#else
    private static void download(string data, string filename, string MimeType)
    { }

    private static void copyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }
#endif

    public void DownloadLog()
    {
        download(System.IO.File.ReadAllText(LogFile), LogFile, "application/json");
    }
    public void CopyExperimentToClipboard()
    {
        copyToClipboard(System.IO.File.ReadAllText(LogFile));
    }
    #endregion

    #region Experiemnt classes
    [System.Serializable]
    public class ExperimentTrial
    {
        public float startTime;
        public float targetEndTime;
        public float noticeTime;
        public float endTime;
        public string symbol;
        public string stimulusType;
        public int result;
        public string gridType;
        public string tag;
        public string audioName;
        private bool logged = false;

        public ExperimentTrial(float start, float end, string symb, string gType, string aName)
        {
            startTime = start;
            endTime = end;
            symbol = symb;
            gridType = gType;
            audioName = aName;
        }
        public void LogToFile(string filename)
        {
            if (!logged)
            {
                logged = true;
                // string thisJson = JsonUtility.ToJson(this);
                // System.IO.File.AppendAllText(filename, thisJson + "\n");
                logWrapper.AddToLog(this);
            }
        }
    }

    [System.Serializable]
    public class Experiment
    {
        public float starttime;
        public float endtime = 0;
        private bool logged = false;

        public Experiment(float start)
        {
            starttime = start;
        }
        public void LogToFile(string filename)
        {
            //string thisJson = JsonUtility.ToJson(this);
            //System.IO.File.AppendAllText(filename, thisJson + "\n");
            if (!logged) logWrapper.AddToLog(this);
            logged = true;
        }
    }

    [System.Serializable]
    public class LogWrapper
    {
        public List<ExperimentTrial> expTrials;
        public List<PairingTraining.SymbolTrainingTrial> symbTrainingTrials;
        public List<Experiment> expFlags;
        public List<AwarenessQuestionnaire.Questionnaire> expQuestions;
        public List<EyeTrackingLogger.EyeGazeData> eyeGazeData;
        public List<LogOnGaze.ObjectGaze> objectGaze;

        public LogWrapper()
        {
            expTrials = new List<ExperimentTrial>();
            symbTrainingTrials = new List<PairingTraining.SymbolTrainingTrial>();
            expFlags = new List<Experiment>();
            expQuestions = new List<AwarenessQuestionnaire.Questionnaire>();
            eyeGazeData = new List<EyeTrackingLogger.EyeGazeData>();
            objectGaze = new List<LogOnGaze.ObjectGaze>();
        }
        private bool logged = false;
        private bool writeflag = false;
        public void LogToFile(string filename)
        {
            string thisJson = JsonUtility.ToJson(this);
            System.IO.File.WriteAllText(filename, thisJson + "\n");
            writeflag = false;
        }
        public void AddToLog(object trial)
        {
            if (trial is ExperimentTrial) expTrials.Add((ExperimentTrial)trial);
            else if (trial is PairingTraining.SymbolTrainingTrial) symbTrainingTrials.Add((PairingTraining.SymbolTrainingTrial)trial);
            else if (trial is Experiment) expFlags.Add((Experiment)trial);//This is for start/end Flags
            else if (trial is AwarenessQuestionnaire.Questionnaire) expQuestions.Add((AwarenessQuestionnaire.Questionnaire)trial);
            else if (trial is EyeTrackingLogger.EyeGazeData) eyeGazeData.Add((EyeTrackingLogger.EyeGazeData)trial);
            else if (trial is LogOnGaze.ObjectGaze) objectGaze.Add((LogOnGaze.ObjectGaze)trial);
            writeflag = true;
        }

        public void RemoveUselessTrial()
        {
            expTrials.RemoveAll(trial => trial.endTime == 0);
        }
        public bool needToWrite()
        {
            return writeflag;
        }
    }
    #endregion
}


/**
Input.GetKeyDown(KeyCode.S) => Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.APP) || Input.GetKeyDown(KeyCode.S)
Input.GetKeyDown(KeyCode.Tab) => Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.A) || Input.GetKeyDown(KeyCode.Tab)
Input.GetKeyDown(KeyCode.Space) => isAAA  || Input.GetKeyDown(KeyCode.Space)
**/
