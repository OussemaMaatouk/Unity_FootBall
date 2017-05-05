using UnityEngine;
using System.Collections;

public class Shadow_Script : MonoBehaviour {
	
	
	public Vector3 test;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {

        //fixer l'ombre
        transform.rotation = Quaternion.Euler( -90, 0, 0);
		
	}
}
