using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ICP : MonoBehaviour {
	public Transform parentA, parentB, parentC, floorParent;
	public int iterationsUntilFailure, numSuccessesRequired;
	public float toleranceForBMovement;
	public bool cullMatches;
	public int pairDownsample;
	public float convergeDist;
	public float maxLengthPermissible, rotateSpeed;
	public float prevTotalDist, totalDistance;
	float currMaxRange;
	int successFlag;
	// Use this for initialization
	void Start () {
		InvokeRepeating ("tidyWalls", 0f, 1f);
		successFlag = 0;
		currMaxRange = 1f;
	}

	Dictionary<Transform, Transform> matchPoints(){
		Dictionary<Transform, Transform> dictMatched = new Dictionary<Transform, Transform> ();
		Dictionary<Transform, int> density = new Dictionary<Transform, int> ();
		//print ("matching points now!");
		totalDistance = 0;
		int childCount = 0;
		foreach (Transform child in parentB) {
			childCount++;
			int collCount = 0;

			if(Random.Range(0, pairDownsample) != 0)
				continue;
			Collider[] foundTargets = Physics.OverlapSphere(child.transform.position, currMaxRange);
			float minLength = maxLengthPermissible * maxLengthPermissible;
			Transform bestTransform = child; //if it's still child at the end, we couldn't find a close enough one

			foreach (Collider matchColl in foundTargets) {
				if(matchColl.gameObject == child.gameObject)
					continue;
				if(matchColl.transform.parent == parentB)
					continue;
				collCount++;
				Transform match = matchColl.gameObject.transform;
				Vector3 offset = match.position - child.position;
				if (Vector3.SqrMagnitude (offset) < minLength) {
					minLength = Vector3.SqrMagnitude (offset);
					bestTransform = match;
					totalDistance += Vector3.Magnitude (offset);
				}


			}
			if (bestTransform != child) {
				dictMatched.Add (child, bestTransform);
				//Debug.DrawLine (child.position + Vector3.back, bestTransform.position + Vector3.back, Color.white, .7f);
			} else {
				print ("MinLength: " + minLength);
			}
		}
		if(cullMatches){
			//remove the worst half of matches, based on density.
			int totalDensity = 0;
			float sumDist = 0f;
			List< KeyValuePair<Transform, Transform> > buddyArray = new List< KeyValuePair<Transform, Transform> > ();
			Dictionary<Transform, float> distanceArray = new Dictionary<Transform, float>();
			foreach (KeyValuePair<Transform, Transform> buddies in dictMatched) {
				Collider[] foundTargets = Physics.OverlapSphere(buddies.Key.position, .005f);
				float distance = Vector3.Magnitude(buddies.Key.position - Camera.main.GetComponent<RobotRender>().robot.transform.position);
				distanceArray[buddies.Key] = distance;
				sumDist += distance;
				totalDensity += foundTargets.Length;
				density[buddies.Key] = foundTargets.Length;
				buddyArray.Add( new KeyValuePair<Transform, Transform>(buddies.Key, buddies.Value));
			}
		
			float avgDensity = (float)totalDensity / (float)dictMatched.Count;
			float avgDist = sumDist / dictMatched.Count;
			
			//print (avgDensity);
			foreach(KeyValuePair<Transform, Transform> buddies in buddyArray){
			if(density[buddies.Key] < avgDensity){
				dictMatched.Remove(buddies.Key);
				}
				//if(distanceArray[buddies.Key] > avgDist){
			////		dictMatched.Remove (buddies.Key);
			//	}
			}
		}


		//print ("ChildCount: " + childCount);

		//print (totalDistance);
		prevTotalDist = totalDistance;
		return dictMatched;
	}

	double[,] getHMatrix(Dictionary<Transform, Transform> matched, Vector3 avg1, Vector3 avg2){
		double[,] hMatrix = new double[3,3];
		hMatrix.Initialize();
		foreach(KeyValuePair<Transform, Transform> pair in matched){ 
			Vector3 a = pair.Key.position;// - avg1;
			Vector3 b = pair.Value.position;// - avg2;
			for(int i = 0; i < 3; i++){
				for(int j = 0; j < 3; j++){
					hMatrix[i,j] += (double)a[i] * (double)b[j];
				}
			}
		}
		return hMatrix;
	}

	public void print3x3(double[,] matrix){
		print ("PRINTING MATRIX:");
		for (int i = 0; i < 3; i++) {
			print (matrix[i, 0].ToString() + " " + matrix[i, 1].ToString() + " " + matrix[i, 2].ToString());
		}
	}

	public void smartCullChildren(int maxNeighbors, float radius, bool lonely) {
		foreach (Transform child in parentB) {
			Collider[] foundCollisions = Physics.OverlapSphere (child.position, radius);
			int count = 0;
			foreach(Collider found in foundCollisions){
				if(found.gameObject.transform.parent == parentA){
					count++;
				}
			}

			if(count > maxNeighbors && !lonely){
				Destroy (child.gameObject);
			}
			if(count < maxNeighbors && lonely){
				Destroy (child.gameObject);
			}
		}
	}

	public void transferParenthood(){
		smartCullChildren (3, .005f, false);
		while(parentB.childCount > 0){
			foreach(Transform child in parentB){
				child.gameObject.GetComponent<MeshRenderer>().material.color = Color.black;
				child.parent = parentA;
			}
		}
	}

	public void tidyWalls(){
		foreach (Transform child in parentA) {
			if ( Random.value < .2f ) {
				Collider[] foundCollisions = Physics.OverlapSphere (child.position, .004f);
				int count = 0;
				foreach(Collider found in foundCollisions){
					if(found.gameObject.transform.parent == parentA){
						count++;
					}
				}
				if(count > 3){
					Destroy (child.gameObject);
				}
			}
		}
	}


	public bool alignSubset(float percentage){
		Transform oldB = parentB; //cache this so we can put it back.
		parentB = parentC;
		foreach(Transform child in parentA){
			if(Random.value < percentage){
				child.parent = parentC;
			} else {
				Destroy (child.gameObject);
			}
		}

		//do ICP

		bool returnable = runICP (false);
		//print (parentC.childCount);
		//reset to normal
		if (returnable) {
			print ("SUBSET ALIGNED");

		} else {
			print ("ICP was a failure.");
			Camera.main.GetComponent<PointRender>().clearChildren(parentB.gameObject);

		}
		foreach (Transform child in parentC) {
			child.parent = parentA;
			child.gameObject.GetComponent<MeshRenderer> ().material.color = Color.yellow;
			//child.position += new Vector3 (0, 0, -.3f);
		}
		parentB = oldB; //tidy up afterwards.
		return returnable;
	}

	public bool runICP(bool SLAM){
			successFlag = 0;
		float distanceBMoved = 0f;
		if (parentB.childCount < 4) {
			print ("b wasn't ready to ICP!");
			return false; //b was not ready to go yet!
		}
		Vector3 totalRobotMovementDistance = Vector3.zero;
		currMaxRange = maxLengthPermissible;
		float totalAngleTurned = 0f;
		Vector3 posBeforeICP = Camera.main.GetComponent<RobotRender> ().robot.transform.position;
		Quaternion rotBeforeICP = Camera.main.GetComponent<RobotRender> ().robot.transform.rotation;
		for (int time = 0; time < iterationsUntilFailure; time++) {

			Dictionary<Transform, Transform> matched = new Dictionary<Transform, Transform>();
			int timeout = 0;

			while(matched.Count < 3 && timeout < 3){
				matched = matchPoints ();
				timeout++;
			}
			if(matched.Count < 3){
				print ("Insufficient count: " + matched.Count.ToString());
				continue;
			}
			print ("COUNT:" + matched.Count);
			
			//calculate averages for zero-centering
			Vector3 avg1, avg2;
			avg1 = avg2 = Vector3.zero;
			foreach (KeyValuePair<Transform, Transform> pair in matched) {
				Vector3 key = pair.Key.position;
				key.z = 0;
				pair.Key.position = key;

				Vector3 value = pair.Value.position;
				value.z = 0;
				pair.Value.position = value;

				avg1 += pair.Key.position;
				avg2 += pair.Value.position;
			}
			avg1 = avg1 / matched.Count;
			avg2 = avg2 / matched.Count;
			Vector3 diff = avg1 - avg2;


			
			
			
			//create H matrix
			double[,] hMatrix = getHMatrix (matched, avg1, avg2);
			
			//find SVD of H Matrix
			double[,] Vt = new double[3, 3];
			double[] W = new double[3];
			double[,] U = new double[3, 3];
			Vt.Initialize ();
			W.Initialize ();
			U.Initialize ();
			alglib.rmatrixsvd (hMatrix, 3, 3, 1, 1, 0, out W, out U, out Vt);
			double[,] V = new double[3, 3];
			alglib.rmatrixtranspose (3, 3, Vt, 0, 0, ref V, 0, 0); //transpose Vt back to normal
			
			double[,] Ut = new double[3, 3];
			Ut.Initialize ();
			alglib.rmatrixtranspose (3, 3, U, 0, 0, ref Ut, 0, 0);
			double[,] outMatrix = new double[3, 3];
			outMatrix.Initialize ();
			alglib.rmatrixgemm (3, 3, 3, 1, V, 0, 0, 0, Ut, 0, 0, 0, 0, ref outMatrix, 0, 0);
			double det = alglib.rmatrixdet (outMatrix);
			
			//did this work at all? 
			if (det < -.9 && det > -1.1) {
				List<int> zeroIndices = new List<int>();
				int counter = 0;
				foreach(float elt in W){
					if(Mathf.Abs (elt) < .000000001){
						zeroIndices.Add(counter);
					}
					//print (elt);
					counter ++;
				}
				if(zeroIndices.Count == 0){
					Camera.main.GetComponent<RobotRender>().robot.transform.position = posBeforeICP;
					Camera.main.GetComponent<RobotRender>().robot.transform.rotation = rotBeforeICP;
					print ("ICP truly failed");
					return false;
				}
				foreach(int index in zeroIndices){
					for(int i = 0; i < 3; i++){
						V[i, index] = -V[i, index];
					}
				}
				alglib.rmatrixgemm (3, 3, 3, 1, V, 0, 0, 0, Ut, 0, 0, 0, 0, ref outMatrix, 0, 0);
				//print ("ICP attempted to save itself");
			} else {
				//print ("ICP didn't need to save itself");
			}

			distanceBMoved = 0f;



			Vector3 avgStartPos = Vector3.zero;
			Vector3 avgEndPos = Vector3.zero;
			foreach (Transform trans in parentB) {

				Vector3 newPos = trans.position;
				avgStartPos += trans.position;
				double[,] ending = new double[3, 1];
				double[,] posVector = new double[3, 1];
				for (int i = 0; i < 3; i++) {
					posVector [i, 0] = newPos [i];
				}
				alglib.rmatrixgemm (3, 1, 3, 1, outMatrix, 0, 0, 0, posVector, 0, 0, 0, 0, ref ending, 0, 0); 
				newPos.x = (float)ending [0, 0];
				newPos.y = (float)ending [1, 0];
				newPos.z = (float)ending [2, 0];
				
				newPos -= diff;
				if (!float.IsNaN (newPos.x) && !float.IsNaN (newPos.y) && !float.IsNaN (newPos.z)){
					distanceBMoved += Vector3.Magnitude(trans.position - newPos);
					trans.position = newPos;
					avgEndPos += trans.position;
				} else{
					print ("B point movement experiencing NaN problems!");
				}
				ending = null;
				posVector = null;
			}
			if(parentB.childCount > 0){
				avgStartPos = avgStartPos / parentB.childCount;
				avgEndPos = avgEndPos / parentB.childCount;
			}

			diff = avgEndPos - avgStartPos;


			if(SLAM){
				//EVERYBODY GET UP IT'S SLAM TIME
				GameObject odomCache = Camera.main.GetComponent<PointRender>().cachedOdom;
				Transform odom;
				try{
					odom = odomCache.transform;
				} catch(MissingReferenceException e) {
					odom = Camera.main.GetComponent<RobotRender>().robot.transform;
				}
				Vector3 roboPos = odom.position;
				Vector3 startPos = odom.position;
				double[,] endingRobo = new double[3,1];
				double[,] posVectorRobo = new double[3,1];
				for(int i = 0; i < 3; i++)
				{
					posVectorRobo[i, 0] =  roboPos[i];
				}
				alglib.rmatrixgemm (3,1,3,1, outMatrix, 0, 0, 0, posVectorRobo, 0, 0, 0, 0, ref endingRobo, 0, 0);
				//print ("endingRobo: " + endingRobo[0,0]);
				roboPos.x = (float)endingRobo[0, 0];
				roboPos.y = (float)endingRobo[1, 0];
				roboPos.z = (float)endingRobo[2, 0];

				roboPos += diff; //move the robot in the opposite direction as the COM of the points
				//print ("diff: " + diff);
				//print ("roboos: " + roboPos);
				endingRobo= null;
				posVectorRobo = null;
				outMatrix = null;


				if(Vector3.Magnitude(roboPos - startPos) > 5f){
					print ("SLAM robot adjust magnitude was too large");
					Camera.main.GetComponent<RobotRender>().robot.transform.position = posBeforeICP;
					Camera.main.GetComponent<RobotRender>().robot.transform.rotation = rotBeforeICP;
					return false;
				}

				//now robopos has been updated, time to apply!
				if(!float.IsNaN (roboPos.x) && !float.IsNaN(roboPos.y) && !float.IsNaN(roboPos.z)){
					totalRobotMovementDistance += roboPos - startPos;
					GameObject bot = Camera.main.GetComponent<RobotRender>().robot;
					if(bot != null){
						//Camera.main.GetComponent<RobotRender>().enabled = false;
						avgStartPos.z = 0;
						Vector3 beginningArc = odom.position - avgStartPos;
						bot.transform.position += roboPos - startPos;
						odom.position += roboPos - startPos;

						Vector3 endingArc =  odom.position - avgStartPos;
						//endingArc.z = beginningArc.z = 0;
						Vector3 cross = Vector3.Cross(beginningArc, endingArc);
						//print (cross);
						float angle = 0f;
						if(cross.z != 0f){
							angle = Mathf.Asin(cross.magnitude * Mathf.Rad2Deg);
							//print(angle);
							totalAngleTurned += angle * Mathf.Sign (cross.z);
						}

						//print ("Angle: " + angle + " Axis: " + Vector3.Normalize(cross));
						if(!float.IsNaN (angle)){
							//print(angle);
							Camera.main.GetComponent<RobotRender>().robot.transform.RotateAround(Camera.main.GetComponent<RobotRender>().robot.transform.position, cross, -angle);
							//Camera.main.GetComponent<RobotRender>().enabled = false;
						} else {
							//print ("Start:" + beginningArc);
							//print ("End:" + endingArc);
							//print("angle was nan");
							//print (cross);
						}
					}
				} else {
					print ("Detected NaN problems!");
				}

				currMaxRange *= .9f;
			}







		if(distanceBMoved / matched.Count < toleranceForBMovement){
			successFlag += 1;
		} else {
			successFlag = 0;
		}
		if(successFlag > numSuccessesRequired){
			print ("ICP succeeded");
			//print ("robot moved " + totalRobotMovementDistance);
			//this means we might have a situation where it's too late to get the num required, should exit early with false if this happens.
			//print (totalAngleTurned);
			return true;
			}
		}
		print ("ICP timed out. Flag had reached: " + successFlag);
		//sprint ("DistanceBMoved: " + distanceBMoved);
		//Camera.main.GetComponent<RobotRender>().robot.transform.position = posBeforeICP;
		//Camera.main.GetComponent<RobotRender>().robot.transform.rotation = rotBeforeICP;
		return false; //didn't converge in time :( sad times
	}


	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.N)) {
			alignSubset(.3f);
		}
		if (Input.GetKeyDown (KeyCode.M)) {
			if(runICP (true)){
				transferParenthood();
			} else {
				Camera.main.GetComponent<PointRender>().clearChildren (parentB.gameObject);
			}
		}
	}
		
	void FixedUpdate() {
		if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)){
			float rotateDirection = 1;
			if(Input.GetKey (KeyCode.LeftArrow)){
				rotateDirection = -1;
			}
			parentB.Rotate(0, 0, rotateDirection * rotateSpeed * Time.fixedDeltaTime);
		}

		if (Input.GetKey (KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)){
			float moveDirection = 1f;
			if(Input.GetKey (KeyCode.DownArrow)){
				moveDirection = -1f;
			}
			Vector3 pos = parentB.position;
			pos.y += moveDirection * Time.fixedDeltaTime;
			parentB.position = pos;
		}
	}
}
