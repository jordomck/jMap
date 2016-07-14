using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;


public class RobotRender : MonoBehaviour {
	public GameObject robotPrefab;
	public GameObject robot;
	public Vector3 prevRead, prevEuler;
	public Quaternion prevQuat;
	public bool prevQuatFound;
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
					Vector3 read = new Vector3(coords[0], coords[1], coords[2]);
					robot.transform.position += read - prevRead;
					Vector3 beforeFlattening = robot.transform.position;
					beforeFlattening.z = 0;
					robot.transform.position = beforeFlattening;
					prevRead = read;

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

					Quaternion newRot = new Quaternion(coords[0], coords[1], coords[2], coords[3]) * Quaternion.Euler (90f,0,0f);

					if(!prevQuatFound){
						//print ("FIX");
						robot.transform.rotation = newRot;
						prevQuatFound = true;
						prevQuat = newRot;
					}else {

						Quaternion angleChange = newRot * Quaternion.Inverse(prevQuat);
						Vector3 fixDimension = angleChange.eulerAngles;
						//print (fixDimension);
						float temp = fixDimension.y;
						fixDimension.y = fixDimension.z;
						fixDimension.z = temp;
						angleChange = Quaternion.Euler(fixDimension);
						//print (angleChange.ToString());
						Vector3 eulerRot = newRot.eulerAngles;
						robot.transform.rotation *= angleChange;
						prevEuler = eulerRot;
						prevQuat = newRot;

						//robot.transform.eulerAngles = fixDimension;
						

					}

					Vector3 beforeUprighting = robot.transform.eulerAngles;
					if(beforeUprighting.z < 100f){
						beforeUprighting.y = beforeUprighting.z = 90f;
					}
					else if (beforeUprighting.z > 200f){
						beforeUprighting.y = beforeUprighting.z = 270f;
					}
					robot.transform.eulerAngles = beforeUprighting;


				
				

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
		Vector3 pos = Vector3.zero;
		Quaternion rot = Quaternion.identity;
		pos.x = robot.transform.position.x;	
		pos.y = robot.transform.position.y;	
		pos.z = robot.transform.position.z;
		rot.x = robot.transform.rotation.x;
		rot.y = robot.transform.rotation.y;
		rot.z = robot.transform.rotation.z;
		rot.w = robot.transform.rotation.w;
		cachedTransform.transform.position = pos;
		cachedTransform.transform.rotation = rot;
		cachedTransform.transform.parent = cachedOdomParent.transform;
		//cachedTransform.position = robot.transform.position;
		//cachedTransform.rotation = robot.transform.rotation;
		stamps [(int)timestamp] = cachedTransform;











	}

	void Awake() {
		robot = (GameObject)Instantiate (robotPrefab, Vector3.zero, Quaternion.Euler (0,0,0));
		robot.GetComponent<TrailRenderer> ().enabled = false;
		robot.name = "robot";
		stamps = new Dictionary<int, GameObject> ();

 	}
	// Use this for initialization
	void Start () {
		prevRead = robot.transform.position;
		prevEuler = robot.transform.eulerAngles;
		prevQuatFound = false;
		openReader("robotinfile.txt");	
		InvokeRepeating ("GrabOdom", 0f, 1f / 300f);
		InvokeRepeating ("ClearCache", 0f, 1f);
	}

	void GrabOdom() {
		//Camera.main.transform.SetParent (robot.transform);
		openReader ("robotinfile.txt");
		openRotationReader ("rotationinfile.txt");
		tryToReadALine();
		robot.GetComponent<TrailRenderer> ().enabled = true;
		
	}

	void ClearCache() {
		foreach (Transform childTransform in cachedOdomParent.transform) {
			if(!childTransform.gameObject == Camera.main.GetComponent<PointRender>().cachedOdom){
				Destroy (childTransform.gameObject);
			}
			else {
				//print ("spared the cachedOdom from deletion");
			}
		}
		stamps.Clear ();
	
	}
}

