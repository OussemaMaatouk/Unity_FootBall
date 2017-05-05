using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml;
using System.IO;

public class InGameState_Script : MonoBehaviour {

	
	public enum InGameState {
		
		PLAYING,
		KICK_OFF,
		GOAL,
		THROW_BAND, //lancer le ballon a partir de la bande
		THROW_BAND_AIMING, 
		THROW_BAND_REMOVING, 
		THROW_BAND_REMOVED,
		CORNER,
		CORNER_AIMING,//fixe le point de tir
		CORNER_REMOVING, 
		CORNER_REMOVING_2,
		CORNER_REMOVED, 
		THROW_GATE, 
		THROW_GATE_RUN,
		THROW_GATE_REMOVING
		
	};
	
	public InGameState state;
	private GameObject[] players;
	private GameObject[] oponents;
	private GameObject keeper;
	private GameObject keeper_oponent;
	public GameObject lastTouched;
	public float timeToChangeState = 0.0f;
	public Vector3 positionBand;
	public Sphere sphere;
	public Transform center;
	public Vector3 target_throw_band;
	
	private GameObject whoLastTouched;
	public GameObject candidateTothrowBand;
	private float timeToThrowOponent = 3.0f;
	
	public Transform cornerSource;
	public GameObject areaCorner;
	public Transform throw_goal;
	public GameObject goalKeeper;
	public GameObject cornerTrigger;
	
	public Mesh[] Meshes;
	public Material[] Mat;
	
	private float timeToKickOff = 4.0f;
	public GameObject lastCandidate = null;
	
	public int score_eq1= 0;
	public int score_eq2 = 0;

	
	public GameObject[] playerPrefab;
	public GameObject goalKeeperPrefab;
	public GameObject ballPrefab;
	
	public Material mat_temp;
	public Transform target_oponent_goal;
    public Text score_eq1_t;
    public Text score_eq2_t;


	void Start () {
	
		players = GameObject.FindGameObjectsWithTag("PlayerTeam1");
		oponents = GameObject.FindGameObjectsWithTag("OponentTeam");
		keeper = GameObject.FindGameObjectWithTag("GoalKeeper");
		keeper_oponent = GameObject.FindGameObjectWithTag("GoalKeeper_Oponent");
		
		state = InGameState.PLAYING;
	
	}
	
	
	
