using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pvr_UnitySDKAPI;

public class PairingTraining : MonoBehaviour
{
    public ExperimentController expctrl;
    public GameObject qSymbol;
    public bool isTrialStart;
    int trialNum = 0;
    int trialCount = 15; // symbol * audio = 5 * 3
    int[] trialsArray;
    SymbolTrainingTrial currentTrial = new SymbolTrainingTrial(0, "null");
    float qStartTime = 0;
    string[] btnName = new string[5] { "Animal", "Explosion", "Ground Vehicle", "Human Speech", "Unidentified" };

    // Start is called before the first frame update
    void Start()
    {
        trialsArray = new int[trialCount];
        trialsArray = expctrl.GenerateRandomArray(trialCount, trialsArray);
        Invoke("NewQuestion", 0.01f);
    }

    void Update()
    {
        if (Pvr_UnitySDKAPI.Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.APP) || Input.GetKeyDown(KeyCode.S)) expctrl.trialsDone = 15;
    }

    public void InputAnswer(string answer)
    {
        currentTrial.enteredAnswer = answer;
        currentTrial.endTime = Time.time;

        if (answer == qSymbol.name)
        {
            transform.Find(answer).GetComponent<Image>().color = Color.green;
            currentTrial.result = true;
            currentTrial.LogToFile(expctrl.LogFile);

            expctrl.trialsDone++;
            trialNum++;
            if (expctrl.trialsDone == trialCount)
            {
                trialsArray = expctrl.GenerateRandomArray(trialCount, trialsArray);
                trialNum = 0;
            }
            Invoke("NewQuestion", 0.7f);
        }
        else
        {
            transform.Find(answer).GetComponent<Image>().color = Color.red;
            currentTrial.result = false;
            currentTrial.LogToFile(expctrl.LogFile);
            currentTrial = new SymbolTrainingTrial(qStartTime, qSymbol.name);
        }
    }
    void NewQuestion()
    {
        isTrialStart = true;
        Transform iconHolder = transform.Find("Symbol Icons");
        foreach (Transform child in iconHolder.transform)
        {
            child.gameObject.SetActive(false);
        }
        foreach (string name in btnName)
        {
            transform.Find(name).GetComponent<Image>().color = Color.white;
        }

        SetQSymbol(trialsArray[trialNum] / 3);
        qStartTime = Time.time;
        currentTrial = new SymbolTrainingTrial(qStartTime, qSymbol.name);
    }

    void SetQSymbol(int symbolNum)
    {
        qSymbol = transform.Find("Symbol Icons").GetChild(symbolNum).gameObject;
        qSymbol.SetActive(true);
    }

    [System.Serializable]
    public class SymbolTrainingTrial
    {
        public float startTime;
        public float endTime;
        public string question;
        public string enteredAnswer;
        public bool result;
        private bool logged = false;
        public void LogToFile(string fileName)
        {
            //  string thisJson = JsonUtility.ToJson(this);
            //  System.IO.File.AppendAllText(fileName, thisJson + "\n");
            if (!logged) ExperimentController.logWrapper.AddToLog(this);
            logged = true;
        }
        public SymbolTrainingTrial(float inStart, string inQuestion)
        {
            startTime = inStart;
            question = inQuestion;
        }
    }
}
