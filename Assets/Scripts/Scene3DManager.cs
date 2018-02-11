using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene3DManager : MonoBehaviour {

	//die on contact
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag ("Boulder")) {
			SceneManager.LoadScene("Game Over");
		}
	}
}
