using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;


public class PointRender : MonoBehaviour {
	public float survivalMargin;  //the distance a point needs to move from the last iteration in order to be deleted and remade
	public GameObject spherePrefab, pointParent, lineParent, permanentLineParent, parentA, parentB;
	public int renderGap, linesPerFrame;
	public uint timeTravelAmount; //rendergap displays one of each X laser beams. Linesperframe should be consistent at 662.
	public Material lineRendererMaterial;
	public bool __________________;
	public bool stopRendering; //this turns on when 's' is pressed. It removes all points, and stops more from spawning
	public bool grabSampleA, grabSampleB; 
	public string fileName; //filename of laserscan data. this is associated with an odometry timestamp. There are 250 files cycling through.
	public StreamReader reader;
	public GameObject penObj; //empty gameobj that holds pen
	public LineRenderer pen;
	public int vertices; //number of verts on the latest pen
	public int numberOfSpheresCreated; 
	public int linesRead;
	public GameObject robot;
	public GameObject cachedOdom; //stores transform of robot sometime in the past, when laser scan was evaluated
	public uint timestamp; //most recently read stamp from manager file
	public bool wrong; //if wrong, we didn't have stored odometry from when the laser scan occurred, so we can't safely render.
	public Vector3 prevPos; //Stores previously rendered point for line drawing purposes.
	int skipRender; //counter variable to help skip renders if you want to throttle renderer. Don't modify this other than the ++ in getPoints.

	//Stores a new reader in the reader var
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

	//overwrites a txt file with a blank file.
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

