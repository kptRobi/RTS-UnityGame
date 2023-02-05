using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LookAtCamera : MonoBehaviour {


    Transform cameraTransform;

    // Use this for initialization
    void Awake () {

        cameraTransform = Camera.main.transform;
    }
	
	// Update is called once per frame
	void Update () {
        //pozycja nad jednostką
        transform.LookAt(cameraTransform);
        var rotation = transform.localEulerAngles;
        rotation.y = 180;
        transform.localEulerAngles = rotation;
    }
}
