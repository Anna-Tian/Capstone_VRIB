using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreathingTask : MonoBehaviour
{
    float ratePerSecond = 1;
    float minscale = 1;
    float maxscale = 4;
    bool isExpanding = true;
    // Start is called before the first frame update
    void Start()
    {
        transform.parent.Find("MinCircle").localScale.Set(minscale, minscale, 1);
        transform.parent.Find("MaxCircle").localScale.Set(maxscale, maxscale, 1);
    }

    // Update is called once per frame
    void Update()
    {
         float deltaRate = ratePerSecond * Time.deltaTime;
        if (isExpanding)
        {
            transform.localScale += new Vector3(deltaRate, deltaRate, 0);
        }
        else
        {
            transform.localScale -= new Vector3(deltaRate, deltaRate, 0);
        }
        if (transform.localScale.x >= maxscale || transform.localScale.y >= maxscale)
        {
            isExpanding = false;
            transform.parent.Find("BreathingInstruction").GetComponent<Text>().text = "Exhale";
            transform.localScale.Set(maxscale, maxscale, 1);
        }
        else if (transform.localScale.x <= minscale || transform.localScale.y <= minscale)
        {
            isExpanding = true;
            transform.parent.Find("BreathingInstruction").GetComponent<Text>().text = "Inhale";
            transform.localScale.Set(minscale, minscale, 1);
        }
    }
}