	// creates a new linerenderer. If L is held, it will remain indefinitely, otherwise it goes in the queue to delete and is invisible.
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
			//print("READING A LINE!");
			string line = reader.ReadLine();
			if(linesRead % renderGap != 0 ) { //this allows a downsample of laser scans. High rendergap means large performance gain.
				return;
			}
			if (line != null && !wrong) //file is valid, cachedOdom is valid
			{

				string[] readings = line.Split();
				if(readings.Length < 2) //this is just insurance against partially written files
				{
					return;
				}
				//next, turn strings read into floats
				for(int i = 0; i < 2; i++){ 
					float parsed = -1;
					float.TryParse (readings[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsed);
					parsedReadings[i] = parsed;
				}
				if(readings.Length == 2 && parsedReadings[1] != -1) //if [1] is -1, the original data was NaN and we can't render it.
				{
					Vector3 scanDirection = cachedOdom.transform.right; //robot's +x direction is forward from ROS perspective
					//rotate scan direction by the reported scan angle
					scanDirection = Quaternion.AngleAxis (Mathf.Rad2Deg * parsedReadings[0], -cachedOdom.transform.up) * scanDirection;
					//draw a ray of the laser scan
					//Debug.DrawRay(cachedOdom.transform.position + cachedOdom.transform.right * .235f, scanDirection * parsedReadings[1]);
					//add the translation of the robot
					Vector3 coordinate = (cachedOdom.transform.position + cachedOdom.transform.right * .235f) + Vector3.Normalize(scanDirection) * parsedReadings[1];

					//draw lines
					Vector3 vFromPrevPos = coordinate - prevPos;
					if(Vector3.SqrMagnitude(vFromPrevPos) > .05 && pen != null) //line segment too far to continue, time to begin a new line
					{
						vertices += 1;
						pen.SetVertexCount(vertices);
						pen.SetPosition(vertices - 1, prevPos);
						//pen.enabled = true;

						pen = makeNewLine();
						vertices = 1;
						pen.SetPosition (0, coordinate);
					}
					else if (pen != null) { //line segment short enough to connect the dots!
						vertices += 1;
						pen.SetVertexCount (vertices);
						pen.SetPosition (vertices - 1, coordinate);
					}
					prevPos = coordinate; //cache this for the next iteration so we can connect it to the next dot.


					GameObject relevantSphere = GameObject.Find (parsedReadings[0].ToString ()); //pointer for copying to parentA or parentB
					if(GameObject.Find(parsedReadings[0].ToString ()) == null){ //never seen this angle before, as at startup
						GameObject newestSphere = (GameObject)Instantiate(spherePrefab, coordinate, Quaternion.identity);
						newestSphere.transform.parent = pointParent.transform;
						newestSphere.name = parsedReadings[0].ToString();
						relevantSphere = newestSphere;
					} else {
						GameObject oldSphere = GameObject.Find (parsedReadings[0].ToString());
						if(Vector3.Magnitude(coordinate - oldSphere.transform.position) > survivalMargin) //too far to live
						{
							//Destroy(oldSphere);
							//GameObject newestSphere = (GameObject)Instantiate(spherePrefab, coordinate, Quaternion.identity);
							//newestSphere.transform.parent = pointParent.transform;
							//newestSphere.name = parsedReadings[0].ToString();
							oldSphere.transform.position = coordinate;
							relevantSphere = oldSphere;
						} else {
							oldSphere.transform.position= coordinate;
							relevantSphere = oldSphere;
						}
					}

					//if we requested a sample, send a copy over to parentA or parentB for storage
					if((grabSampleA || grabSampleB)){
						GameObject duplicatedSphere = (GameObject)Instantiate((Object)relevantSphere, relevantSphere.transform.position, relevantSphere.transform.rotation);
						duplicatedSphere.GetComponent<BlockFade>().enabled = false;
						if(grabSampleA) {
							duplicatedSphere.transform.parent = parentA.transform;
							duplicatedSphere.GetComponent<MeshRenderer>().material.color = Color.red;
						}
						else if (grabSampleB) {
							duplicatedSphere.transform.parent = parentB.transform;
							duplicatedSphere.GetComponent<MeshRenderer>().material.color = Color.green;
							//duplicatedSphere.transform.localScale = duplicatedSphere.transform.localScale * 1.1f;
						}
					}

				}
				numberOfSpheresCreated++;
			}
			if (linesRead >= 660) //reached EOF, time to spam requests to the moveon file until it gives us the go-ahead to get new laser data
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
					if(stamp > timestamp) //we got the go-ahead to open up a new laser file!
					{

						reader.Close();
						fileName = twoHalves[1];
						openReader (fileName);
						cachedOdom = new GameObject();
						cachedOdom.transform.position = Vector3.zero;
						cachedOdom.transform.rotation = Quaternion.identity;
						timestamp = stamp;
						stamp = stamp - timeTravelAmount;
						if(Camera.main.GetComponent<RobotRender>().stamps.ContainsKey((int)stamp)){
							Destroy (cachedOdom);
							cachedOdom = Camera.main.GetComponent<RobotRender>().stamps[(int)stamp];
							//print (Camera.main.GetComponent<RobotRender>().timestamp - stamp);
							wrong = false;
							grabSampleA = false;
							grabSampleB = false;
							if(Input.GetKey (KeyCode.A)){
								//pointParent.SetActive(false);
								clearChildren(parentA);
								grabSampleA = true;
								parentA.GetComponent<odomStorer>().cachedOdom = cachedOdom.transform;
								parentA.transform.position = cachedOdom.transform.position;
							}
							else if(!Input.GetKey(KeyCode.B) && parentA.transform.childCount > 0){
								clearChildren(parentB);
								grabSampleB = true;
								parentB.GetComponent<odomStorer>().cachedOdom = cachedOdom.transform;
								parentB.transform.position = cachedOdom.transform.position;
							}
						}/* else if (Camera.main.GetComponent<RobotRender>().stamps.ContainsKey((int)stamp - 1)) {
							Destroy (cachedOdom);
							cachedOdom = Camera.main.GetComponent<RobotRender>().stamps[(int)stamp - 1];
							wrong = false;
						} */else {
							//print("WRONG");
							wrong = true;
							cachedOdom.transform.position = robot.transform.position;
							cachedOdom.transform.rotation = robot.transform.rotation;
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
		stopRendering = false;
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
		InvokeRepeating ("grabPoints", 0f, .2f);
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

	void Update() {
		if (Input.GetKeyDown(KeyCode.S)) {
			stopRendering = !stopRendering;
			print ("RENDER TOGGLE");
		}
		if (stopRendering) {
			clearChildren (pointParent);
		}
	}

	// Update is called once per frame
	void grabPoints () {
		skipRender++;
		if(skipRender % 1 != 0 || stopRendering)
			return;
		clearLines ();
		for(linesRead = 0; linesRead < linesPerFrame; linesRead++)
			tryToReadALine();
		if (parentB.transform.childCount > 0) {
			if(Camera.main.GetComponent<ICP>().runICP()){
				Camera.main.GetComponent<ICP>().transferParenthood();
			}
			else{
				clearChildren(parentB);
			}
		}


	}

	void clearChildren(GameObject parent) {
		foreach (Transform child in parent.transform) {
			Destroy (child.gameObject);
		}
	}


}

