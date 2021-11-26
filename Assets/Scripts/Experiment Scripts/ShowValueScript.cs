using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowValueScript : MonoBehaviour
{
    AwarenessQuestionnaire.Patterns q3 = new AwarenessQuestionnaire.Patterns();
    AwarenessQuestionnaire.Patterns q4 = new AwarenessQuestionnaire.Patterns();

    public void textUpdate(float value)
    {
        GetComponent<Text>().text = value + "";
    }

    public void checkQ3Value()
    {
        foreach (Transform child in transform)
        {
            q3.SetPattern(child.name, child.gameObject.GetComponent<Slider>().value);
        }
        if (q3.diamond != 0 && q3.horizontalRect != 0 && q3.xPattern != 0 &&
            q3.bigSquare != 0 && q3.fourSquares != 0 && q3.verticalRect != 0)
        {
            transform.parent.Find("Q3Next").gameObject.SetActive(true);
        }
    }

    public void checkQ4Value()
    {
        foreach (Transform child in transform)
        {
            q4.SetPattern(child.name, child.gameObject.GetComponent<Slider>().value);
        }
        if (q4.diamond != 0 && q4.horizontalRect != 0 && q4.xPattern != 0 &&
            q4.bigSquare != 0 && q4.fourSquares != 0 && q4.verticalRect != 0)
        {
            transform.parent.Find("Q4Submit").gameObject.SetActive(true);
        }
    }
}
