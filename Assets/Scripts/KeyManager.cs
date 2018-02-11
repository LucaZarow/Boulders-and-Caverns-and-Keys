using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour {

	public Key template;

	List<Vector3> keyPos;
	GameObject gate;

	//points so far
	int victory = 0;

	//find the blocked exit
	void Start () {
		gate = GameObject.Find ("Gate");
	}

	//unlock when have enough points
	void FixedUpdate () {
		if (victory == 3) {
			Destroy (gate);
		}
	}

	//place keys
	public void SetManagerPosition(List<Vector3> newKeyPos){
		keyPos = newKeyPos;
		foreach (Vector3 v in keyPos) {
			template.SetPosition (v);
			Key key = Instantiate (template, v, new Quaternion ());
			key.SetPosition (v);
		}
	}

	//increase points
	public void GetKey(){
		victory++;
	}
}
