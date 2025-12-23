using UnityEngine;
using System.Collections;

public class RotatefAround : MonoBehaviour {

    [Tooltip("This is the object that the script's game object will rotate around")]
	public Transform target; 
    [Tooltip("This is the speed at which the object rotates")]
	public int speed; 
	
	void Start() {
		if (target == null) 
		{
			target = this.gameObject.transform;
			Debug.Log ("RotateAround target not specified. Defaulting to this GameObject");
		}
	}

	void Update () {
		transform.RotateAround(target.transform.position,target.transform.up,speed * Time.deltaTime);
	}
}
