using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;


public class RobotRender : MonoBehaviour {
	public GameObject robotPrefab;
	public GameObject robot;
	public uint timestamp;
	public Dictionary<int, Transform> stamps;
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
				// Do whatever you need to do with the text line, it's a string now
				// In this example, I split it into arguments based on comma
				// deliniators, then send that array to DoStuff()
				string[] entries = line.Split();
				Vector4 coords = Vector4.zero;
				//print ("read a line!");
				for(int i = 0; i < entries.Length; i++){
					float parsed;
					float.TryParse (entries[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsed);
					coords[i] = parsed;
					//print(coords[i]);
				}
				if(entries.Length == 4)
				{
					robot.transform.position = new Vector3(coords[0], coords[1], coords[2]);
					//print(coords[3]);
					timestamp = (uint)coords[3];
				}
				if(linesRead % 100 == 0)
					print ("read 100 lines!");

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
		catch (EndOfStreamException e)
		{
			print("ERROR: ");
			print(e.Message);
		}

		stamps [(int)timestamp] = robot.transform;


	}

	void Awake() {
		robot = (GameObject)Instantiate (robotPrefab, Vector3.zero, Quaternion.identity);
		robot.name = "robot";
		stamps = new Dictionary<int, Transform> ();
 	}
	// Use this for initialization
	void Start () {

		openReader("robotinfile.txt");	
	}
	
	// Update is called once per frame
	void Update () {
		openReader ("robotinfile.txt");
		openRotationReader ("rotationinfile.txt");
		tryToReadALine();
		
	}
}

