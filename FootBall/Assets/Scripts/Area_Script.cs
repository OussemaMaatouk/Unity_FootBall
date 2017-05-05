using UnityEngine;
using System.Collections;

public class Area_Script : MonoBehaviour {


	
	void OnTriggerExit( Collider coll) {
	
		
		if ( coll.gameObject.tag == "GoalKeeper" || coll.gameObject.tag == "GoalKeeper_Oponent" ) {
					
			coll.gameObject.GetComponent<GoalKeeper_Script>().state = GoalKeeper_Script.GoalKeeper_State.GO_ORIGIN;
			
		}
	
	
	}
	
}
