using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockUIToCam : MonoBehaviour
{
    public Camera cam;
    public float uiDistance = 2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = cam.transform.rotation;
        this.transform.position = cam.transform.position + cam.transform.forward * uiDistance;
    }
}
