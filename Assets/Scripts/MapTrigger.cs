using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTrigger : MonoBehaviour {

	//create map when player passes this, cause rondomization is time based
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag ("Player")) {
			GetComponentInParent<MapGenerator>().GenerateMap();
			Destroy (gameObject);
		}
	}
}
