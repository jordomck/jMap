using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	public GameObject poi;
	public bool roboRotate;
	public float xTolerance, yTolerance;
	public bool ______________;
	public Vector3 offset;
	// Use this for initialization
	void Start () {
		poi = Camera.main.GetComponent<RobotRender> ().robot;
	}
	
	// Update is called once per frame
	void Update () {
		if (roboRotate)
			Camera.main.transform.SetParent (poi.transform);
		//Vector3 cameraRotation = Camera.main.transform.rotation.eulerAngles;
		//cameraRotation.z = poi.transform.rotation.x;
		////Camera.main.transform.rotation = Quaternion.Euler (cameraRotation);
		offset = poi.transform.position - this.gameObject.transform.position;
		Vector3 pos = this.gameObject.transform.position;
		if (Mathf.Abs (offset.x) > xTolerance && offset.x > 0)
			pos.x = poi.transform.position.x - xTolerance;
		if (Mathf.Abs (offset.x) > xTolerance && offset.x < 0)
			pos.x = poi.transform.position.x + xTolerance;
		if (Mathf.Abs (offset.y) > yTolerance && offset.y > 0)
			pos.y = poi.transform.position.y - yTolerance;
		if (Mathf.Abs (offset.y) > yTolerance && offset.y< 0)
			pos.y = poi.transform.position.y + yTolerance;
		this.gameObject.transform.position = pos;
	}
}
