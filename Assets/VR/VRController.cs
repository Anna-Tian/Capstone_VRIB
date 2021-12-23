using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.Extras;

public class VRController : MonoBehaviour
{
   static public GameObject rightController;
   static public GameObject leftController;
   public GameObject inRight;
   public GameObject inLeft;
   static public SteamVR_LaserPointer laserPointer;
   public SteamVR_LaserPointer inLaser;
    // Start is called before the first frame update
    void Start()
    {
        rightController = inRight;
        leftController = inLeft;
        laserPointer = inLaser;

        if (laserPointer != null)
        {
            laserPointer.PointerClick -= HandleTriggerClicked;
            laserPointer.PointerClick += HandleTriggerClicked;
            laserPointer.PointerIn -= HandlePointerIn;
            laserPointer.PointerIn += HandlePointerIn;
            laserPointer.PointerOut -= HandlePointerOut;
            laserPointer.PointerOut += HandlePointerOut;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rightController == null) rightController = GameObject.Find("Controller (right)");
        if (leftController == null) leftController = GameObject.Find("Controller (left)");
        if (laserPointer == null) laserPointer = FindObjectOfType<SteamVR_LaserPointer>();
    }

   static public void ResetReferences()
    {
        if (rightController == null) rightController = GameObject.Find("Controller (right)");
        if (leftController == null) leftController = GameObject.Find("Controller (left)");
        if (laserPointer == null) laserPointer = FindObjectOfType<SteamVR_LaserPointer>();
    }
    #region Pointer Methods
    private void HandleTriggerClicked(object sender, PointerEventArgs e)
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }
    private void HandlePointerIn(object sender, PointerEventArgs e)
    {
        var button = e.target.GetComponent<Button>();
        if (button != null)
        {
            button.Select();
            Debug.Log("HandlePointerIn", e.target.gameObject);
        }
    }

    private void HandlePointerOut(object sender, PointerEventArgs e)
    {

        var button = e.target.GetComponent<Button>();
        if (button != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("HandlePointerOut", e.target.gameObject);
        }
    }
    #endregion
}
