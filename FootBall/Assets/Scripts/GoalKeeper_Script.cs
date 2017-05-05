using UnityEngine;
using System.Collections;

public class GoalKeeper_Script : MonoBehaviour {

	public string Name;	
	
	public enum GoalKeeper_State { 
	   RESTING,
	   GO_ORIGIN,
	   STOLE_BALL,
	   GET_BALL_DOWN,
	   UP_WITH_BALL,
	   PASS_HAND,
	   THROW_GATE,
	   JUMP_LEFT,
	   JUMP_RIGHT,
	   JUMP_LEFT_FLAT,
	   JUMP_RIGHT_FLAT	  
	};
	
	public GoalKeeper_State state;
	public Transform center_field;
	public Sphere sphere;
	public Vector3 initial_Position;
	public Transform hand_bone;
	private float timeToSacar = 1.0f;
	public CapsuleCollider capsuleCollider;	
	// Use this for initialization
	void Start () {
	
		initial_Position = transform.position;
		state = GoalKeeper_State.RESTING;
		GetComponent<Animation>()["run"].speed = 1.0f;
		
		GetComponent<Animation>()["goalkeeper_high_right"].speed = 1.0f; //gardien dégagement latérale droit élevé
		GetComponent<Animation>()["goalkeeper_high_left"].speed = 1.0f; //gardien dégagement latérale gauche élevé
		GetComponent<Animation>()["goalkeeper_low_right"].speed = 1.0f; //gardien dégagement latérale droit plat
		GetComponent<Animation>()["goalkeeper_low_left"].speed = 1.0f; //gardien dégagement latérale gauche plat
		
	}
	
