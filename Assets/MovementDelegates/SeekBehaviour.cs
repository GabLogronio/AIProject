using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekBehaviour : MovementBehaviour {

	public Vector3 destination;

	public float gas = 3f;
	public float steer = 30f;
	public float brake = 20f;

	public float brakeAt = 5f;
	public float stopAt = 0.01f;

	public override Vector3 GetAcceleration (MovementStatus status) {
		if (destination != Vector3.zero) {
			Vector3 verticalAdj = new Vector3 (destination.x, transform.position.y, destination.z);
			Vector3 toDestination = (verticalAdj - transform.position);

			if (toDestination.magnitude > stopAt) {
				Vector3 tangentComponent = Vector3.Project (toDestination.normalized, status.movementDirection);
				Vector3 normalComponent = (toDestination.normalized - tangentComponent);
				return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
			} else {
				return Vector3.zero;
			}
		} else {
			return Vector3.zero;
		}
	}

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }
}
