using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateUIElement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    Vector3 rotation = new Vector3(0, 0, 0);
    public float speed = 3;
    // Update is called once per frame
    void Update()
    {
       // if (Input.GetKey(KeyCode.A))
       //     rotation.z = 0.1f;
       // if (Input.GetKey(KeyCode.D))
       //    rotation.z = -0.1f;
        rotation.z -= Input.GetAxis("Mouse X");
        transform.Rotate(rotation * speed);
        rotation = Vector3.zero;
    }
}
