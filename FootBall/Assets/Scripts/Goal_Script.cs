using UnityEngine;
using System.Collections;

public class Goal_Script : MonoBehaviour {
	
	public Sphere sphere;
	public GameObject goalKeeper;
	public InGameState_Script ingame;
	public MeshFilter red;
	private Vector3[] arrayOriginalVertices;	
	
	// Use this for initialization
	void Start () {
	
		
		sphere = (Sphere)GameObject.FindObjectOfType( typeof(Sphere) );	
		
		
		arrayOriginalVertices = new Vector3[ red.mesh.vertices.Length ];
		
		for (int f=0; f< red.mesh.vertices.Length; f++) {
			arrayOriginalVertices[f] = red.mesh.vertices[f];
		}
    }
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnTriggerEnter( Collider other) {

		
		if ( other.gameObject.tag == "Ball"  ) {
			
			sphere.owner = null;
			
			goalKeeper.GetComponent<GoalKeeper_Script>().state = GoalKeeper_Script.GoalKeeper_State.THROW_GATE;
			goalKeeper.GetComponent<Animation>().PlayQueued("repose");

		
			if ( goalKeeper.tag == "GoalKeeper_Oponent" && ingame.state != InGameState_Script.InGameState.GOAL) {
				ingame.score_eq1++;
			} 
			
			if ( goalKeeper.tag == "GoalKeeper" && ingame.state != InGameState_Script.InGameState.GOAL) {			
				ingame.score_eq2++;
			}
			
			
			Camera.main.GetComponent<InGameState_Script>().timeToChangeState = 2.0f;
			Camera.main.GetComponent<InGameState_Script>().state = InGameState_Script.InGameState.GOAL;
		}
		
		
		
	}
	void OnTriggerStay( Collider other ) {
	
		
		
		if ( other.gameObject.tag == "Ball" ) {

			
//			Time.timeScale = 0.1f;
			
			
			Mesh meshRed = red.mesh;
			
			int numberVertex = meshRed.vertexCount;
			Vector3[] arrayVertices = meshRed.vertices;
			
	
			for ( int i=0; i<numberVertex; i++) {
						
				Vector3 worldPos = red.transform.TransformPoint( arrayOriginalVertices[i] );
							
				float distance = (worldPos-other.transform.position).magnitude;
				
				if ( distance < 3.0f ) {
					
					
										
		
				
					Vector3 destLocal = red.transform.InverseTransformPoint( other.transform.position );
					Vector3 sourceLocal = arrayOriginalVertices[i];					
					Vector3 dirLocal = (destLocal-sourceLocal);
				
				
					if (  Vector3.Dot( dirLocal, meshRed.normals[i] ) > 0.0f  ) {
				
		
						float tension = 0.0f;
						Color color = new Color( 0,0,0 );
						if (distance <= 3.0 && distance > 2.0f) {
							tension = 1.5f;
							color = Color.red;
						}

						if (distance <= 2.0 && distance > 1.0f) {
							tension = 0.5f;
							color = Color.green;

						}
					
						if (distance <= 1.0 && distance >= 0.0f) {
							tension = 0.1f;
							color = Color.blue;

						}
					
					
					
						Vector3 finalLocal = sourceLocal + (dirLocal/(distance+0.1f));
						arrayVertices[i] = finalLocal; 
						other.GetComponent<Rigidbody>().drag = 3.0f;
		
					
						Debug.DrawLine(/* other.transform.position,*/ red.transform.TransformPoint( finalLocal ) ,red.transform.TransformPoint( finalLocal ) + new Vector3(0,0.1f,0),  color );
					
			
					
					
					
					} else {
						arrayVertices[i] = arrayOriginalVertices[i];					
					}
					
				} else {
				
					arrayVertices[i] = arrayOriginalVertices[i];
				}
				
				
				
			}
		
		
			meshRed.vertices = arrayVertices;
		
		}		
		
	}
	
	
}
