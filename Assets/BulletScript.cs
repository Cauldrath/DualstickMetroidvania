using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour {
	public float speed = 20.0f;
	public float life = 0.5f;

	float lifeRemaining;
	Vector3 velocity = new Vector3(0, 0, 0);

	void Start () {
		lifeRemaining = life;
	}
	
	void Update () {
		float step = Time.deltaTime;
		lifeRemaining -= step;

		transform.position += velocity * step;
		if (lifeRemaining <= 0) {
			GameObject.Destroy(gameObject);
		}
	}

	public void setDirection(Vector3 direction) {
		velocity = direction * speed;
	}
}
