using UnityEngine;
using System.Collections;

public class floorLimit : MonoBehaviour {
	public int numChildren, kidLimit;

	public void kidCount(){
		numChildren = this.gameObject.transform.childCount;
	}

	public void cullChildren(float percentage){
		foreach (Transform child in this.gameObject.transform) {
			if(Random.value < percentage)
				Destroy (child.gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		InvokeRepeating ("kidCount", 0f, 1f);
	
	}
	
	// Update is called once per frame
	void Update () {
		if(numChildren > kidLimit){
			cullChildren (.3f);
			kidCount ();
		}
	
	}
}
