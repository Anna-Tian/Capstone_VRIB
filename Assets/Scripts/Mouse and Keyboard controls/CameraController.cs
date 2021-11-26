using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public ExperimentController expCtrl;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    Vector2 rotation = new Vector2(0, 0);
    public float speed = 3;
    void Update()
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        rotation.x = Mathf.Clamp(rotation.x, -89 / speed, 89 / speed);
        //if (Input.GetKey(KeyCode.A))
        //    rotation.y -= 0.1f;
        //if (Input.GetKey(KeyCode.D))
        //    rotation.y += 0.1f;

        RaycastHit buttonhit;
        Physics.Raycast(this.transform.position, this.transform.forward, out buttonhit);
        this.transform.Find("Pointer").position = buttonhit.point;

        if (Input.GetMouseButtonDown(0) && !expCtrl.menuOpen)
        {
            if (buttonhit.transform != null)
            {
                ExecuteEvents.Execute(buttonhit.transform.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
            }

            if (expCtrl.EventRunning && (Time.time > expCtrl.currentTrial.startTime + 0.2) //...
                && (expCtrl.currentTrial.startTime > expCtrl.currentTrial.endTime) //...
                && !expCtrl.menuOpen && //...
                (Time.time > expCtrl.currentTrial.noticeTime + 0.2)) //endIf
            {
                if (expCtrl.inTraining && expCtrl.trialsDone == 0 && (expCtrl.North.active || expCtrl.East.active || expCtrl.South.active || expCtrl.West.active))
                {
                    expCtrl.ResolveEventAngle(this.gameObject);
                    //expCtrl.SetTutText("Return to the starting position and identify the symbol");
                }
                else expCtrl.ResolveEventAngle(this.gameObject);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (expCtrl.currentTrial.noticeTime == 0 && expCtrl.EventRunning)
            {
                // expCtrl.userNoticedStim = true;
            }

        }

        if (expCtrl.menuOpen)
        {
            rotation.x = 0;
            rotation.y = 0;
        }
    }
}