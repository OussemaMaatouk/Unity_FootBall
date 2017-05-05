using UnityEngine;
using System.Collections;

public class Band_Script : MonoBehaviour {
	
	public Sphere sphere;
	public Vector3 direction_throw;
	
	// Use this for initialization
	void Start () {
		
		sphere = (Sphere)GameObject.FindObjectOfType( typeof(Sphere) );		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	void OnTriggerEnter( Collider other) {
	
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
			Camera.main.GetComponent<InGameState_Script>().state = InGameState_Script.InGameState.THROW_BAND;
			Camera.main.GetComponent<InGameState_Script>().positionBand = sphere.gameObject.transform.position;		
			
		}
		
		
		
	}
	
	
}
