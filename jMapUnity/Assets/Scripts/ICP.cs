using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ICP : MonoBehaviour {
	public Transform parentA, parentB;
	public int iterationsUntilFailure, numSuccessesRequired;
	public float toleranceForBMovement;
	public int pairDownsample;
	public float convergeDist;
	public float maxLengthPermissible, rotateSpeed;
	public float prevTotalDist, totalDistance;
	int successFlag;
	// Use this for initialization
	void Start () {
		InvokeRepeating ("tidyWalls", 0f, .2f);
		successFlag = 0;
	}

	Dictionary<Transform, Transform> matchPoints(){
		Dictionary<Transform, Transform> dictMatched = new Dictionary<Transform, Transform> ();
		print ("matching points now!");
		totalDistance = 0;
		foreach (Transform child in parentB) {
			if(Random.Range(0, pairDownsample) != 0)
				continue;
			Collider[] foundTargets = Physics.OverlapSphere(child.transform.position, maxLengthPermissible);
			float minLength = maxLengthPermissible;
			Transform bestTransform = child; //if it's still child at the end, we couldn't find a close enough one

			foreach (Collider matchColl in foundTargets) {
				if(matchColl.gameObject == child.gameObject)
					continue;
				if(matchColl.transform.parent == parentB)
					continue;
				Transform match = matchColl.gameObject.transform;
				Vector3 offset = match.position - child.position;
				if (Vector3.SqrMagnitude (offset) < minLength) {
					minLength = Vector3.SqrMagnitude (offset);
					bestTransform = match;
					totalDistance += Vector3.SqrMagnitude (offset);
				}


			}
			if (bestTransform != child) {
				dictMatched.Add (child, bestTransform);
				Debug.DrawLine (child.position + Vector3.back, bestTransform.position + Vector3.back, Color.white, .7f);
			} 
		}
		//print (totalDistance);
		prevTotalDist = totalDistance;
		return dictMatched;
	}

	double[,] getHMatrix(Dictionary<Transform, Transform> matched){
		double[,] hMatrix = new double[3,3];
		hMatrix.Initialize();
		foreach(KeyValuePair<Transform, Transform> pair in matched){
			Vector3 a = pair.Key.position;
			Vector3 b = pair.Value.position;
			for(int i = 0; i < 3; i++){
				for(int j = 0; j < 3; j++){
					hMatrix[i,j] += (double)a[i] * (double)b[j];
				}
			}
		}
		return hMatrix;
	}

	public void print3x3(double[,] matrix){
		for (int i = 0; i < 3; i++) {
			print (matrix[i, 0].ToString() + " " + matrix[i, 1].ToString() + " " + matrix[i, 2].ToString());
		}
	}

	public void transferParenthood(){
		while(parentB.childCount > 0){
			foreach(Transform child in parentB){
					child.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
					child.parent = parentA;
				//if(Physics.OverlapSphere(child.position, .01f).Length > 5){
				//	Destroy (child.gameObject);
				//}
			}
		}
	}

	public void tidyWalls(){
		foreach (Transform child in parentA) {
			if (Physics.OverlapSphere (child.position, .04f).Length > 8 && Random.value < .45f) {
				//print ("DESTROYED");
				child.gameObject.SetActive (false);
				Destroy (child.gameObject);
			}
		}
	}
	

	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.N)) {
			transferParenthood();
		}
		if (Input.GetKeyDown (KeyCode.M)) {
			runICP ();
		}
	}

	public bool runICP(){
		successFlag = 0;
		for (int time = 0; time < iterationsUntilFailure; time++) {
			Dictionary<Transform, Transform> matched = matchPoints ();
			
			//calculate averages for zero-centering
			Vector3 avg1, avg2;
			avg1 = avg2 = Vector3.zero;
			foreach (KeyValuePair<Transform, Transform> pair in matched) {
				avg1 += pair.Key.position;
				avg2 += pair.Value.position;
			}
			avg1 = avg1 / matched.Count;
			avg2 = avg2 / matched.Count;
			Vector3 diff = avg1 - avg2;
			
			
			
			//create H matrix
			double[,] hMatrix = getHMatrix (matched);
			
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
			
			/*
			//extrapolate W into a 2D matrix
			double[,] W2D = new double[3,3];
			for(int i = 0; i < 3; i++)
			{}
				W2D[i,i] = W[i];
			}

			//transpose the new 2D version of W
			double[,] Wt = new double[3,3];
			alglib.rmatrixtranspose(3, 3, W2D, 0, 0, ref Wt, 0, 0);
			*/
			
			double[,] Ut = new double[3, 3];
			Ut.Initialize ();
			alglib.rmatrixtranspose (3, 3, U, 0, 0, ref Ut, 0, 0);
			double[,] outMatrix = new double[3, 3];
			outMatrix.Initialize ();
			alglib.rmatrixgemm (3, 3, 3, 1, V, 0, 0, 0, Ut, 0, 0, 0, 0, ref outMatrix, 0, 0);
			double det = alglib.rmatrixdet (outMatrix);
			
			//did this work at all? 
			
			if (det < 0) {
				print ("ICP failed :(");
				/*
					foreach (KeyValuePair<Transform, Transform> pair in matched) {
						avg1 -= pair.Key.position;
						avg2 -= pair.Value.position;
					}
					*/
				return false;
			}

			float distanceBMoved = 0f;
			foreach (Transform trans in parentB) {
				Vector3 newPos = trans.position;
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
					distanceBMoved += Vector3.SqrMagnitude(trans.position - newPos);
					trans.position = newPos;
				}
				//print ("DISTANCE MOVED: " + distanceBMoved);

			}
		if(distanceBMoved < toleranceForBMovement){
			successFlag += 1;
		} else {
			successFlag = 0;
		}
		//print ("TIME: " + time + " FLAG: " + successFlag);
		if(successFlag > numSuccessesRequired){
			print ("ICP succeeded after " + time + " iterations!");
			//this means we might have a situation where it's too late to get the num required, should exit early with false if this happens.
			return true;
			}
		}
		print ("ICP timed out. Flag had reached: " + successFlag);
		return false; //didn't converge in time :( sad times
	}
		
		void FixedUpdate() {
		if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)){
			float rotateDirection = 1;
			if(Input.GetKey (KeyCode.LeftArrow)){
				rotateDirection = -1;
			}
			parentB.Rotate(0, 0, rotateDirection * rotateSpeed * Time.fixedDeltaTime);
		}
	}
}
