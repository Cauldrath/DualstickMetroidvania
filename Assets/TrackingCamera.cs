using UnityEngine;
using System.Collections;

public class TrackingCamera : MonoBehaviour {

	public Transform focalPoint;
	public Vector3 cameraPosition;
	public Vector3 sniperOffset;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = focalPoint.position + cameraPosition + sniperOffset;
	}
}