	// Update is called once per frame
	void Update () {
	
		// update scorer
		score_eq1_t.text = "" + score_eq1;
		score_eq2_t.text = "" + score_eq2;
		
		
		timeToChangeState -= Time.deltaTime;
		
		if ( timeToChangeState < 0.0f ) {
		
			switch (state) {
				
				case InGameState.PLAYING:
			
				break;
	
				case InGameState.THROW_BAND:
				
					whoLastTouched = lastTouched;	
				
					foreach ( GameObject go in players ) {
						go.GetComponent<Player_Script>().state = Player_Script.Player_State.RESTING;
					}
					foreach ( GameObject go in oponents ) {
						go.GetComponent<Player_Script>().state = Player_Script.Player_State.RESTING;
					}
				
				
					sphere.owner = null;
				
					if ( whoLastTouched.tag == "PlayerTeam1" )
						candidateTothrowBand = SearchPlayerNearBall( oponents );
					else	
						candidateTothrowBand = SearchPlayerNearBall( players );
				
				
					candidateTothrowBand.transform.position = new Vector3( positionBand.x, candidateTothrowBand.transform.position.y, positionBand.z);
				
					if ( whoLastTouched.tag == "PlayerTeam1" ) {
					
						candidateTothrowBand.GetComponent<Player_Script>().temporallyUnselectable = true;
						candidateTothrowBand.GetComponent<Player_Script>().timeToBeSelectable = 1.0f;
				
						candidateTothrowBand.transform.LookAt( SearchPlayerNearBall( oponents ).transform.position);
					}
					else
						candidateTothrowBand.transform.LookAt( center ); 
	
				
					candidateTothrowBand.transform.Rotate(0, sphere.fHorizontal*10.0f, 0);
					candidateTothrowBand.GetComponent<Player_Script>().state = Player_Script.Player_State.THROW_BAND;
				
					sphere.GetComponent<Rigidbody>().isKinematic = true;
					sphere.gameObject.transform.position = candidateTothrowBand.GetComponent<Player_Script>().hand_bone.position;
				
					target_throw_band = candidateTothrowBand.transform.position + candidateTothrowBand.transform.forward;
				
				
					candidateTothrowBand.GetComponent<Animation>().Play("serve_the_band"); //lancer depuis la bande 
					candidateTothrowBand.GetComponent<Animation>()["serve_the_band"].time = 0.1f;
					candidateTothrowBand.GetComponent<Animation>()["serve_the_band"].speed = 0.0f;
				
					state = InGameState.THROW_BAND_AIMING;
				
				break;
				case InGameState.THROW_BAND_AIMING:
				

					candidateTothrowBand.transform.position = new Vector3( positionBand.x, candidateTothrowBand.transform.position.y, positionBand.z);
					candidateTothrowBand.transform.LookAt( target_throw_band );
					candidateTothrowBand.GetComponent<Player_Script>().state = Player_Script.Player_State.THROW_BAND;
				
					sphere.GetComponent<Rigidbody>().isKinematic = true;
					sphere.gameObject.transform.position = candidateTothrowBand.GetComponent<Player_Script>().hand_bone.position;

					if ( whoLastTouched.tag != "PlayerTeam1" ) {
				
						target_throw_band += new Vector3( 0,0,sphere.fHorizontal/10.0f);
					
						if (sphere.bPassButton) {
							candidateTothrowBand.GetComponent<Animation>().Play("serve_the_band");
							state = InGameState.THROW_BAND_REMOVING;
		
						}
						
					} else {
					
						timeToThrowOponent -= Time.deltaTime;
					
						if ( timeToThrowOponent < 0.0f ) {					
							timeToThrowOponent = 3.0f;
							sphere.gameObject.GetComponent<Rigidbody>().isKinematic = true;
							candidateTothrowBand.GetComponent<Animation>().Play("serve_the_band");
							state = InGameState.THROW_BAND_REMOVING;
						}
					
					}
				
				break;	
				
				case InGameState.THROW_BAND_REMOVING:
					
					candidateTothrowBand.GetComponent<Animation>()["serve_the_band"].speed = 1.0f;

					if ( candidateTothrowBand.GetComponent<Animation>()["serve_the_band"].normalizedTime < 0.5f && sphere.gameObject.GetComponent<Rigidbody>().isKinematic == true ) {
						sphere.gameObject.transform.position = candidateTothrowBand.GetComponent<Player_Script>().hand_bone.position;
					}

					if ( candidateTothrowBand.GetComponent<Animation>()["serve_the_band"].normalizedTime >= 0.5f && sphere.gameObject.GetComponent<Rigidbody>().isKinematic == true ) {
						sphere.gameObject.GetComponent<Rigidbody>().isKinematic = false;
						sphere.gameObject.GetComponent<Rigidbody>().AddForce( candidateTothrowBand.transform.forward*4000.0f + new Vector3(0.0f, 1300.0f, 0.0f) );					
					} 
				
				
				
					if ( candidateTothrowBand.GetComponent<Animation>().IsPlaying("serve_the_band") == false ) {
						state = InGameState.THROW_BAND_REMOVED;
					}
				
				
				break;

				case InGameState.THROW_BAND_REMOVED:
					candidateTothrowBand.GetComponent<Player_Script>().state = Player_Script.Player_State.MOVE_AUTOMATIC;
					state = InGameState.PLAYING;
				
				break;
				
				
				
				
				case InGameState.CORNER:
				
					whoLastTouched = lastTouched;	
				
					if ( whoLastTouched.tag == "GoalKeeper_Oponent" )
						whoLastTouched.tag = "OponentTeam";
					if ( whoLastTouched.tag == "GoalKeeper" )
						whoLastTouched.tag = "PlayerTeam1";
				
				
				
				
					if ( cornerTrigger.tag == "Corner_Oponent" && whoLastTouched.tag == "PlayerTeam1") {
						state = InGameState.THROW_GATE;
						break;
					}
					if ( cornerTrigger.tag != "Corner_Oponent" && whoLastTouched.tag == "OponentTeam" ) {
						state = InGameState.THROW_GATE;
						break;
					}
				
				
				
				
					foreach ( GameObject go in players ) {
						go.GetComponent<Player_Script>().state = Player_Script.Player_State.RESTING;
					}
					foreach ( GameObject go in oponents ) {
						go.GetComponent<Player_Script>().state = Player_Script.Player_State.RESTING;
					}
				
				
					sphere.owner = null;
				
					if ( whoLastTouched.tag == "PlayerTeam1" ) {
						PutPlayersInCornerArea( players, Player_Script.TypePlayer.DEFENDER );
						PutPlayersInCornerArea( oponents, Player_Script.TypePlayer.ATTACKER );
						candidateTothrowBand = SearchPlayerNearBall( oponents );
					}
					else {	
						PutPlayersInCornerArea( oponents, Player_Script.TypePlayer.DEFENDER );
						PutPlayersInCornerArea( players, Player_Script.TypePlayer.ATTACKER );
						candidateTothrowBand = SearchPlayerNearBall( players );
					}				
				
					candidateTothrowBand.transform.position = new Vector3 ( cornerSource.position.x, candidateTothrowBand.transform.position.y, cornerSource.position.z);
					
				
					if ( whoLastTouched.tag == "PlayerTeam1" ) {
					
						candidateTothrowBand.GetComponent<Player_Script>().temporallyUnselectable = true;
						candidateTothrowBand.GetComponent<Player_Script>().timeToBeSelectable = 1.0f;
				
						candidateTothrowBand.transform.LookAt( SearchPlayerNearBall( oponents ).transform.position);

					}
					else {
						candidateTothrowBand.transform.LookAt( center ); 
					}
				
	
				
					candidateTothrowBand.transform.Rotate(0, sphere.fHorizontal*10.0f, 0);
					candidateTothrowBand.GetComponent<Player_Script>().state = Player_Script.Player_State.THROW_CORNER;
				
					sphere.GetComponent<Rigidbody>().isKinematic = true;
				
					sphere.gameObject.transform.position = cornerSource.position;
				
				
					target_throw_band = candidateTothrowBand.transform.position + candidateTothrowBand.transform.forward;
				
				
					candidateTothrowBand.GetComponent<Animation>().Play("repose");
					state = InGameState.CORNER_AIMING;
				
				break;

					
			case InGameState.CORNER_AIMING:


				candidateTothrowBand.transform.LookAt( target_throw_band );
				candidateTothrowBand.GetComponent<Player_Script>().state = Player_Script.Player_State.THROW_CORNER;
				
				sphere.GetComponent<Rigidbody>().isKinematic = true;

				if ( whoLastTouched.tag != "PlayerTeam1" ) {
				
					target_throw_band += Camera.main.transform.right*(sphere.fHorizontal/10.0f);
					
					if (sphere.bPassButton) {
						candidateTothrowBand.GetComponent<Animation>().Play("steps_behind"); //pas en arrière
						state = InGameState.CORNER_REMOVING;
		
					}
						
				} else {
					
					timeToThrowOponent -= Time.deltaTime;
					
					if ( timeToThrowOponent < 0.0f ) {					
						timeToThrowOponent = 3.0f;
						sphere.gameObject.GetComponent<Rigidbody>().isKinematic = true;
						candidateTothrowBand.GetComponent<Animation>().Play("steps_behind");
						state = InGameState.CORNER_REMOVING;
					}
					
				}

				
				
			break;

				
			case InGameState.CORNER_REMOVING:
			
				candidateTothrowBand.transform.position -= candidateTothrowBand.transform.forward * Time.deltaTime;
				
				if ( candidateTothrowBand.GetComponent<Animation>().IsPlaying("steps_behind") == false ) {
					
					candidateTothrowBand.GetComponent<Animation>().Play("corner_kick"); //lancer depuis le corner
					state = InGameState.CORNER_REMOVING_2;
				}
				
			break;				
				
				
			case InGameState.CORNER_REMOVING_2:
				

				if ( candidateTothrowBand.GetComponent<Animation>()["corner_kick"].normalizedTime >= 0.5f && sphere.gameObject.GetComponent<Rigidbody>().isKinematic == true ) {
					sphere.gameObject.GetComponent<Rigidbody>().isKinematic = false;
					sphere.gameObject.GetComponent<Rigidbody>().AddForce( candidateTothrowBand.transform.forward*7000.0f + new Vector3(0.0f, 3300.0f, 0.0f) );					
				} 
				
				
				if ( candidateTothrowBand.GetComponent<Animation>().IsPlaying("corner_kick") == false ) {
					state = InGameState.CORNER_REMOVED;
				}
				
				
				
			break;
				
				
				
			case InGameState.CORNER_REMOVED:
				
				candidateTothrowBand.GetComponent<Player_Script>().state = Player_Script.Player_State.MOVE_AUTOMATIC;				
				state = InGameState.PLAYING;
				
			break;
				
				
			case InGameState.THROW_GATE:
				
				sphere.transform.position = throw_goal.position;
				sphere.gameObject.GetComponent<Rigidbody>().isKinematic = true;
				goalKeeper.transform.rotation = throw_goal.transform.rotation;
				goalKeeper.transform.position = new Vector3( throw_goal.transform.position.x, goalKeeper.transform.position.y ,throw_goal.transform.position.z)- (goalKeeper.transform.forward*1.0f);
				goalKeeper.GetComponent<GoalKeeper_Script>().state = GoalKeeper_Script.GoalKeeper_State.THROW_GATE;
							
		
				foreach ( GameObject go in players ) {
					go.GetComponent<Player_Script>().state = Player_Script.Player_State.GO_ORIGIN;
				}
				foreach ( GameObject go in oponents ) {
					go.GetComponent<Player_Script>().state = Player_Script.Player_State.GO_ORIGIN;
				}
				
				sphere.owner = null;

			
				goalKeeper.GetComponent<Animation>().Play("steps_behind");	
				state = InGameState.THROW_GATE_RUN;
				
				
			break;
			case InGameState.THROW_GATE_RUN:
				
				goalKeeper.transform.position -= goalKeeper.transform.forward * Time.deltaTime;
				
				if ( goalKeeper.GetComponent<Animation>().IsPlaying("steps_behind") == false ) {
					goalKeeper.GetComponent<Animation>().Play("corner_kick");	
					state = InGameState.THROW_GATE_REMOVING;
				}
			
				
			break;	
				
			case InGameState.THROW_GATE_REMOVING:
				
				goalKeeper.transform.position += goalKeeper.transform.forward * Time.deltaTime;

				if ( goalKeeper.GetComponent<Animation>()["corner_kick"].normalizedTime >= 0.5f && sphere.gameObject.GetComponent<Rigidbody>().isKinematic == true) {
					sphere.gameObject.GetComponent<Rigidbody>().isKinematic = false;
					float force = Random.Range(5000.0f, 12000.0f);
					sphere.gameObject.GetComponent<Rigidbody>().AddForce( (goalKeeper.transform.forward*force) + new Vector3(0,3000.0f,0) );
				}
	
				if ( goalKeeper.GetComponent<Animation>().IsPlaying("corner_kick") == false ) {

					goalKeeper.GetComponent<GoalKeeper_Script>().state = GoalKeeper_Script.GoalKeeper_State.GO_ORIGIN;	
					state = InGameState.PLAYING;
					
				}
				
			break;

			case InGameState.GOAL:
				
				
				foreach ( GameObject go in players ) {
					go.GetComponent<Player_Script>().state = Player_Script.Player_State.THROW_BAND;
					go.GetComponent<Animation>().Play("repose");
				}
				foreach ( GameObject go in oponents ) {
					go.GetComponent<Player_Script>().state = Player_Script.Player_State.THROW_BAND;
					go.GetComponent<Animation>().Play("repose");
				}
				
					keeper_oponent.GetComponent<GoalKeeper_Script>().state = GoalKeeper_Script.GoalKeeper_State.RESTING;
					keeper.GetComponent<GoalKeeper_Script>().state = GoalKeeper_Script.GoalKeeper_State.RESTING;
				
				timeToKickOff -= Time.deltaTime;
				
				if ( timeToKickOff < 0.0f ) {
					timeToKickOff = 4.0f;
					state = InGameState_Script.InGameState.KICK_OFF;
				}
				
				
			break;


				
			case InGameState.KICK_OFF:
				
				
				foreach ( GameObject go in players ) {
					go.GetComponent<Player_Script>().state = Player_Script.Player_State.MOVE_AUTOMATIC;
					go.transform.position = go.GetComponent<Player_Script>().initialPosition;
				}
				foreach ( GameObject go in oponents ) {
					go.GetComponent<Player_Script>().state = Player_Script.Player_State.MOVE_AUTOMATIC;
					go.transform.position = go.GetComponent<Player_Script>().initialPosition;
				}
				
				keeper.GetComponent<GoalKeeper_Script>().state = GoalKeeper_Script.GoalKeeper_State.RESTING;
				keeper_oponent.GetComponent<GoalKeeper_Script>().state = GoalKeeper_Script.GoalKeeper_State.RESTING;
				
				sphere.owner = null;
				sphere.gameObject.transform.position = center.position;
				sphere.gameObject.GetComponent<Rigidbody>().drag = 0.5f;
				state = InGameState_Script.InGameState.PLAYING;
				
			break;				
				
				
				
			}
		
		}
		
	}
	
