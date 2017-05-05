using UnityEngine;
using System.Collections;

public class Camera_Script : MonoBehaviour {
	
	private bool isTriggering = false;
	private float distanceAdder = 0.0f;
	private float distanceAdder2 = 0.0f;
	private bool isExit = false;
	public Transform target;
	 

	public Vector3 targetOffsetPos;
	private Vector3 oldPos;	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
	
		
		if ( GetComponent<InGameState_Script>().state == InGameState_Script.InGameState.PLAYING 
			|| GetComponent<InGameState_Script>().state == InGameState_Script.InGameState.THROW_GATE_RUN
			|| GetComponent<InGameState_Script>().state == InGameState_Script.InGameState.THROW_GATE_REMOVING) {
			oldPos = transform.position;
			Vector3 newPos = new Vector3( target.position.x+targetOffsetPos.x, target.position.y+targetOffsetPos.y, target.position.z+targetOffsetPos.z );
			
			float lerpX =  Mathf.Lerp( oldPos.x, newPos.x,  0.05f );
			float lerpY =  Mathf.Lerp( oldPos.y, newPos.y,  0.05f );
			float lerpZ =  Mathf.Lerp( oldPos.z, newPos.z,  0.05f );
			
			transform.position = new Vector3( lerpX, lerpY, lerpZ );			
			transform.LookAt( target );
		}
		
		if ( GetComponent<InGameState_Script>().state == InGameState_Script.InGameState.CORNER_AIMING
			|| GetComponent<InGameState_Script>().state == InGameState_Script.InGameState.CORNER_REMOVING) {
		
		
			GameObject thrower = GetComponent<InGameState_Script>().candidateTothrowBand;
		
			if ( thrower ) { 
			
				transform.position = thrower.transform.position - thrower.transform.forward*15.0f + thrower.transform.up*3.0f ;
				transform.LookAt( target );
			
			}
			
		
		}		
		
	}
}
