using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidBlending : MonoBehaviour {

	private Collider [] neighbors = new Collider [200];
	// this must be large enough

	void FixedUpdate () {

		Vector3 globalDirection = Vector3.zero;

		int count = Physics.OverlapSphereNonAlloc (transform.position, BoidShared.BoidFOW, neighbors);

		foreach (BoidComponent bc in GetComponents<BoidComponent> ()) {
			globalDirection += bc.GetDirection (neighbors, count);
		}

		if (globalDirection != Vector3.zero) {
			transform.rotation = Quaternion.LookRotation ((globalDirection.normalized + transform.forward) / 2f);
		}

		transform.position += transform.forward * BoidShared.BoidSpeed * Time.deltaTime;
	}
}