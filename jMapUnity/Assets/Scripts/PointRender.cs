using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;


public class PointRender : MonoBehaviour {
	public float survivalMargin;
	public GameObject spherePrefab, pointParent;
	public int renderGap, linesPerFrame;
	public bool __________________;
	public StreamReader reader;
	public int numberOfSpheresCreated;
	public int linesRead;
	public GameObject robot;
	public Transform cachedOdom;
	public uint timestamp;
	public bool wrong;


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

	public void clearFile(string filename)
	{
		try {
		 	StreamWriter writer = new StreamWriter (filename);
			writer.Write("");
			writer.Close ();
		}
		catch (FileNotFoundException e)
		{
			print("ERROR: ");
			print(e.Message);
		}

	}
	

	public void tryToReadALine()
	{
		Vector2 parsedReadings = Vector2.zero;
		try {
			//TEMP
			//cachedOdom = robot.transform;
			//TEMP
			//print("READING A LINE!");
			string line = reader.ReadLine();
			linesRead++;
			if(linesRead % renderGap != 0 ) {
				return;
			}
			if (line != null && !wrong)
			{

				string[] readings = line.Split();
				if(readings.Length < 2)
				{
					return;
				}
				for(int i = 0; i < 2; i++){
					float parsed = -1;
					float.TryParse (readings[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsed);
					parsedReadings[i] = parsed;
				}
				if(readings.Length == 2 && parsedReadings[1] != -1)
				{
					Vector3 scanDirection = cachedOdom.forward;
					scanDirection = Quaternion.AngleAxis (Mathf.Rad2Deg * parsedReadings[0] - 90f, -cachedOdom.up) * scanDirection;
					Debug.DrawRay(cachedOdom.position, scanDirection);
					Vector3 coordinate = cachedOdom.position + cachedOdom.right* .2f + Vector3.Normalize(scanDirection) * parsedReadings[1];
					if(GameObject.Find(parsedReadings[0].ToString ()) == null){ //never seen this angle before, as at startup
						GameObject newestSphere = (GameObject)Instantiate(spherePrefab, coordinate, Quaternion.identity);
						newestSphere.transform.parent = pointParent.transform;
						newestSphere.name = parsedReadings[0].ToString();
					} else {
						GameObject oldSphere = GameObject.Find (parsedReadings[0].ToString());
						if(Vector3.Magnitude(coordinate - oldSphere.transform.position) > survivalMargin) //too far to live
						{
							Destroy(oldSphere);
							GameObject newestSphere = (GameObject)Instantiate(spherePrefab, coordinate, Quaternion.identity);
							newestSphere.transform.parent = pointParent.transform;
							newestSphere.name = parsedReadings[0].ToString();
						} else {
							oldSphere.transform.position= coordinate;
						}
					}
					//if(GameObject.Find(parsedReadings[0].ToString()) != null) {
					//	Destroy (GameObject.Find (parsedReadings[0].ToString ()));
					//}
					//GameObject newestSphere = (GameObject)Instantiate(spherePrefab, coordinate, Quaternion.identity);
					//newestSphere.transform.parent = pointParent.transform;
					//newestSphere.name = parsedReadings[0].ToString();

				}
				numberOfSpheresCreated++;
			}
			if (line == null)
			{
				try {
					StreamReader moveon = new StreamReader ("infilerefresh.txt", Encoding.Default);
					uint stamp = 0;
					uint.TryParse (moveon.ReadLine (), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out stamp);
					if(stamp > timestamp)
					{
						reader.Close();
						openReader ("basescaninfile.txt");
						cachedOdom = robot.transform;
						timestamp = stamp;
						//print("LASERSTAMP: ");
						//print(stamp);
						//print("ROBOTSTAMP: ");
						//print(Camera.main.GetComponent<RobotRender>().timestamp);
						//print("DIFF: ");
						//int diff = (int)Camera.main.GetComponent<RobotRender>().timestamp - (int)stamp;
						//print(diff);
						stamp = stamp - 100;
						if(Camera.main.GetComponent<RobotRender>().stamps.ContainsKey((int)stamp)){
							print("Found diff at stamp ");
							//print((int)stamp);
							cachedOdom = Camera.main.GetComponent<RobotRender>().stamps[(int)stamp];
							wrong = false;
						} else {
							print("WRONG");
							wrong = true;
						}
					}
				}
				catch (FileNotFoundException e)
				{
					print("ERROR: ");
					print(e.Message);
				}
			}
		}    
			
		catch (EndOfStreamException e)
		{
			print("ERROR: ");
			print(e.Message);
		}

	}
	
	// Use this for initialization
	void Start () {
		wrong = false;
		numberOfSpheresCreated = 0;
		robot = Camera.main.GetComponent<RobotRender> ().robot;
		cachedOdom = robot.transform;
		clearFile ("basescaninfile.txt");
		openReader("basescaninfile.txt");	
	}
	
	// Update is called once per frame
	void Update () {
		for(int i = 0; i < linesPerFrame; i++)
			tryToReadALine();

	}


}

