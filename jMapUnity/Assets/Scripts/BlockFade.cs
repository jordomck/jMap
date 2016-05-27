using UnityEngine;
using System.Collections;

public class BlockFade : MonoBehaviour {
	public float lifetime;
	public bool makePermaSpheres;
	public Object spherePrefab;
	public Object lineRendPrefab;
	GameObject robot;
	float start;

	// Use this for initialization
	void Start () {
		start = Time.time;
		robot = Camera.main.GetComponent<RobotRender> ().robot;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - start > lifetime && makePermaSpheres) {
			GameObject newSphere = (GameObject)Instantiate (spherePrefab, this.transform.position, Quaternion.identity);
			newSphere.GetComponent<MeshRenderer> ().material.color = Color.red;
			newSphere.name = Time.time.ToString ();
			//GameObject lineObj = (GameObject)Instantiate (lineRendPrefab, this.transform.position, Quaternion.identity);
			//lineObj.GetComponent<LineRenderer>().SetPosition(0, this.transform.position);
			//lineObj.GetComponent<LineRenderer>().SetPosition(1, robot.transform.position);
			Destroy (this.gameObject);
		} else if (Time.time - start > lifetime)
			Destroy (this.gameObject);
		else if (Time.time - start > lifetime / 2f)
			this.gameObject.GetComponent<MeshRenderer> ().material.color = Color.green;
	}
}
