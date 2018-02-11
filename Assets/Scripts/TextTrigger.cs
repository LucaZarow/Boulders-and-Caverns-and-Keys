using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTrigger : MonoBehaviour {

	public Text text;
	List<string> phrases;
	int keysLeft = 3;
	int time=0;

	//instructions
	void Start () {
		phrases = new List<string> {"You'll fall.","Check the underground caves, but beware of boulders.\nYou can shoot them by left clicking.","Keys left : ", "Locked.", "You hear something unlock.", "It'sthe end!"};
	}

	//don't let text linger
	void Update () {
		time++;
		if (time % 200 == 0) {
			text.text = "";
		}
	}

	//trigger text at certain points on map
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag ("Trigger1")) {//near ravine
			text.text = phrases[0];
			Destroy (other);
			time = 0;
		}
		if (other.gameObject.CompareTag ("Trigger2")) {//instruct about cave
			text.text = phrases[1];
			time = 0;
		}
		if (other.gameObject.CompareTag ("Trigger3")) {//entering cave
			text.text = phrases[2] + keysLeft.ToString();
			keysLeft--;
			Destroy (other);
			time = 0;
		}
		if (other.gameObject.CompareTag ("Key")) {//pick up key
			if (keysLeft > 0) {
				text.text = phrases [2] + keysLeft.ToString ();
				keysLeft--;
				time = 0;
			} else {//all keys picked up
				text.text = phrases [4];
				time = 0;
			}
		}
		if (other.gameObject.CompareTag ("Trigger4")) {//gate still locked
			text.text = phrases[3];
			time = 0;
		}
		if (other.gameObject.CompareTag ("Trigger6")) {//the end
			text.text = phrases[5];
			Destroy (other);
			time = 0;
		}
	}
}
