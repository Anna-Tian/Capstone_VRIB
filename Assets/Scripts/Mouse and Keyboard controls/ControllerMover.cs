using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerMover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    Vector2 rotation = new Vector2(0, 0);
    public float speed = 3;
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            rotation.y -= 0.1f;
        if (Input.GetKey(KeyCode.D))
            rotation.y += 0.1f;
        transform.Rotate(rotation * speed);
        rotation = Vector2.zero;
    }
}
