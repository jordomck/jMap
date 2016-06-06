using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;


public class RobotRender : MonoBehaviour {
	public GameObject robotPrefab;
	public GameObject robot;
	public int timestamp;
	public Dictionary<int, GameObject> stamps;
	public GameObject cachedOdomParent;
	public bool __________________;
	public StreamReader reader, rotationReader;
	public long linesRead;

	public void openReader(string filename)
	{
		try {
		reader = new StreamReader (filename, Encoding.Default);
		}
		catch (FileNotFoundException e)
		{
			print("ERROR: ");
			print(e.Message);
		}
	}

	public void openRotationReader(string filename)
	{
		try {
			rotationReader = new StreamReader (filename, Encoding.Default);
		}
		catch (FileNotFoundException e)
		{
			print("ERROR: ");
			print(e.Message);
		}
	}


	public void tryToReadALine()
	{
		try {
			string line = reader.ReadLine();
			if (line != null)
			{
				linesRead++;
				string[] entries = line.Split();
				Vector4 coords = Vector4.zero;
				for(int i = 0; i < entries.Length; i++){
					float parsed;
					float.TryParse (entries[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsed);
					coords[i] = parsed;
				}
				if(entries.Length == 4)
				{
					robot.transform.position = new Vector3(coords[0], coords[1], coords[2]);
					//print(coords[3]);
					timestamp = (int)coords[3];
				}
				//if(linesRead % 100 == 0)
				//	print ("read 100 lines!");

			}
		}
		catch (EndOfStreamException e)
		{
			print("ERROR: ");
			print(e.Message);
		}

		try {
			string line = rotationReader.ReadLine();
			if (line != null)
			{
				string[] entries = line.Split();
				Vector4 coords = Vector3.zero;
				for(int i = 0; i < entries.Length; i++){
					float parsed;
					float.TryParse (entries[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsed);
					coords[i] = parsed;
				}
				if(entries.Length == 4)
				{
					robot.transform.rotation = new Quaternion(coords[0], coords[1], coords[2], coords[3]) * Quaternion.Euler (90f,0,0f);
					//robot.transform.rotation *= Quaternion.Euler (0, 0, 0);
				}


			}
		}    
		catch (EndOfStreamException e) {
			print("ERROR: ");
			print(e.Message);
		}





		GameObject cachedTransform = new GameObject();
		cachedTransform.name = "CACHE";
		cachedTransform.transform.position = robot.transform.position;
		cachedTransform.transform.rotation = robot.transform.rotation;
		cachedTransform.transform.parent = cachedOdomParent.transform;
		//cachedTransform.position = robot.transform.position;
		//cachedTransform.rotation = robot.transform.rotation;
		stamps [(int)timestamp] = cachedTransform;











	}

	void Awake() {
		robot = (GameObject)Instantiate (robotPrefab, Vector3.zero, Quaternion.identity);
		robot.GetComponent<TrailRenderer> ().enabled = false;
		robot.name = "robot";
		stamps = new Dictionary<int, GameObject> ();

 	}
	// Use this for initialization
	void Start () {

		openReader("robotinfile.txt");	
		InvokeRepeating ("GrabOdom", 0f, 1f / 200f);
		InvokeRepeating ("ClearCache", 0f, 3f);
	}
	
	// Update is called once per frame
	void GrabOdom() {
		//Camera.main.transform.SetParent (robot.transform);
		openReader ("robotinfile.txt");
		openRotationReader ("rotationinfile.txt");
		tryToReadALine();
		robot.GetComponent<TrailRenderer> ().enabled = true;
		
	}

	void ClearCache() {
		foreach (Transform childTransform in cachedOdomParent.transform) {
			Destroy (childTransform.gameObject);
		}
		stamps.Clear ();
	
	}
}

