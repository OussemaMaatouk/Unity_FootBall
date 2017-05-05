using UnityEngine;
using System;
using System.Collections;
public class Player_Script : MonoBehaviour {


public string Name;
public TypePlayer type = TypePlayer.DEFENDER;
public float Speed = 1.0f;	
public float Strong = 1.0f;
public float Control = 1.0f;
	

private const float STAMINA_DIVIDER = 100.0f; 
private const float STAMINA_MIN = 0.3f;	
private const float STAMINA_MAX = 1.0f;	
	
	
public enum TypePlayer {
		DEFENDER,
		MIDDLER,
		ATTACKER
	};
	
public Vector3 actualVelocityPlayer;
private Vector3 oldVelocityPlayer;
public Sphere sphere;
private GameObject[] players;
private GameObject[] oponents;
public Vector3 initialPosition;
private float inputSteer;
private const float initialDisplacement = 20.0f;	
public Transform goalPosition;
public Transform headTransform;	
[HideInInspector]	
public bool temporallyUnselectable = true;
[HideInInspector]	
public float timeToBeSelectable = 1.0f;	
public float maxDistanceFromPosition = 20.0f;	
	
public enum Player_State { 
	   RESTING,
	   GO_ORIGIN,
	   CONTROLLING,
	   PASSING,
	   SHOOTING,
	   MOVE_AUTOMATIC,
	   ONE_STEP_BACK,
	   STOLE_BALL,
	   OPONENT_ATTACK,
	   PICK_BALL,
	   CHANGE_DIRECTION,
	   THROW_BAND,
	   THROW_CORNER
	  };
   
public Player_State state;
private float timeToRemove = 3.0f;	
private float timeToPass = 1.0f;
	
// hand of player in squeleton hierarchy
public Transform hand_bone;
	
public InGameState_Script inGame;
	
public Texture barTexture;
public Texture barStaminaTexture;
private int barPosition=0;
private Quaternion initialRotation;	
	
public float stamina = 64.0f;	
void  Awake (){
	GetComponent<Animation>().Stop();
 	state = Player_State.MOVE_AUTOMATIC; 
}



