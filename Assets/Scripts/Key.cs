using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour {

	Rigidbody rigidBody;
	KeyManager keyManager;
	Vector3 position;
	int i=0;

	void Start () {
		rigidBody = GetComponent<Rigidbody> ();
		keyManager = GameObject.Find ("KeyManager").GetComponent<KeyManager> ();
	}

	//move key's rigidbody to correct position
	void FixedUpdate () {
		if (i == 0) {
			if (!position.Equals(new Vector3(0,0,0))) {
				rigidBody.MovePosition (position);
				i++;
			}
		}
	}

	//when player picks up key, tell KeyManager so
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag ("Player")) {
			keyManager.GetKey();
			Destroy (gameObject);
		}
	}

	//KeyManager sets this
	public void SetPosition(Vector3 newPos){
		position = newPos;
	}
}
