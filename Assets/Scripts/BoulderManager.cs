using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderManager : MonoBehaviour {

	public Boulder template;

	List<Vector3> path;
	int time = 0;

	//creates a boulder a the beginning of the path in a fixed time interval
	void FixedUpdate () {
		if (path != null) {
			
			if(time % 200 == 0){
				
				Boulder boulder = (Boulder) Instantiate (template, new Vector3(0,-100,0),new Quaternion());
				boulder.SetPath (path);
			}
		}
		time++;

	}

	//set by MapGenerator
	public void SetManagerPath(List<Vector3> newPath){
		path = newPath;
		template.SetPath (path);
	}
}
