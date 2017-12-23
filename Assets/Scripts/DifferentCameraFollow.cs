using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifferentCameraFollow : MonoBehaviour {

	[SerializeField]
	private GameObject target;

	[SerializeField]
	private float lerp = 0.2f;


	void FixedUpdate () {
		Debug.Assert (target != null);
		Vector3 position = Vector3.Lerp (transform.position, target.transform.position, lerp);
		position.z = -20;
		transform.position = position;
	}
}
