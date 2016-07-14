using UnityEngine;
using System.Collections;

public class countChildren : MonoBehaviour {
	public int numChildren;
	public int maxChildren;

	public void cullChildren(float percentage){
		foreach (Transform child in this.gameObject.transform) {
			if(Random.value < percentage)
				Destroy (child.gameObject);
		}
	}
	

	public void smartCullChildren(float percentage, int maxNeighbors, float radius, bool lonely) {
		foreach (Transform child in this.gameObject.transform) {
			if(Random.value < percentage){
				if(Physics.OverlapSphere(child.position, radius).Length > maxNeighbors && !lonely ||
				   Physics.OverlapSphere(child.position, radius).Length < maxNeighbors && lonely){
					Destroy (child.gameObject);
				}
			}
		}
	}

	public void kidCount(){
		numChildren = this.gameObject.transform.childCount;
	}

	// Use this for initialization
	void Start () {
		InvokeRepeating ("kidCount", 0f, 1f);

	}
	
	// Update is called once per frame
	void Update () {
		if (numChildren > maxChildren){
			cullChildren(.2f);
			kidCount();
		} 
		if (Input.GetKeyDown(KeyCode.Backslash)) {
			smartCullChildren (.7f, 2, .03f, true);
			kidCount ();
		}
	
	}
}
