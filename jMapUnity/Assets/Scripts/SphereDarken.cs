using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SphereDarken : MonoBehaviour {
	public GameObject robot;
	public float maxDarkenDistance;
	public bool __________________;
	public Renderer rend;


	// Use this for initialization
	void Start () {
		rend = this.gameObject.GetComponent<MeshRenderer> ();
	
	}
	
	// Update is called once per frame
	void Update () {

		//the robot reference is established in the Start() function of robotRender.
		robot = Camera.main.GetComponent<RobotRender> ().robot;
		float distanceToRobot = Vector3.Magnitude (robot.transform.position - this.gameObject.transform.position);
		float percentDarkened = distanceToRobot / maxDarkenDistance;
		if (percentDarkened > 1f)
			percentDarkened = 1f;
		//print (percentDarkened);
		float color = 350f - 400f * percentDarkened;
		rend.material.color = new Color (percentDarkened, percentDarkened, percentDarkened);
		
	}
}
