using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AwarenessQuestionnaire : MonoBehaviour
{
    int questionNum;

    void Start()
    {
        transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (1/3)";
        transform.Find("Question 1").gameObject.SetActive(true);
        transform.Find("Question 2").gameObject.SetActive(true);
    }

    public void Q2Next()
    {
        transform.Find("Question 1").gameObject.SetActive(false);
        transform.Find("Question 2").gameObject.SetActive(false);
        questionNum = 2;
        transform.Find("Question 3").gameObject.SetActive(true);
        transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (2/3)";
    }

    public void Q3Next()
    {
        questionNum = 3;
        transform.Find("Question 3").gameObject.SetActive(false);
        transform.Find("Question 4").gameObject.SetActive(true);
        transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (3/3)";
    }

    public void Q4Submit()
    {
        questionNum = 0;
        transform.Find("Question 4").gameObject.SetActive(false);
        GameObject.Find("ExperimentController").GetComponent<ExperimentController>().isQtnDone = true;
        transform.Find("Title").GetComponent<Text>().text = "Post-Test Questionnaire (1/3)";
        transform.Find("Question 1").gameObject.SetActive(true);
        transform.Find("Question 2").gameObject.SetActive(true);

    }
}