	GameObject SearchPlayerNearBall( GameObject[] arrayPlayers) {
		
	    GameObject candidatePlayer = null;
		float distance = 1000.0f;
		foreach ( GameObject player in arrayPlayers ) {			
			
			if ( !player.GetComponent<Player_Script>().temporallyUnselectable ) {
				
				Vector3 relativePos = sphere.transform.InverseTransformPoint( player.transform.position );		
				float newdistance = relativePos.magnitude;
				
				if ( newdistance < distance ) {
				
					distance = newdistance;					
					candidatePlayer = player;					

				}
			}
			
		}
						
		return candidatePlayer;	
	}
	
	
	
	
	void PutPlayersInCornerArea( GameObject[] arrayPlayers, Player_Script.TypePlayer type) {
	
		
		foreach ( GameObject player in arrayPlayers ) {			
			
			if ( player.GetComponent<Player_Script>().type == type ) {
			
			
				float xmin = areaCorner.GetComponent<BoxCollider>().bounds.min.x;
				float xmax = areaCorner.GetComponent<BoxCollider>().bounds.max.x;
				float zmin = areaCorner.GetComponent<BoxCollider>().bounds.min.z;
				float zmax = areaCorner.GetComponent<BoxCollider>().bounds.max.z;
				
				float x = Random.Range( xmin, xmax );
				float z = Random.Range( zmin, zmax );
				
				player.transform.position = new Vector3( x, player.transform.position.y ,z);
				
				
			}
			
			
		}		
		
		
		
	}
	
	
	
}
