using UnityEngine;
using System.Collections;

public class DeleteIfCollided : MonoBehaviour {
	public float creationTime;
	// Use this for initialization
	void Start () {
		creationTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Permasphere" && creationTime >= other.gameObject.GetComponent<DeleteIfCollided>().creationTime)
			Destroy (this.gameObject);
	}
}
