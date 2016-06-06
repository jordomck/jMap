using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;


public class PointRender : MonoBehaviour {
	public float survivalMargin;
	public GameObject spherePrefab, pointParent, lineParent, permanentLineParent;
	public int renderGap, linesPerFrame;
	public Material lineRendererMaterial;
	public bool __________________;
	public string fileName;
	public StreamReader reader;
	public GameObject penObj;
	public LineRenderer pen;
	public int vertices;
	public int numberOfSpheresCreated;
	public int linesRead;
	public GameObject robot;
	public GameObject cachedOdom;
	public uint timestamp;
	public bool wrong;
	public Vector3 prevPos;
	int skipRender;


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

	public LineRenderer makeNewLine()
	{
		GameObject newPenObj = new GameObject ();
		newPenObj.name = "LineCreator";
		newPenObj.transform.SetParent (lineParent.transform);

		newPenObj.AddComponent<LineRenderer> ();
		LineRenderer pencil = newPenObj.GetComponent<LineRenderer> ();
		pencil.material = lineRendererMaterial;
		pencil.SetWidth (0.05f, 0.05f);
		pencil.enabled = false;
		if (Input.GetKey (KeyCode.L)) {
			newPenObj.transform.SetParent (permanentLineParent.transform);
			pencil.enabled = true;
		}
		return pencil;
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
					Vector3 scanDirection = cachedOdom.transform.right; //robot's forward
					scanDirection = Quaternion.AngleAxis (Mathf.Rad2Deg * parsedReadings[0], -cachedOdom.transform.up) * scanDirection;
					Debug.DrawRay(cachedOdom.transform.position + cachedOdom.transform.right * .235f, scanDirection * parsedReadings[1]);
					Vector3 coordinate = (cachedOdom.transform.position + cachedOdom.transform.right * .235f) + Vector3.Normalize(scanDirection) * parsedReadings[1];
					Vector3 vFromPrevPos = coordinate - prevPos;
					if(Vector3.SqrMagnitude(vFromPrevPos) > .05 && pen != null)
					{
						vertices += 1;
						pen.SetVertexCount(vertices);
						pen.SetPosition(vertices - 1, prevPos);
						//pen.enabled = true;

						pen = makeNewLine();
						vertices = 1;
						pen.SetPosition (0, coordinate);
					}
					else if (pen != null) {
						vertices += 1;
						pen.SetVertexCount (vertices);
						pen.SetPosition (vertices - 1, coordinate);
					}

					//if(!Input.GetKey (KeyCode.L))
					//	Destroy(pen);
					
					prevPos = coordinate;
					if(GameObject.Find(parsedReadings[0].ToString ()) == null){ //never seen this angle before, as at startup
						GameObject newestSphere = (GameObject)Instantiate(spherePrefab, coordinate, Quaternion.identity);
						newestSphere.transform.parent = pointParent.transform;
						newestSphere.name = parsedReadings[0].ToString();
					} else {
						GameObject oldSphere = GameObject.Find (parsedReadings[0].ToString());
						if(Vector3.Magnitude(coordinate - oldSphere.transform.position) > survivalMargin) //too far to live
						{
							//Destroy(oldSphere);
							//GameObject newestSphere = (GameObject)Instantiate(spherePrefab, coordinate, Quaternion.identity);
							//newestSphere.transform.parent = pointParent.transform;
							//newestSphere.name = parsedReadings[0].ToString();
							oldSphere.transform.position = coordinate;
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
			if (linesRead >= 660)
			{
				//print(linesRead);
				try {
					StreamReader moveon = new StreamReader ("infilerefresh.txt", Encoding.Default);
					uint stamp = 0;
					string moveOnString = moveon.ReadLine ();
					string[] twoHalves = new string[2];
					if(moveOnString != null) {
						twoHalves = moveOnString.Split();
						uint.TryParse (twoHalves[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out stamp);
					}
					if(stamp > timestamp)
					{
						reader.Close();
						fileName = twoHalves[1];
						openReader (fileName);
						cachedOdom = new GameObject();
						cachedOdom.transform.position = Vector3.zero;
						cachedOdom.transform.rotation = Quaternion.identity;
						timestamp = stamp;
						stamp = stamp;
						if(Camera.main.GetComponent<RobotRender>().stamps.ContainsKey((int)stamp)){
							Destroy (cachedOdom);
							cachedOdom = Camera.main.GetComponent<RobotRender>().stamps[(int)stamp];
							print (Camera.main.GetComponent<RobotRender>().timestamp - stamp);
							wrong = false;
						} else if (Camera.main.GetComponent<RobotRender>().stamps.ContainsKey((int)stamp - 1)) {
							Destroy (cachedOdom);
							cachedOdom = Camera.main.GetComponent<RobotRender>().stamps[(int)stamp - 1];
							wrong = false;
						} else {
							//print("WRONG");
							wrong = true;
							Destroy (cachedOdom);
						}
					} else {
						//print ("NOT EVEN WRONG");
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
		skipRender = 0;
		pen = makeNewLine();
		//print (pen.GetType());
		vertices = 0;
		prevPos = new Vector3 (9999f, 9999f, 9999f);
		wrong = false;
		numberOfSpheresCreated = 0;
		robot = Camera.main.GetComponent<RobotRender> ().robot;
		cachedOdom = new GameObject();
		cachedOdom.name = "CACHEFROMPOINTRENDERSTART";
		cachedOdom.transform.position = robot.transform.position;
		cachedOdom.transform.rotation = robot.transform.rotation;
		fileName = "laserScans/baseScanInfile1";
		clearFile (fileName);
		openReader(fileName);
		InvokeRepeating ("grabPoints", 0f, .008f);
	}

	void clearLines(){
		int counter = 0;
		foreach(Transform child in lineParent.transform){
			counter++;
			Destroy (child.gameObject);
		}
		pen = makeNewLine ();
		vertices = 0;
	}

	// Update is called once per frame
	void grabPoints () {

		skipRender++;
		if(skipRender % 1 != 0)
			return;
		clearLines ();
		for(linesRead = 0; linesRead < linesPerFrame; linesRead++)
			tryToReadALine();

	}


}

