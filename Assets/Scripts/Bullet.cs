using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Bullet : MonoBehaviour {

	int time = 0;
	FirstPersonController fpc;//keeps track of number of bullets in flight

	void Start () {
		fpc = GameObject.Find ("FPSController").GetComponent<FirstPersonController>();
	}

	//max time for bullet to stay active
	void FixedUpdate () {
		time++;
		if (time > 100) {
			fpc.BulletGone ();
			Destroy (gameObject);
		}
	}
		
	void OnTriggerEnter(Collider other) {
		//desstroy boulder and self on contact
		if (other.gameObject.CompareTag ("Boulder")) {
			Destroy (other.gameObject);
			fpc.BulletGone ();
			Destroy (gameObject);
		}

		//destroy self on contact
		if (other.gameObject.CompareTag ("Wall")) {
		  	fpc.BulletGone ();
			Destroy (gameObject);
		}
	}
}
