using UnityEngine;
using System.Collections;

public class GoalKeeperJump : MonoBehaviour {
	
	
	
	public GoalKeeper_Script goalKeeper;
	// Use this for initialization
	void Start () {
		

		
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	void OnTriggerEnter( Collider other ) {
	
		if ( other.tag == "Ball" ) {
            if (tag == "GoalKeeper_Jump_Left")
            {

                goalKeeper.state = GoalKeeper_Script.GoalKeeper_State.JUMP_LEFT;
                goalKeeper.gameObject.GetComponent<Animation>().Play("goalkeeper_high_left");


                Debug.Log("Left");
            }

            if (tag == "GoalKeeper_Jump_Right")
            {

                goalKeeper.state = GoalKeeper_Script.GoalKeeper_State.JUMP_RIGHT;
                goalKeeper.gameObject.GetComponent<Animation>().Play("goalkeeper_high_right");

                Debug.Log("Right");

            }

          
		
		}
		
		
	}
	
}
