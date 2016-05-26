using UnityEngine;
using System.Collections;

public class RosStarter : MonoBehaviour {

	// Use this for initialization
	void Start () {
		System.Diagnostics.Process proc = new System.Diagnostics.Process();
		proc.EnableRaisingEvents=false;
		proc.StartInfo.FileName="/bin/bash";
		proc.StartInfo.Arguments = "-c roscore";
		proc.StartInfo.UseShellExecute = false;
		proc.StartInfo.RedirectStandardError = true;
		proc.StartInfo.RedirectStandardInput = true;
		proc.StartInfo.RedirectStandardOutput = true;
		proc.WaitForExit ();
		proc.Start();
		

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
