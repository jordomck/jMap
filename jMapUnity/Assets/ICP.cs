using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ICP : MonoBehaviour {
	public Transform parentA, parentB;
	public float maxLengthPermissible, rotateSpeed;
	// Use this for initialization
	void Start () {
	}

	Dictionary<Transform, Transform> matchPoints(){
		Dictionary<Transform, Transform> dictMatched = new Dictionary<Transform, Transform> ();
		print ("matching points now!");
		float totalDistance = 0;
		foreach (Transform child in parentB) {
			if(Random.Range(0, 25) == 0)
				continue;
			float minLength = maxLengthPermissible;
			Transform bestTransform = child; //if it's still child at the end, we couldn't find a close enough one
			foreach (Transform match in parentA) {
				Vector3 offset = match.position - child.position;
				if (Vector3.Magnitude (offset) < minLength) {
					minLength = Vector3.Magnitude (offset);
					bestTransform = match;
					totalDistance += Vector3.Magnitude (offset);
				}


			}
			if (bestTransform != child) {
				dictMatched.Add (child, bestTransform);
				//Debug.DrawLine (child.position + Vector3.back, bestTransform.position + Vector3.back, Color.white, 4f);
			} 
		}
		print (totalDistance);
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

	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.M)) {
			for (int time = 0; time < 1; time++) {
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
				//print ("avg1: " + avg1.ToString());
				//print ("avg2: " + avg2.ToString());
				Vector3 diff = avg1 - avg2;
				//print ("diff " + diff.ToString());


				//create H matrix
				double[,] hMatrix = getHMatrix (matched);
				//print3x3(hMatrix);

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
			{
				W2D[i,i] = W[i];
			}

			//transpose the new 2D version of W
			double[,] Wt = new double[3,3];
			alglib.rmatrixtranspose(3, 3, W2D, 0, 0, ref Wt, 0, 0);
			*/

				double[,] Ut = new double[3, 3];
				Ut.Initialize ();
				alglib.rmatrixtranspose (3, 3, U, 0, 0, ref Ut, 0, 0);
				//print3x3(Ut);
				//multiply 
				double[,] outMatrix = new double[3, 3];
				outMatrix.Initialize ();
				alglib.rmatrixgemm (3, 3, 3, 1, V, 0, 0, 0, Ut, 0, 0, 0, 0, ref outMatrix, 0, 0);
				/*
			for(int i = 0; i < 3; i++){
				for(int j = 0; j < 3; j++)
					print (outMatrix[i, j]);
			}
			*/
				double det = alglib.rmatrixdet (outMatrix);
				//print("Determinant: " + det.ToString());

				//did this work at all?
				if (det < 0) {
					print ("ICP failed :(");
					return;
				}
				//print3x3(outMatrix);
				foreach (Transform trans in parentB) {
					Vector3 newPos = trans.position;
					double[,] ending = new double[3, 1];
					double[,] posVector = new double[3, 1];
					for (int i = 0; i < 3; i++) {
						posVector [i, 0] = newPos [i];
					}
					alglib.rmatrixgemm (3, 1, 3, 1, outMatrix, 0, 0, 0, posVector, 0, 0, 0, 0, ref ending, 0, 0); 
					//print (pair.Key.transform.position.x - ending[0,0]);
					//print (pair.Key.transform.position.y - ending[1,0]);
					//print (ending[2,0]);
					newPos.x = (float)ending [0, 0];
					newPos.y = (float)ending [1, 0];
					newPos.z = (float)ending [2, 0];

					newPos -= diff;
					trans.position = newPos;
				}

		
			}
		}
	}

	void FixedUpdate() {
		if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)){
			float rotateDirection = 1;
			if(Input.GetKey (KeyCode.LeftArrow)){
				rotateDirection = -1;
			}
			Transform odom = parentB.GetComponent<odomStorer>().cachedOdom;
			parentB.Rotate(0, 0, rotateDirection * rotateSpeed * Time.fixedDeltaTime);
		}
	}
}
