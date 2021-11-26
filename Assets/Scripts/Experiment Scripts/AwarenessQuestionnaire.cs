using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AwarenessQuestionnaire : MonoBehaviour
{
    public ExperimentController expctrl;
    int questionNum;
    Questionnaire question = new Questionnaire("", "");

    void Start()
    {
        transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (1/4)";
        transform.Find("Question 1").gameObject.SetActive(true);
    }

    public void InputAnswerQ1(string answer)
    {
        if (answer == "Yes")
        {
            // expctrl.isQtnDone = true;
            transform.Find("Question 2").gameObject.SetActive(true);
            transform.Find("Question 1").Find("Q1Next").gameObject.SetActive(false);
        }
        if (answer == "No")
        {
            transform.Find("Question 1").Find("Q1Next").gameObject.SetActive(true);
        }
        question.q1 = answer;
    }

    public void Q1Next()
    {
        transform.Find("Question 1").gameObject.SetActive(false);
        transform.Find("Question 1").Find("Q1Next").gameObject.SetActive(false);
        questionNum = 1;
        ViewPatterns();
    }

    public void Q2Next()
    {
        question.q2 = transform.Find("Question 2").Find("InputField").gameObject.GetComponent<InputField>().text;
        transform.Find("Question 1").gameObject.SetActive(false);
        transform.Find("Question 1").Find("Q1Next").gameObject.SetActive(false);
        transform.Find("Question 2").gameObject.SetActive(false);
        questionNum = 2;
        ViewPatterns();
    }

    public void ViewPatterns()
    {
        transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (2/4)";
        transform.Find("Patterns").gameObject.SetActive(true);
        transform.Find("Question 3").gameObject.SetActive(false);
        transform.Find("Question 4").gameObject.SetActive(false);
        transform.Find("ViewPatternsBtn").gameObject.SetActive(false);
    }

    public void PatternsNext()
    {
        transform.Find("Patterns").gameObject.SetActive(false);
        if (questionNum == 1 || questionNum == 2)
        {
            transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (3/4)";
            transform.Find("Question 3").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (4/4)";
            transform.Find("Question 4").gameObject.SetActive(true);
        }
        transform.Find("ViewPatternsBtn").gameObject.SetActive(true);
    }

    public void Q3Next()
    {
        foreach (Transform child in transform.Find("Question 3").Find("PatternSlider").transform)
        {
            question.q3.SetPattern(child.name, child.gameObject.GetComponent<Slider>().value);
        }
        questionNum = 3;
        transform.Find("Question 3").gameObject.SetActive(false);
        transform.Find("Question 3").Find("Q3Next").gameObject.SetActive(false);
        transform.Find("Question 4").gameObject.SetActive(true);
        transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (4/4)";
    }

    public void Q4Submit()
    {
        foreach (Transform child in transform.Find("Question 4").Find("PatternSlider").transform)
        {
            question.q4.SetPattern(child.name, child.gameObject.GetComponent<Slider>().value);
        }

        StoreAndClearAnswer();
    }

    void StoreAndClearAnswer()
    {
        question.phase = expctrl.currentTrial.tag;
        question.date = System.DateTime.Now.ToString("s");
        question.LogToFile(expctrl.LogFile);

        questionNum = 0;
        question = new Questionnaire("", "");
        transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (1/4)";
        transform.Find("Question 1").gameObject.SetActive(true);
        transform.Find("Question 2").Find("InputField").gameObject.GetComponent<InputField>().text = "";
        foreach (Transform child in transform.Find("Question 3").Find("PatternSlider").transform)
        {
            child.gameObject.GetComponent<Slider>().value = 0;
        }
        foreach (Transform child in transform.Find("Question 4").Find("PatternSlider").transform)
        {
            child.gameObject.GetComponent<Slider>().value = 0;
        }
        transform.Find("Question 4").gameObject.SetActive(false);
        transform.Find("Question 4").Find("Q4Submit").gameObject.SetActive(false);
        transform.Find("ViewPatternsBtn").gameObject.SetActive(false);

        expctrl.isQtnDone = true;

    }

    [System.Serializable]
    public class Patterns
    {
        public float diamond;
        public float horizontalRect;
        public float xPattern;
        public float bigSquare;
        public float fourSquares;
        public float verticalRect;
        private bool logged = false;
        public void LogToFile(string fileName)
        {
            //  string thisJson = JsonUtility.ToJson(this);
            //  System.IO.File.AppendAllText(fileName, thisJson + "\n");
            if (!logged) ExperimentController.logWrapper.AddToLog(this);
            logged = true;
        }
        public Patterns()
        {

        }

        public void SetPattern(string name, float value)
        {
            if (name == "diamond") diamond = value;
            else if (name == "horizontalRect") horizontalRect = value;
            else if (name == "xPattern") xPattern = value;
            else if (name == "bigSquare") bigSquare = value;
            else if (name == "fourSquares") fourSquares = value;
            else if (name == "verticalRect") verticalRect = value;
        }
    }

    [System.Serializable]
    public class Questionnaire
    {
        public string phase;
        public string date;
        public string q1;
        public string q2;
        public Patterns q3 = new Patterns();
        public Patterns q4 = new Patterns();

        private bool logged = false;
        public void LogToFile(string fileName)
        {
            //  string thisJson = JsonUtility.ToJson(this);
            //  System.IO.File.AppendAllText(fileName, thisJson + "\n");
            if (!logged) ExperimentController.logWrapper.AddToLog(this);
            logged = true;
        }
        public Questionnaire(string inPhase, string inDate)
        {
            phase = inPhase;
            date = inDate;
        }
    }
}