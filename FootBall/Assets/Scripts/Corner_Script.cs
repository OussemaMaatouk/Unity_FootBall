using UnityEngine;
using System.Collections;

public class Corner_Script : MonoBehaviour {
	
	
	public Transform downPosition;
	public Transform upPosition;
	
	public GameObject area;
	public Transform point_throw_gate;
	public GameObject goalKeeper;
	
	public Sphere sphere;
	
	// Use this for initialization
	void Start () {
	
		sphere = (Sphere)GameObject.FindObjectOfType( typeof(Sphere) );
	}

	void OnTriggerEnter( Collider other) {
	
		
		if ( Camera.main.GetComponent<InGameState_Script>().state != InGameState_Script.InGameState.GOAL ) {
		
			if ( (other.gameObject.tag == "PlayerTeam1" || other.gameObject.tag == "OponentTeam") && Camera.main.GetComponent<InGameState_Script>().state == InGameState_Script.InGameState.PLAYING ) {
			
				if ( other.gameObject != sphere.owner ) {
				
					other.gameObject.GetComponent<Player_Script>().temporallyUnselectable = true;
					other.gameObject.GetComponent<Player_Script>().timeToBeSelectable = 0.5f;
					other.gameObject.GetComponent<Player_Script>().state = Player_Script.Player_State.GO_ORIGIN;
				}
				
			}
	
			
			if ( other.gameObject.tag == "Ball" && Camera.main.GetComponent<InGameState_Script>().state == InGameState_Script.InGameState.PLAYING ) {
				
				sphere.owner = null;
				Camera.main.GetComponent<InGameState_Script>().timeToChangeState = 2.0f;
				Camera.main.GetComponent<InGameState_Script>().areaCorner = area;
				Camera.main.GetComponent<InGameState_Script>().throw_goal = point_throw_gate;
				Camera.main.GetComponent<InGameState_Script>().goalKeeper = goalKeeper;
				Camera.main.GetComponent<InGameState_Script>().cornerTrigger = this.gameObject;
				
				
				// chercher le corner le plus proche du ballon
				Vector3 positionBall = sphere.gameObject.transform.position;			
				if ( (positionBall-downPosition.position).magnitude > (positionBall-upPosition.position).magnitude ) {
					Camera.main.GetComponent<InGameState_Script>().cornerSource = upPosition;
				} else {
					Camera.main.GetComponent<InGameState_Script>().cornerSource = downPosition;		
				}
				
				Camera.main.GetComponent<InGameState_Script>().state = InGameState_Script.InGameState.CORNER;
				
			}
		
		}	
		
	}
	
	
		
	
}
