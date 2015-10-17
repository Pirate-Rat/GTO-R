using UnityEngine;
using System.Collections;

public class FreeLook : MonoBehaviour {
	
	public float speed = 10;
	private GameObject player;
	
	public float yMinLimit	= -40; 
	public float yMaxLimit	= 80;
	public float xSpeed		= 125.0f;
	public float ySpeed		= 125.0f; 

	private float x = 0.0f;
	private float y = 0.0f;

	void Start () {
		player = (GameObject)this.gameObject;
		Vector3 angles = transform.eulerAngles; 
		x = angles.y; 
		y = angles.x; 
	} 

	void Update () {
		if (Input.GetKey(KeyCode.W)) {
			player.transform.position += player.transform.forward * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S)) {
			player.transform.position -= player.transform.forward * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.A)) {
			player.transform.position -= player.transform.right * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.D)) {
			player.transform.position += player.transform.right * speed * Time.deltaTime;
		}

		x += Input.GetAxis("Mouse X") * xSpeed * 0.02f; 
		y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f; 
		
		y = ClampAngle(y,yMinLimit, yMaxLimit);
		Quaternion rotation = Quaternion.Euler(y, x, 0); 
		transform.rotation = rotation; 

	}
	public static float ClampAngle (float angle, float min, float max) { 
		if(angle < -360) 
			angle += 360; 
		if(angle > 360) 
			angle -= 360; 
		return Mathf.Clamp (angle, min, max); 
	} 
}