    void  Start (){
		
	    // changer la taille des joueurs (pour faire de la difference)
	    transform.localScale = new Vector3( UnityEngine.Random.Range(0.9F, 1.1F), UnityEngine.Random.Range(0.9F, 1.1F), UnityEngine.Random.Range(0.9F, 1.1F) );
	    players = GameObject.FindGameObjectsWithTag("PlayerTeam1");
	    oponents = GameObject.FindGameObjectsWithTag("OponentTeam");

	    if ( gameObject.tag == "PlayerTeam1" )
		    initialPosition = new Vector3( transform.position.x, transform.position.y, transform.position.z+initialDisplacement ); 

	    if ( gameObject.tag == "OponentTeam" )
		    initialPosition = new Vector3( transform.position.x, transform.position.y, transform.position.z-initialDisplacement );

            // // modifier vitesse animation pour que ca soit coherent avec les mvmnts		
        GetComponent<Animation>()["back_loop"].speed = 1.5f; //boucle reculer
	    GetComponent<Animation>()["start"].speed = 1.0f; //démarrage
	    GetComponent<Animation>()["start_ball"].speed = 1.0f; //démarrage avec ballon
	    GetComponent<Animation>()["run"].speed = 1.2f; //courir
	    GetComponent<Animation>()["run_ball"].speed = 1.0f; //courir avec ballon
	    GetComponent<Animation>()["pass"].speed = 1.8f; //passe
	    GetComponent<Animation>()["repose"].speed = 1.0f; //repos
	    GetComponent<Animation>()["change_sense"].speed = 1.3f; //changer de direction

	    GetComponent<Animation>()["enter"].speed = 1.2f;	//rentre dedans (mvmnt de tete)
	
		
	    GetComponent<Animation>().Play("repose");	
	
        initialRotation = transform.rotation * headTransform.rotation;
    }
	
	
    // si on controle ce joueur
    void Case_Controlling() {

	    if ( sphere.inputPlayer == gameObject ) { //si il s'agit du joueur en contrle
				
		
		    if ( sphere.fVertical != 0.0f || sphere.fHorizontal != 0.0f ) { //s'il il y'a des commandes de mvmnts
					
			    oldVelocityPlayer = actualVelocityPlayer;
			
			    Vector3 right = inGame.transform.right;
			    Vector3 forward = inGame.transform.forward;
			    //determienr la place de la cible a regarder (le ballon)	
			    right *= sphere.fHorizontal;
			    forward *= sphere.fVertical;
				
			    Vector3 target = transform.position + right + forward;
			    target.y = transform.position.y;
						
			    float speedForAnimation = 5.0f;
			
			    // si c celui qui a le ballon
			    if ( sphere.owner == gameObject ) {
			
				    if ( GetComponent<Animation>().IsPlaying("repose") ) { //si il etait en repos 
					    GetComponent<Animation>().Play("start_ball");
					    speedForAnimation = 1.0f;
				    }
					
				    if ( GetComponent<Animation>().IsPlaying("start_ball") == false ) //si l'animation start ball est terminer alors commancer l'anmation courir ball
					    GetComponent<Animation>().Play("run_ball");
			
			    }else { // si ce n'est pas celui qui a le ballon
					
				    if ( GetComponent<Animation>().IsPlaying("repose") ) { // si il etait en repos
					    GetComponent<Animation>().Play("start");
					    speedForAnimation = 1.0f;
				    }
					
				    if ( GetComponent<Animation>().IsPlaying("start") == false ) // aprés l'animation start commence l'animation run
					    GetComponent<Animation>().Play("run");
					
			    }
				
				
			    transform.LookAt( target ); 
			    float staminaTemp = Mathf.Clamp ((stamina/STAMINA_DIVIDER), STAMINA_MIN ,STAMINA_MAX );
			    actualVelocityPlayer = transform.forward*speedForAnimation*Time.deltaTime*staminaTemp*Speed;
			    transform.position += actualVelocityPlayer; //changer la position du joueur en fonction de ces parametres
			
				
			    // si un changement de sens soudain 
			    float dotp = Vector3.Dot( oldVelocityPlayer.normalized, actualVelocityPlayer.normalized );
			
			    if ( dotp < 0.0f && sphere.owner == gameObject ) {
		
				    GetComponent<Animation>().Play("change_sense");
				    state = Player_State.CHANGE_DIRECTION;
				    transform.forward = -transform.forward;
                    //liberer le ballon et lui donner une petite force pou avance 
                    sphere.owner = null;
                      gameObject.GetComponent<CapsuleCollider>().enabled = false;
                     sphere.gameObject.GetComponent<Rigidbody>().AddForce(  -transform.forward.x*700.0f, -transform.forward.y*700.0f, -transform.forward.z*700.0f );

                }


            } else {
	
			    GetComponent<Animation>().Play("repose");
            }
			
			
		    // pass
		    if ( sphere.bPassButton && sphere.owner == gameObject ) {
			    GetComponent<Animation>().Play("pass");
			    timeToBeSelectable = 2.0f;
			    state = Player_State.PASSING;
			    sphere.pressingPassButton = false;
		    }
				
		    // shoot
		    if ( sphere.bShootButtonFinished && sphere.owner == gameObject ) {
			    GetComponent<Animation>().Play("shoot");
			    timeToBeSelectable = 2.0f;
			    state = Player_State.SHOOTING;
			    sphere.pressingShootButton = false;
			    sphere.bShootButtonFinished = false;
		    }
			
				
						
	    } else {
	
		    state = Player_State.MOVE_AUTOMATIC;
			
	    }
		
    }
	//verifier si y'a qq devan
    bool NoOneInFront( GameObject[] team_players ) {
	
		
	    foreach( GameObject go in team_players ) {

		    Vector3 relativePos = transform.InverseTransformPoint( go.transform.position ); 
		
		    if ( relativePos.z > 0.0f )
			    return true;		
	    }
		
	    return false;
		
    }
	
	
    // NPC control
    void Case_Oponent_Attack() {
		
		    actualVelocityPlayer = transform.forward*5.0f*Time.deltaTime;
		    GetComponent<Animation>().Play("run_ball");
		    Vector3 RelativeWaypointPosition = transform.InverseTransformPoint(goalPosition.position);
		    inputSteer = RelativeWaypointPosition.x / RelativeWaypointPosition.magnitude;
		    transform.Rotate(0, inputSteer*10.0f , 0);
		    float staminaTemp = Mathf.Clamp ((stamina/STAMINA_DIVIDER), STAMINA_MIN ,STAMINA_MAX );
		    transform.position += transform.forward*4.0f*Time.deltaTime*staminaTemp*Speed;
		
		    timeToPass -= Time.deltaTime;
		    
		    if ( timeToPass < 0.0f && NoOneInFront( oponents ) ) {
			    timeToPass = UnityEngine.Random.Range( 1.0f, 5.0f);	
			    state = Player_State.PASSING;
			    GetComponent<Animation>().Play("pass");
			    timeToBeSelectable = 1.0f;
			    temporallyUnselectable = true;
		    }
		
		    float distance = (goalPosition.position - transform.position).magnitude;
		    Vector3 relative = transform.InverseTransformPoint(goalPosition.position);
		
		    if ( distance < 20.0f && relative.z > 0 ) {

			    state = Player_State.SHOOTING;
			    GetComponent<Animation>().Play("shoot");
			    timeToBeSelectable = 1.0f;
			    temporallyUnselectable = true;
			
		    }
		
    }
	    
