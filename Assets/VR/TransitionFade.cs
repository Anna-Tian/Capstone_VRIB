using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionFade : MonoBehaviour
{
    Image fade;
    Color fadecol;
   public bool fadein = false;
   public bool initialFadeIn = false;
    public Canvas canvas;
    float faderate = 1f/5f;
    // Start is called before the first frame update
    void Start()
    {
        
        fade = this.GetComponent<Image>();
        fadecol = fade.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (canvas.worldCamera == null) canvas.worldCamera = Camera.main.gameObject.transform.Find("UICamera").GetComponent<Camera>();
       if (fadein && fade.color.a > 0) //black to clear
        {
            fadecol.a -= Time.deltaTime * faderate;
            fade.color = fadecol;
        }
        else if (!fadein && fade.color.a < 1) // clear to black
        {
            fadecol.a += Time.deltaTime * faderate;
            fade.color = fadecol;
        }
    }

   public void ToggleFade()
    {
        fadein = !fadein;
        //Logger.markerStream.Write("fade " + fadein);
    }
}
