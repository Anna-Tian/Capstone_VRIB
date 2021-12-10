using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class BuildingController : MonoBehaviour
{
    public bool isChangePos;
    public GameObject windows;
    public GameObject half;
    public GameObject singleA;
    public GameObject singleB;
    public bool isTesting;

    private GameObject[,] windowGrid = new GameObject[24, 21];

    // Start is called before the first frame update

    private void Start()
    {
        //GenerateGrid();
        //ChangeTag();
    }

    void Update()
    {
        //ChangePosition();
        // TestCreateParentObject();
        // TestDestroyObject();
    }

    private void TestDestroyObject()
    {
        if (isTesting)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject obj = GameObject.Find(string.Format("windowHalf_{0}", i.ToString("00")));
                obj.transform.parent = windows.transform;
            }
            GameObject par = GameObject.Find("Half_Window");
            DestroyImmediate(par);
        }
    }

    private void TestCreateParentObject()
    {
        if (isTesting)
        {
            GameObject halfParent = new GameObject("Half_Window");
            halfParent.transform.parent = windows.transform;
            for (int i = 0; i < 6; i++)
            {
                GameObject obj = GameObject.Find(string.Format("windowHalf_{0}", i.ToString("00")));
                obj.transform.parent = halfParent.transform;
            }
        }
    }

    private void ChangePosition()
    {
        if (isChangePos)
        {
            for (int i = 0; i < 24; i++)
            {
                Vector3 oldPos = GameObject.Find(string.Format("BuildEX7_2x2LOD_Target_{0}", i)).transform.position;
                GameObject.Find(string.Format("BuildEX7_2x2_Target_{0}", i)).transform.position = oldPos;
            }
            isChangePos = false;

        }
    }

    void GenerateGrid()
    {
        Vector3 windowSingle = new Vector3((float)60.5589981, (float)67.4349976, (float)16.9939995);
        Vector3 windowHalf = new Vector3((float)47.6899986, (float)64.6589966, (float)16.9939995);
        for (int rowGroup = 0; rowGroup < 6; rowGroup++)
        {
            GameObject windowClone = Instantiate(half, new Vector3(windowHalf.x, (float)(windowHalf.y - 10.602 * rowGroup), windowHalf.z), half.transform.rotation);
            windowClone.transform.parent = windows.transform;
            windowClone.name = string.Format("windowHalf_{0}", rowGroup.ToString("00"));
        }

        for (int colGroup = 0; colGroup < 7; colGroup++)
        {
            for (int col = 0; col < 3; col++)
            {
                for (int rowGroup = 0; rowGroup < 6; rowGroup++)
                {
                    for (int row = 0, rowNum = 0; row < 4; row = row + 3, rowNum++)
                    {
                        GameObject windowClone = Instantiate(singleA, new Vector3((float)(windowSingle.x + 3.029 * col + 10.601 * colGroup), (float)(windowSingle.y - 7.751 * rowNum - 10.599 * rowGroup), windowSingle.z), singleA.transform.rotation);
                        windowClone.transform.parent = windows.transform;
                        windowClone.name = string.Format("window_{0}_{1}", (row + 4 * rowGroup).ToString("00"), (col + 3 * colGroup).ToString("00"));
                        windowGrid[row + 4 * rowGroup, col + 3 * colGroup] = windowClone;
                    }
                    for (int row = 1; row < 3; row++)
                    {
                        GameObject windowClone = Instantiate(singleB, new Vector3((float)(windowSingle.x + 3.029 * col + 10.601 * colGroup), (float)(windowSingle.y - 2.78 * row - 10.599 * rowGroup), windowSingle.z), singleB.transform.rotation);
                        windowClone.transform.parent = windows.transform;
                        windowClone.name = string.Format("window_{0}_{1}", (row + 4 * rowGroup).ToString("00"), (col + 3 * colGroup).ToString("00"));
                        windowGrid[row + 4 * rowGroup, col + 3 * colGroup] = windowClone;

                    }
                }
            }
        }
        //if(listWindows.Count > 0)
        //{
        //    listWindows.Sort(delegate (GameObject a, GameObject b) {
        //        return a.name.CompareTo(b.name);
        //    });

        //}


        //    for (int row = 2; row < 22; row++)
        //{
        //    for (int col = 0; col < 20; col++)
        //    {
        //        windowGrid[row, col].GetComponent<MeshRenderer>().material = windowLightMaterial;
        //    }
        //}

    }

    void ChangeTag()
    {
        for (int row = 2; row < 22; row++)
        {
            for (int col = 0; col < 20; col++)
            {
                windowGrid[row, col].tag = "window";
            }
        }
    }
}