    void LateUpdate() {
		

		Vector3 relativePos = transform.InverseTransformPoint( sphere.gameObject.transform.position );

		
		if ( relativePos.z > 0.0f ) {
	        //regarder dans le sens du ballon
			Quaternion lookRotation = Quaternion.LookRotation (sphere.transform.position + new Vector3(0, 1.0f,0) - headTransform.position);
			headTransform.rotation = lookRotation * initialRotation ;			
			headTransform.eulerAngles = new Vector3( headTransform.eulerAngles.x, headTransform.eulerAngles.y,  -90.0f);
			
		}
				
	}
	
    void  Update() {
				
	    stamina += 2.0f * Time.deltaTime;

		
	    stamina = Mathf.Clamp(stamina, 1, 100);		
	    switch ( state ) {
				
			
		    case Player_State.THROW_BAND:
			
		    break;

		    case Player_State.THROW_CORNER:
			
		    break;
			
		    case Player_State.CHANGE_DIRECTION:
			
			    if ( !GetComponent<Animation>().IsPlaying("change_sense")) {
				    gameObject.GetComponent<CapsuleCollider>().enabled = true;
				    transform.forward = -transform.forward;
				    GetComponent<Animation>().Play("repose");
				    state = Player_State.CONTROLLING;
			    }
		    break;
			
 		    case Player_State.CONTROLLING:
		
			    if ( gameObject.tag == "PlayerTeam1" ) 
				    Case_Controlling();			
		    break;

		    case Player_State.OPONENT_ATTACK:
			    Case_Oponent_Attack();			
		    break;
			
		    case Player_State.PICK_BALL: //ramasser le ballon
			    transform.position += transform.forward * Time.deltaTime * 5.0f;
						
			    if (GetComponent<Animation>().IsPlaying("enter") == false) {
				
				    if ( gameObject.tag == "OponentTeam" )
					    state = Player_State.OPONENT_ATTACK;
				    else
					    state = Player_State.MOVE_AUTOMATIC;
					
			    }

		    break;
			

		    case Player_State.SHOOTING:
			
			    if (GetComponent<Animation>().IsPlaying("shoot") == false)
				    state = Player_State.MOVE_AUTOMATIC; //aprés le tir

			
			    if (GetComponent<Animation>()["shoot"].normalizedTime > 0.2f && sphere.owner == this.gameObject) {
				    state = Player_State.MOVE_AUTOMATIC;
				    sphere.owner = null;
				    if ( gameObject.tag == "PlayerTeam1" ) {
					    sphere.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(transform.forward.x*30.0f, 5.0f, transform.forward.z*30.0f );
				        barPosition = 0;
				    }
				    else {
				
					    float valueRndY = UnityEngine.Random.Range( 4.0f, 10.0f );
					    sphere.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(transform.forward.x*30.0f, valueRndY, transform.forward.z*30.0f );
				    }
				
			    }
		    break;
			
		    case Player_State.PASSING:

			    if (GetComponent<Animation>().IsPlaying("pass") == false)
				    state = Player_State.MOVE_AUTOMATIC;
	
				
			    if (GetComponent<Animation>()["pass"].normalizedTime > 0.3f && sphere.owner == this.gameObject) {
				    sphere.owner = null;
								
				    GameObject bestCandidatePlayer = null;
				    float bestCandidateCoord = 1000.0f;
				
				
				    if ( gameObject.tag == "PlayerTeam1" ) {
				
					    foreach ( GameObject go in players ) {//tirer le ballon vers le joueur le plus proche de la direction choisie
						
						    if ( go != gameObject ) {
							    Vector3 relativePos = transform.InverseTransformPoint( new Vector3( go.transform.position.x, go.transform.position.y, go.transform.position.z  ) );
									
							    float magnitude = relativePos.magnitude;
							    float direction = Mathf.Abs(relativePos.x);
							
							    if ( relativePos.z > 0.0f && direction < 5.0f && magnitude < 15.0f && (direction < bestCandidateCoord) ) {
								    bestCandidateCoord = direction;
								    bestCandidatePlayer = go;
								
							    }
						    }
							
					    }
				
				    } else {
				
					    foreach ( GameObject go in oponents ) { //tirer le ballon vers le joueur le plus proche de la direction choisie

                                    if ( go != gameObject ) {
							    Vector3 relativePos = transform.InverseTransformPoint( new Vector3( go.transform.position.x, go.transform.position.y, go.transform.position.z  ) );
									
							    float magnitude = relativePos.magnitude;
							    float direction = Mathf.Abs(relativePos.x);
							
							    if ( relativePos.z > 0.0f && direction < 15.0f && (magnitude+direction < bestCandidateCoord) ) {
								    bestCandidateCoord = magnitude+direction;
								    bestCandidatePlayer = go;		
							    }
					
						    }
							
					    }
					
				    }
				
				
				
					
				    if ( bestCandidateCoord != 1000.0f ) {
				
					    sphere.inputPlayer = bestCandidatePlayer;
					    Vector3 directionBall = (bestCandidatePlayer.transform.position - transform.position).normalized;
					    float distanceBall1 = (bestCandidatePlayer.transform.position - transform.position).magnitude*1.4f;
                            distanceBall1 = Mathf.Clamp( distanceBall1, 15.0f, 40.0f );
					    sphere.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(directionBall.x*distanceBall1, distanceBall1/5.0f, directionBall.z*distanceBall1 );
				
				    } else {
					    // if not found a candidate just throw the ball forward....
					    sphere.gameObject.GetComponent<Rigidbody>().velocity = transform.forward*20.0f;
					
				    }
	
			
			
			    }
			    break;
 		    case Player_State.GO_ORIGIN:
			
			    GetComponent<Animation>().Play("run");
                //on trouve une position relative a aller depuis al position initial,comme ca on determine les distances verticale et haurzntl 
			    Vector3 RelativeWaypointPosition = transform.InverseTransformPoint(new Vector3( 
														    initialPosition.x, 
														    initialPosition.y, 
														    initialPosition.z ) );
	
                // en divisant la position horizenal / magnitude , on obtien un pct % decimal de l'angle de rotation 
			
			    inputSteer = RelativeWaypointPosition.x / RelativeWaypointPosition.magnitude;

			    if ( inputSteer == 0 && RelativeWaypointPosition.z < 0 )
				    inputSteer = 10.0f;
			
                //retourner a la place d'origine
			    transform.Rotate(0, inputSteer*10.0f , 0);
			    float staminaTemp = Mathf.Clamp ((stamina/STAMINA_DIVIDER), STAMINA_MIN ,STAMINA_MAX );
			    transform.position += transform.forward*3.0f*Time.deltaTime*staminaTemp*Speed;			transform.position += transform.forward*3.0f*Time.deltaTime;

			    if ( RelativeWaypointPosition.magnitude < 1.0f ) {
				    state = Player_State.MOVE_AUTOMATIC;					
			    }
				
 							
		    break;

		    case Player_State.MOVE_AUTOMATIC:
			
			    timeToRemove += Time.deltaTime;				
			    float distance = (transform.position - initialPosition).magnitude;
			
			    // distance du ballon par rapport joueur
			    float distanceBall = (transform.position - sphere.transform.position).magnitude;
			
			    // si le joueur s'eloigne trop de sa place initiale, il revient a sa place
			    if ( distance > maxDistanceFromPosition ) {
			
				    Vector3 ball = sphere.transform.position;
				    Vector3 direction = (ball - transform.position).normalized;
				
				    Vector3 RelativeWaypointP = transform.InverseTransformPoint(new Vector3( 
															    initialPosition.x, 
															    initialPosition.y, 
															    initialPosition.z ) );

				
				    inputSteer = RelativeWaypointP.x / RelativeWaypointP.magnitude;
					
		
				    if ( inputSteer == 0 && RelativeWaypointP.z < 0 )
					    inputSteer = 10.0f;
					
				    transform.Rotate(0, inputSteer*20.0f , 0);
				    GetComponent<Animation>().Play("run");
				    float staminaTemp2 = Mathf.Clamp ((stamina/STAMINA_DIVIDER), STAMINA_MIN ,STAMINA_MAX );
				    transform.position += transform.forward*5.5f*Time.deltaTime*staminaTemp2*Speed;
									
			    } // si on n'est pas trés loin , on avance vers le ballon
			    else {
		
				    Vector3 ball = sphere.transform.position;
				    Vector3 direction = (ball - transform.position).normalized;
				    Vector3 posFinal = initialPosition + ( direction * maxDistanceFromPosition ); 
				
				    Vector3 RelativeWaypointP = new Vector3(posFinal.x, posFinal.y, posFinal.z);
				
				    // aller to Ball position....
				    if ( distanceBall > 5.0f ) {
					    RelativeWaypointP = transform.InverseTransformPoint(new Vector3( 
																    posFinal.x, 
																    posFinal.y, 
																    posFinal.z ) );

				    } else if ( distanceBall < 5.0f && distanceBall > 2.0f ) {
				
					    // garder une distance  "5"par rapport au ballon
					    RelativeWaypointP = transform.InverseTransformPoint(new Vector3( 
																    transform.position.x, 
																    transform.position.y, 
																    transform.position.z ) );
	
				    // si on est trés prés ,on recule avec une animation speciale
				    } else if ( distanceBall < 2.0f ) {
					
					    GetComponent<Animation>().Play("back_loop");
					    state = Player_State.ONE_STEP_BACK;
					    break;
					
				    }
				
				    inputSteer = RelativeWaypointP.x / RelativeWaypointP.magnitude;
	
				    if ( inputSteer == 0 && RelativeWaypointP.z < 0 )
					    inputSteer = 10.0f;

				    if ( inputSteer > 0.0f )
					    transform.Rotate(0, inputSteer*20.0f , 0);
				
			
				    // verifier si l'emplacementest sufisament prés
				    if ( RelativeWaypointP.magnitude < 1.5f ) {
										
					    transform.LookAt( new Vector3( sphere.GetComponent<Transform>().position.x, transform.position.y ,sphere.GetComponent<Transform>().position.z)  );
					    GetComponent<Animation>().Play("repose");		
					    timeToRemove = 0.0f;
					
				    }	else {			

					
					    if ( timeToRemove > 1.0f ) {					
						    GetComponent<Animation>().Play("run");
						    staminaTemp = Mathf.Clamp ((stamina/STAMINA_DIVIDER), STAMINA_MIN , STAMINA_MAX );
						    transform.position += transform.forward*5.5f*Time.deltaTime*staminaTemp*Speed;
					    }
				    }
		
				
			    }
			
		    break;

			
 
 		    case Player_State.RESTING:

			    transform.LookAt( new Vector3( sphere.GetComponent<Transform>().position.x, transform.position.y ,sphere.GetComponent<Transform>().position.z)  );
			    GetComponent<Animation>().Play("repose"); 		  
 		
 		    break;
			
 
			
			
		    case Player_State.ONE_STEP_BACK://reculer d'un pas
		
			    if (GetComponent<Animation>().IsPlaying("back_loop") == false)
				    state = Player_State.MOVE_AUTOMATIC;

			    transform.position -= transform.forward*Time.deltaTime*6.0f;	
			
		    break;
			
			
		    case Player_State.STOLE_BALL:
			
			    Vector3 relPos = transform.InverseTransformPoint( sphere.transform.position );
			    inputSteer = relPos.x / relPos.magnitude;
			    transform.Rotate(0, inputSteer*20.0f , 0);
			
			    GetComponent<Animation>().Play("run");
			    float staminaTemp3 = Mathf.Clamp ((stamina/STAMINA_DIVIDER), STAMINA_MIN ,STAMINA_MAX );
			    transform.position += transform.forward*4.5f*Time.deltaTime*staminaTemp3*Speed;
			
			
		    break;
			
			
	    };


        // aprés tir / passe le joueur devien non selectionable pr un moment
        timeToBeSelectable -= Time.deltaTime;

        if ( timeToBeSelectable < 0.0f )
		    temporallyUnselectable = false;
	    else
		    temporallyUnselectable = true;

    }
	
	
	void OnCollisionStay( Collision coll ) {
	
		
		
		if ( coll.collider.transform.gameObject.tag == "Ball" && !gameObject.GetComponent<Player_Script>().temporallyUnselectable ) {
			
			
			inGame.lastTouched = gameObject;
		
			

			// voler le ballon d'un enemy
			if ( sphere.owner && (sphere.owner.tag != gameObject.tag) ) {
				
				
				
				if ( (gameObject.tag == "PlayerTeam1" && sphere.bPassButton) || gameObject.tag == "OponentTeam" ) {
				
					// comparer la force des deux joueurs
					
					float myStrong = (this.Strong + UnityEngine.Random.Range(0.0f, 1.0f));
					float oponentStrong = (sphere.owner.GetComponent<Player_Script>().Strong + UnityEngine.Random.Range(0.0f, 1.0f))/* + 10.0f*/;
					if ( myStrong < oponentStrong  ) {
					
						Debug.Log("joueur en possession plus fort " + myStrong + " vs " + oponentStrong );
						
						this.gameObject.GetComponent<Animation>().Play("back_loop");
						this.gameObject.GetComponent<Player_Script>().state = Player_State.ONE_STEP_BACK;
						this.gameObject.GetComponent<Player_Script>().timeToBeSelectable = 0.5f;
						this.gameObject.GetComponent<Player_Script>().temporallyUnselectable = true;
						
						return;
					} else {

						Debug.Log("joueur basculant plus fort " + myStrong + " vs " + oponentStrong);
						
					}
                    sphere.owner.GetComponent<Player_Script>().state = Player_State.ONE_STEP_BACK;
					sphere.owner.GetComponent<Player_Script>().timeToBeSelectable = 2.0f;
					sphere.owner.GetComponent<Player_Script>().temporallyUnselectable = true;
					sphere.owner = gameObject;
					sphere.owner.GetComponent<Player_Script>().state = Player_State.PICK_BALL;
					sphere.owner.GetComponent<Animation>().Play("enter");	
					return;
				} else {	
					return;
				}
			}
			
			
			Vector3 relativePos = transform.InverseTransformPoint( sphere.gameObject.transform.position );
			
					
			// coller le ballon seulement si la collision est en bas
			if ( relativePos.y < 0.35f ) { 
				coll.rigidbody.rotation = Quaternion.identity;
				GameObject ball = coll.collider.transform.gameObject;
				ball.GetComponent<Sphere>().owner = gameObject;
				if ( gameObject.tag == "OponentTeam" ) {
					state = Player_Script.Player_State.OPONENT_ATTACK;
				}		
			}
		}
	}
			
	void OnGUI() {//affichage de la texture lors de tir / passe	
			if ( sphere.timeShootButtonPressed > 0.0f && sphere.inputPlayer == this.gameObject) {
				
				Vector3 posBar = Camera.main.WorldToScreenPoint( headTransform.position + new Vector3(0,0.8f,0) );
				GUI.DrawTexture( new Rect( posBar.x-30, (Screen.height-posBar.y), barPosition, 10 ), barTexture );
				
				barPosition = (int)(sphere.timeShootButtonPressed * 128.0f);
				if ( barPosition >= 99 )
					barPosition = 99;
				
			}
			
			
			
			if ( sphere.owner == this.gameObject ) {
			
				Vector3 posBar = Camera.main.WorldToScreenPoint( headTransform.position + new Vector3(0,1.0f,0) );
				GUI.DrawTexture( new Rect( posBar.x-30, (Screen.height-posBar.y), (int)stamina, 10 ), barStaminaTexture );
				stamina -= 1.5f * Time.deltaTime;	
			}
		}
	}