	// Update is called once per frame
	void Update () {
		
		
		switch (state) {
	
			case GoalKeeper_State.JUMP_LEFT:
				
				
				capsuleCollider.direction = 0;
			
				if ( GetComponent<Animation>()["goalkeeper_high_left"].normalizedTime < 0.45f  ) {
					transform.position -= transform.right * Time.deltaTime * 7.0f;
				}
			
			
				if ( !GetComponent<Animation>().IsPlaying("goalkeeper_high_left") ) {
					state = GoalKeeper_State.STOLE_BALL;		
					capsuleCollider.direction = 1;

				}
			
			break;
	
			case GoalKeeper_State.JUMP_RIGHT:

				capsuleCollider.direction = 0;

				if ( GetComponent<Animation>()["goalkeeper_high_right"].normalizedTime < 0.45f  ) {
					transform.position += transform.right * Time.deltaTime * 7.0f;
				}		
				if ( !GetComponent<Animation>().IsPlaying("goalkeeper_high_right") ) {
					state = GoalKeeper_State.STOLE_BALL;		
					capsuleCollider.direction = 1;

				}
				
				
			break;
			
			case GoalKeeper_State.JUMP_LEFT_FLAT:
				
				
				capsuleCollider.direction = 0;
			
				if ( GetComponent<Animation>()["goalkeeper_low_left"].normalizedTime < 0.45f  ) {
					transform.position -= transform.right * Time.deltaTime * 4.0f;
				}
			
			
				if ( !GetComponent<Animation>().IsPlaying("goalkeeper_low_left") ) {
					state = GoalKeeper_State.STOLE_BALL;		
					capsuleCollider.direction = 1;

				}
			
			break;
	
			case GoalKeeper_State.JUMP_RIGHT_FLAT:

				capsuleCollider.direction = 0;

				if ( GetComponent<Animation>()["goalkeeper_low_right"].normalizedTime < 0.45f  ) {
					transform.position += transform.right * Time.deltaTime * 4.0f;
				}		
				if ( !GetComponent<Animation>().IsPlaying("goalkeeper_low_right") ) {
					state = GoalKeeper_State.STOLE_BALL;		
					capsuleCollider.direction = 1;

				}
				
				
			break;
						
				case GoalKeeper_State.THROW_GATE:
		
			break;			
			
			case GoalKeeper_State.PASS_HAND:
								//gardien lance à main levé
				if ( GetComponent<Animation>()["goalkeeper_serve_high_hand"].normalizedTime < 0.65f && sphere.gameObject.GetComponent<Rigidbody>().isKinematic == true ) {
					sphere.gameObject.transform.position = hand_bone.position;
					sphere.gameObject.transform.rotation = hand_bone.rotation;
				}
		
				if ( GetComponent<Animation>()["goalkeeper_serve_high_hand"].normalizedTime > 0.65f && sphere.gameObject.GetComponent<Rigidbody>().isKinematic == true ) { 
					sphere.gameObject.GetComponent<Rigidbody>().isKinematic = false;
					sphere.gameObject.GetComponent<Rigidbody>().AddForce( transform.forward*5000.0f + new Vector3(0.0f, 1300.0f, 0.0f) );
				}
		
				if ( !GetComponent<Animation>().IsPlaying("goalkeeper_serve_high_hand") || !GetComponent<Animation>().IsPlaying("goalkeeper_serve_high_hand") ) {
					state  = GoalKeeper_State.GO_ORIGIN;			
				}
			
			break;
			

			case GoalKeeper_State.UP_WITH_BALL:
			
			
				if ( !GetComponent<Animation>().IsPlaying("goalkeeper_lifts_ball") ) {
				
					sphere.gameObject.GetComponent<Rigidbody>().isKinematic = true;
					sphere.gameObject.transform.position = hand_bone.position;
					sphere.gameObject.transform.rotation = hand_bone.rotation;
	
					timeToSacar -= Time.deltaTime;
					
					if ( timeToSacar < 0.0f ) {
						timeToSacar = UnityEngine.Random.Range( 2.0f, 5.0f );
						GetComponent<Animation>().Play("goalkeeper_serve_high_hand");
						state = GoalKeeper_State.PASS_HAND;
					}
				
				
				} else {
				
					sphere.gameObject.transform.position = hand_bone.position;
					sphere.gameObject.transform.rotation = hand_bone.rotation;
				
					transform.LookAt( center_field.position );
				
				
				}

			break;			
			
			case GoalKeeper_State.GET_BALL_DOWN:
			
				sphere.gameObject.transform.position = hand_bone.position;
				sphere.gameObject.transform.rotation = hand_bone.rotation;
											//gardien arrete le ballon
				if ( !GetComponent<Animation>().IsPlaying("goalkeeper_front_guard") ) {
					GetComponent<Animation>().Play("goalkeeper_lifts_ball");
					state = GoalKeeper_State.UP_WITH_BALL;
				}
			
			break;
			
			
			case GoalKeeper_State.RESTING:
			
				capsuleCollider.direction = 1;
				if ( !GetComponent<Animation>().IsPlaying("guard") )
					GetComponent<Animation>().Play("guard"); //gardien en repos
				
				transform.LookAt( new Vector3( sphere.gameObject.transform.position.x, transform.position.y , sphere.gameObject.transform.position.z)  );
			
				float distanceBall = (transform.position - sphere.gameObject.transform.position).magnitude;
		
				if ( distanceBall < 10.0f ) {
					state = GoalKeeper_Script.GoalKeeper_State.STOLE_BALL;
				} 
			
			
			break;

			case GoalKeeper_State.STOLE_BALL:
				GetComponent<Animation>().Play("run"); //running

				Vector3 RelativeWaypointPosition = transform.InverseTransformPoint( sphere.gameObject.transform.position );
	
				float inputSteer = RelativeWaypointPosition.x / RelativeWaypointPosition.magnitude;
			
				transform.Rotate(0, inputSteer*10.0f , 0);
				transform.position += transform.forward*6.0f*Time.deltaTime;

		
				if ( RelativeWaypointPosition.magnitude < 1.0f ) {
			
//					state = GoalKeeper_State.RESTING;					
				}

			break;
	
	
			case GoalKeeper_State.GO_ORIGIN:

				GetComponent<Animation>().Play("run");
				RelativeWaypointPosition = transform.InverseTransformPoint( initial_Position );
	
				inputSteer = RelativeWaypointPosition.x / RelativeWaypointPosition.magnitude;
			
				transform.Rotate(0, inputSteer*10.0f, 0);
				transform.position += transform.forward*6.0f*Time.deltaTime;

		
				if ( RelativeWaypointPosition.magnitude < 1.0f ) {
			
					state = GoalKeeper_State.RESTING;					
				}
		
			
			
			break;
			
			
			
		}
		
	}
	
	
	
	// to know if GoalKeeper is touching Ball
	void OnCollisionStay( Collision coll ) {
		
		if ( Camera.main.GetComponent<InGameState_Script>().state == InGameState_Script.InGameState.PLAYING ) {
		
			if ( coll.collider.transform.gameObject.tag == "Ball" && state != GoalKeeper_State.UP_WITH_BALL && state != GoalKeeper_State.PASS_HAND && state != GoalKeeper_State.THROW_GATE &&
				 state != GoalKeeper_State.JUMP_LEFT && state != GoalKeeper_State.JUMP_RIGHT &&
				 state != GoalKeeper_State.JUMP_LEFT_FLAT && state != GoalKeeper_State.JUMP_RIGHT_FLAT) {
							
				Camera.main.GetComponent<InGameState_Script>().lastTouched = gameObject;
			
	
				Vector3 relativePos = transform.InverseTransformPoint( sphere.gameObject.transform.position );
				
				// attraper le ballon sauf si l'altitude est inf a 0.35f (relative)
				if ( relativePos.y < 0.35f ) { 
				
					sphere.owner = null;
		
					GetComponent<Animation>().Play("goalkeeper_front_guard");
					state = GoalKeeper_State.GET_BALL_DOWN;
					
				}
				
				
			}
		
		}
		
	}
	
}
