using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    void Update()
    {
        transform.Rotate(Time.deltaTime * 30, 0, 0);
        transform.Rotate(0, Time.deltaTime * 30, 0, Space.World);
    }
}
