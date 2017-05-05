
using UnityEngine;
using System.Collections;


public class Sphere : MonoBehaviour {
	
	public GameObject owner;	// le joueur qui a le ballon, si null alors le ballon est « orphelin »
	public GameObject inputPlayer;	// jjoueur choisi pour être contrôlé par le joueur
	public GameObject lastInputPlayer;  // dernier joueur choisi pour être contrôlé par le joueur
    private GameObject[] players;
	private GameObject[] oponents;
	public Transform shadowBall;
	public Transform blobPlayerSelected;
	public float timeToSelectAgain = 0.0f;
	public GameObject lastCandidatePlayer;
	
	[HideInInspector]	
	public float fHorizontal;
	[HideInInspector]	
	public float fVertical;
	[HideInInspector]	
	public bool bPassButton;
	[HideInInspector]	
	public bool bShootButton;
	[HideInInspector]
	public bool bShootButtonFinished;
	[HideInInspector]		
	public bool pressingShootButton = false;
	[HideInInspector]	
	public bool pressingPassButton = false;
	[HideInInspector]	
	public bool pressingShootButtonEnded = false;
	
	public InGameState_Script inGame;
	public float timeShootButtonPressed = 0.0f;


	// Use this for initialization
	void Start () {
		players = GameObject.FindGameObjectsWithTag("PlayerTeam1");		//mon equipe
		oponents = GameObject.FindGameObjectsWithTag("OponentTeam");    //l'equipe adverse
		inGame = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<InGameState_Script>();
		blobPlayerSelected = GameObject.FindGameObjectWithTag("PlayerSelected").transform;       //cercle jaune 	
    }


	void LateUpdate() {
	
		shadowBall.position = new Vector3( transform.position.x, 0.35f ,transform.position.z );
		shadowBall.rotation = Quaternion.identity;//mettre l'ombre du ballon en dessous du ballon 

    }
	
	// Update is called once per frame
	void Update () {

		
		// get input
		fVertical = Input.GetAxis("Vertical");
		fHorizontal = Input.GetAxis("Horizontal");

		bPassButton = Input.GetKey(KeyCode.S) || pressingPassButton; //space
		bShootButton = Input.GetKey(KeyCode.D) || pressingShootButton; //leftControl



        //  il a laché le bouton de shoot
        if ( Input.GetKeyUp(KeyCode.D) || pressingShootButtonEnded) {
            bShootButtonFinished = true;
		}
		
		
		if ( bShootButton ) {
			timeShootButtonPressed += Time.deltaTime;
		} else {
			timeShootButtonPressed = 0.0f;
		}

        //  ajuster le ballon au pied du "owner" s'il existe
        if (owner)
        {
            transform.position = owner.transform.position + owner.transform.forward / 1.5f + owner.transform.up / 5.0f;
            float velocity = owner.GetComponent<Player_Script>().actualVelocityPlayer.magnitude; //rapidité
            
            if (fVertical == 0.0f && fHorizontal == 0.0f && owner.tag == "PlayerTeam1")
            {
                velocity = 0.0f;
                gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
            }

            transform.Rotate(owner.transform.right, velocity * 200.0f);

        }


        //en mode PLAYING, trouver le joueur le plus proche, 
        if (inGame.state == InGameState_Script.InGameState.PLAYING)
        {
            ActivateNearestPlayer();
            if (!owner || owner.tag == "PlayerTeam1") //trouver l'enemy le plus proche si le ballon n'est pas dirigé par un enemy
                ActivateNearestOponent();
        }
    }


    void ActivateNearestOponent()
    {
        float distance = 100000.0f;
        GameObject candidatePlayer = null;
        foreach (GameObject oponent in oponents)
        {

            if (!oponent.GetComponent<Player_Script>().temporallyUnselectable) //si le joueur peut etre selectionné
            {

                oponent.GetComponent<Player_Script>().state = Player_Script.Player_State.MOVE_AUTOMATIC;

                Vector3 relativePos = transform.InverseTransformPoint(oponent.transform.position); //de world space -> relative space
                
                float newdistance = relativePos.magnitude; //calculer longeur du vecteur
                //calculer la distance et la mesurer par rapport a la distance actuelle
                if (newdistance < distance)
                {

                    distance = newdistance;
                    candidatePlayer = oponent;

                }
            }

        }


        if (candidatePlayer) //si un enemy a et choisi, il va essayer de voler le ballon
            candidatePlayer.GetComponent<Player_Script>().state = Player_Script.Player_State.STOLE_BALL;


    }


    void ActivateNearestPlayer()
    {

        //   chercher le joueur le plus proche pour qu'il soit selectionné
        lastInputPlayer = inputPlayer;

        float distance = 1000000.0f;
        GameObject candidatePlayer = null;
        foreach (GameObject player in players)
        {

            if (!player.GetComponent<Player_Script>().temporallyUnselectable)
            {

                Vector3 relativePos = transform.InverseTransformPoint(player.transform.position);
                    
                float newdistance = relativePos.magnitude;

                if (newdistance < distance)
                {
                    distance = newdistance;
                    candidatePlayer = player;
                }
            }

        }




        timeToSelectAgain += Time.deltaTime;   //on utilise ce compteur pour eviter que la modification de joueur controllé soient instantanné
        if (timeToSelectAgain > 0.5f)
        {
            inputPlayer = candidatePlayer;
            timeToSelectAgain = 0.0f;
        }else //conttrrller le dernier joueur déja controllé
        {
            candidatePlayer = lastCandidatePlayer;
        }

        lastCandidatePlayer = candidatePlayer;


        if (inputPlayer != null) //si on controle un joueur
        {
            //on met le cercle jaune au dessous du joueur
            blobPlayerSelected.transform.position = new Vector3(candidatePlayer.transform.position.x, candidatePlayer.transform.position.y + 0.1f, candidatePlayer.transform.position.z);
            Quaternion quat = Quaternion.identity;
            blobPlayerSelected.transform.LookAt(new Vector3(blobPlayerSelected.position.x + fHorizontal, blobPlayerSelected.position.y, blobPlayerSelected.position.z + fVertical));


            // mettre le joueur en mode controlling s'il ne shoot et ne pass pas
            if (inputPlayer.GetComponent<Player_Script>().state != Player_Script.Player_State.PASSING &&
                 inputPlayer.GetComponent<Player_Script>().state != Player_Script.Player_State.SHOOTING &&
                 inputPlayer.GetComponent<Player_Script>().state != Player_Script.Player_State.PICK_BALL &&
                inputPlayer.GetComponent<Player_Script>().state != Player_Script.Player_State.CHANGE_DIRECTION
                )
            {
                inputPlayer.GetComponent<Player_Script>().state = Player_Script.Player_State.CONTROLLING;
            }
        }
    }
}

