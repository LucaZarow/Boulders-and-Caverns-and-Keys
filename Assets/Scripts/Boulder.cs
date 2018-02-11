using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour {

	Rigidbody rigidBody;
	List<Vector3> path;
	int i=0;
	int time = 0;

	public Boulder(List<Vector3> newPath){
		path = newPath;

	}

	void Start () {
		rigidBody = GetComponent<Rigidbody> ();
	}

	//the Boulder moves along afixed path given by BoulderManager
	void FixedUpdate () {
		if (path != null) {
			if(time % 4 == 0){
				if (i < path.Count-50) {
					rigidBody.MovePosition (path [i]);
					i++;
				} else {
					Destroy(gameObject); //destroy at end
				}
			}
		}
		time++;
	}

	//BoulderManager sets the path
	public void SetPath(List<Vector3> newPath){
		path = newPath;
	}
}
