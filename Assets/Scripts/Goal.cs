using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour {

	//goes to win scene
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag ("Goal")) {
			SceneManager.LoadScene("Win");
		}
	}
}
