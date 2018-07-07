using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoToBehaviour : GeneralBehaviour {

	override public void ExecuteBehaviour(GameObject target)
    {
		if (Vector3.Distance (transform.position, target.transform.position) > 0.5f)
			return; //AddFOrce towards the target
        else transform.LookAt(target.transform.position);
    }
}
