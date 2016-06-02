using UnityEngine;
using System.Collections;

public class BlockFade : MonoBehaviour {
	public float lifetime;
	public bool makePermaSpheres, castRays;
	public Object spherePrefab;
	public Object lineRendPrefab;
	GameObject robot;
	float start;

	public bool hasRendered;

	// Use this for initialization
	void Start () {
		hasRendered = false;
		start = Time.time;
		robot = Camera.main.GetComponent<RobotRender> ().robot;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - start > lifetime && makePermaSpheres) {
			GameObject newSphere = (GameObject)Instantiate (spherePrefab, this.transform.position, Quaternion.identity);
			newSphere.GetComponent<MeshRenderer> ().material.color = Color.red;
			newSphere.name = Time.time.ToString ();

			Destroy (this.gameObject);
		} else if (Time.time - start > lifetime && castRays && !hasRendered) {
			GameObject lineObj = (GameObject)Instantiate (lineRendPrefab, this.transform.position, Quaternion.identity);
			lineObj.GetComponent<LineRenderer> ().SetPosition (0, this.transform.position);
			lineObj.GetComponent<LineRenderer> ().SetPosition (1, robot.transform.position);
			this.gameObject.name = "WOW SUCH PERMANENT";
			hasRendered = true;
			//Destroy (this.gameObject);
		} else if (Time.time - start > lifetime + Random.insideUnitCircle.x / 2f) {
			Destroy (this.gameObject);
			//print ("");
		}
		//else if (Time.time - start > lifetime / 2f)
		//	this.gameObject.GetComponent<MeshRenderer> ().material.color = Color.green;
	}
}
