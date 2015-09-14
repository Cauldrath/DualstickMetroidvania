using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {
	public float speed = 8.0f;
	public float maxJump = 0.25f;
	public float maxDash = 0.25f;
	public float jumpSpeed = 10f;
	public float dashSpeed = 32.0f;
	public int   airJumps = 1;
	public bool  teleportDash = true;
	public bool  teleportSniper = true;
	public float dashTeleportHeightLimit = 0.5f;

	public float shotgunFireRate = 1.0f;
	public int   shotgunShots = 5;
	public float shotgunSpray = 90.0f;

	public float handgunFireRate = 0.5f;
	
	public float sniperFineSensitivity = 5.0f;
	public float sniperSensitivity = 30.0f;
	public float sniperSnapbackTime = 1.0f;
	public float sniperSnaptoTime = 0.1f;
	public float sniperTeleportSnapTime = 0.5f;
	public float sniperTeleportHeightLimit = 10.0f;
	public float sniperFireRate = 1.0f;

	public TrackingCamera attachedCamera;
	public GameObject laserSightLeft;
	public GameObject laserSightRight;
	public GameObject shotgunBullet;
	public GameObject handgunBullet;
	public GameObject sniperBullet;

	Vector2 storedMovement;
	float jump;
	float dash;
	float snapbackTimer;
	int jumpsLeft;
	Rigidbody body;
	int weaponEquipped;
	bool onGround;
	float shotgunCooldown;
	float handgunCooldown;
	float sniperCooldown;

	void setVisible(GameObject obj, bool visible) {
		foreach(Renderer r in obj.GetComponentsInChildren<Renderer>()) {
			r.enabled = visible;
		}
	}

	// Use this for initialization
	void Start () {
		jump = maxJump;
		jumpsLeft = airJumps;
		dash = maxDash;
		onGround = true;
		body = GetComponent<Rigidbody>();
		weaponEquipped = 0;
		shotgunCooldown = 0;
		handgunCooldown = 0;
		sniperCooldown = 0;
	}
	
	// Update is called once per frame
	void Update () {
		float step = Time.deltaTime;
		Vector2 movement2D;
		Vector2 aim2D;
		Vector3 movement = new Vector3(storedMovement.x, body.velocity.y, storedMovement.y);
		movement2D.x = Input.GetAxisRaw ("Horizontal"); 
		movement2D.y = Input.GetAxisRaw ("Vertical");
		Vector2 normalizedMovement = movement2D.normalized;
		aim2D.x = Input.GetAxisRaw ("HorizontalRight");
		aim2D.y = Input.GetAxisRaw ("VerticalRight");
		Vector2 normalizedAim = aim2D.normalized;

		//Left stick moves, right stick aims when using shotgun
		if(weaponEquipped == 0) {
			this.setVisible(laserSightLeft, false);
			if (Input.GetAxisRaw ("Fire3") >= 0.5) {
				weaponEquipped = 1;
				dash = 0;
			}
			if (Input.GetAxisRaw ("Fire2") >= 0.5) {
				weaponEquipped = 2;
				dash = 0;
				jump = 0;
				snapbackTimer = 0;
			}

			Vector2 velocity2D;
			if(dash > 0) {
				velocity2D = storedMovement;
				if(Vector3.Dot(storedMovement.normalized, normalizedMovement) <= 0) {
					dash = 0;
				}
			} else {
				dash = 0;
				velocity2D = movement2D * speed;
			}
			movement.x = velocity2D.x;
			movement.z = velocity2D.y;
			storedMovement = velocity2D;
			if(normalizedAim.magnitude != 0) {
				this.setVisible(laserSightRight, true);
				laserSightRight.transform.rotation = Quaternion.LookRotation(new Vector3(normalizedAim.x, 0, normalizedAim.y)) * Quaternion.Euler(90.0f, 0, 0);
				laserSightRight.transform.position = transform.position + laserSightRight.transform.rotation * (new Vector3(0, 10.0f, 0));
				transform.rotation = Quaternion.LookRotation(new Vector3(normalizedAim.x, 0, normalizedAim.y));
				if(shotgunCooldown <= 0) {
					for(int looping = 0; looping < shotgunShots; ++looping) {
						float angle = ((looping - shotgunShots / 2.0f) * shotgunSpray) / shotgunShots;
						Vector3 shotVector = Quaternion.Euler(0, angle, 0) * new Vector3(normalizedAim.x, 0, normalizedAim.y);
						GameObject Shot = (GameObject)GameObject.Instantiate(shotgunBullet, transform.position + shotVector, transform.rotation);
						(Shot.GetComponent<BulletScript>()).setDirection(shotVector);
						Shot.SetActive(true);
					}
					shotgunCooldown = shotgunFireRate;
				}
			} else {
				this.setVisible(laserSightRight, false);
				if(normalizedMovement.magnitude != 0) {
					transform.rotation = Quaternion.LookRotation(new Vector3(normalizedMovement.x, 0, normalizedMovement.y));
				}
			}
			if (Input.GetButton ("Dash")) {
				if (dash == 0 && Input.GetButtonDown ("Dash") && movement2D.magnitude > 0) {
					if(onGround || jumpsLeft > 0) {
						if(teleportDash) {
							Vector2 teleportOffset = normalizedMovement * dashSpeed * maxDash;
							Vector3 teleportPosition = new Vector3(body.position.x + teleportOffset.x, body.position.y, body.position.z + teleportOffset.y);
							if(this.teleportOnLevel(ref teleportPosition, false, dashTeleportHeightLimit)) {
								body.position = teleportPosition;
							} else {
								//If you can't teleport there, you can still dash
								dash = maxDash;
								storedMovement = normalizedMovement * dashSpeed;
							}
						} else {
							dash = maxDash;
							storedMovement = normalizedMovement * dashSpeed;
						}
						if(!onGround) {
							jumpsLeft--;
						}
					}
				}
				if (dash > 0) {
					dash -= step;
					if(!onGround) {
						movement.y = 0;
					}
				}
			} else {
				dash = 0;
			}
		}

		//Left stick and right stick aim independantly and horizontal velocity is maintained when using pistols
		if (weaponEquipped == 1) {
			movement.x = storedMovement.x;
			movement.z = storedMovement.y;
			if(movement2D.magnitude == 0) {
				this.setVisible(laserSightLeft, false);
				if(aim2D.magnitude != 0) {
					transform.rotation = Quaternion.LookRotation(new Vector3(aim2D.x, 0, aim2D.y));
				}
			} else {
				this.setVisible(laserSightLeft, true);
				laserSightLeft.transform.rotation = Quaternion.LookRotation(new Vector3(normalizedMovement.x, 0, normalizedMovement.y)) * Quaternion.Euler(90.0f, 0, 0);
				laserSightLeft.transform.position = transform.position + laserSightLeft.transform.rotation * (new Vector3(0, 10.0f, 0));

				if(aim2D.magnitude == 0) {
					transform.rotation = Quaternion.LookRotation(new Vector3(movement2D.x, 0, movement2D.y));
				} else {
					Quaternion leftRotation = Quaternion.LookRotation(new Vector3(movement2D.x, 0, movement2D.y));
					Quaternion rightRotation = Quaternion.LookRotation(new Vector3(aim2D.x, 0, aim2D.y));
					float averageAngle = (leftRotation.eulerAngles.y + rightRotation.eulerAngles.y) / 2;
					if(transform.rotation.y - averageAngle > 180) {
						averageAngle += 180;
					}
					if(averageAngle - transform.rotation.y > 180) {
						averageAngle -= 180;
					}
					transform.rotation = Quaternion.AngleAxis(averageAngle, new Vector3(0, 1, 0));
				}
			}

			if(normalizedAim.magnitude != 0) {
				this.setVisible(laserSightRight, true);
				laserSightRight.transform.rotation = Quaternion.LookRotation(new Vector3(normalizedAim.x, 0, normalizedAim.y)) * Quaternion.Euler(90.0f, 0, 0);
				laserSightRight.transform.position = transform.position + laserSightRight.transform.rotation * (new Vector3(0, 10.0f, 0));
			} else {
				this.setVisible(laserSightRight, false);
			}
			if (Input.GetAxisRaw ("Fire2") >= 0.5 && handgunCooldown <= 0) {
				Vector3 shotVector;
				GameObject Shot;

				if(normalizedAim.magnitude != 0) {
					shotVector = new Vector3(normalizedAim.x, 0, normalizedAim.y);
					Shot = (GameObject)GameObject.Instantiate(handgunBullet, transform.position + shotVector, transform.rotation);
					(Shot.GetComponent<BulletScript>()).setDirection(shotVector);
					Shot.SetActive(true);
				}

				if(normalizedMovement.magnitude != 0) {
					shotVector = new Vector3(normalizedMovement.x, 0, normalizedMovement.y);
					Shot = (GameObject)GameObject.Instantiate(handgunBullet, transform.position + shotVector, transform.rotation);
					(Shot.GetComponent<BulletScript>()).setDirection(shotVector);
					Shot.SetActive(true);
				}

				handgunCooldown = handgunFireRate;
			}
			if (Input.GetAxisRaw ("Fire3") < 0.5) {
				weaponEquipped = 0;
			}
		}

		//Only allow jumping when using shotgun or pistols
		if (weaponEquipped == 0 || weaponEquipped == 1) {
			if(snapbackTimer > 0) {
				attachedCamera.sniperOffset = Vector3.Lerp(new Vector3(0, 0, 0), attachedCamera.sniperOffset, snapbackTimer / sniperSnapbackTime);
				snapbackTimer -= step;
				if(snapbackTimer <= 0) {
					snapbackTimer = 0;
					attachedCamera.sniperOffset = new Vector3(0, 0, 0);
				}
			}
			if (Input.GetButton ("Jump")) {
				if (jump == 0 && jumpsLeft > 0 && Input.GetButtonDown ("Jump")) {
					jump = maxJump;
					dash = 0;
					if(!onGround) {
						jumpsLeft--;
					}
				}
				if (jump > 0) {
					movement.y = jumpSpeed;
					jump -= step;
				} else {
					jump = 0;
				}
			} else {
				jump = 0;
			}
		}

		//Left stick is fine aim and right stick is long-distance aim when using sniper rifle
		if (weaponEquipped == 2) {
			movement.x = 0;
			movement.z = 0;
			Vector3 sniperAimOffset = new Vector3(aim2D.x * sniperSensitivity + movement2D.x * sniperFineSensitivity, 0, aim2D.y * sniperSensitivity + movement2D.y * sniperFineSensitivity);
			if(sniperAimOffset.magnitude > 0) {
				transform.rotation = Quaternion.LookRotation(sniperAimOffset);
				if (Input.GetAxisRaw ("Fire3") >= 0.5 && sniperCooldown <= 0) {
					Vector3 shotVector;
					GameObject Shot;
					
					shotVector = sniperAimOffset.normalized;
					Shot = (GameObject)GameObject.Instantiate(sniperBullet, transform.position + shotVector, transform.rotation);
					(Shot.GetComponent<BulletScript>()).setDirection(shotVector);
					Shot.SetActive(true);

					sniperCooldown = sniperFireRate;
				}
			}
			if (Input.GetButtonDown ("Dash") && teleportSniper) {
				Vector3 teleportPosition = body.position + sniperAimOffset;
				if(this.teleportOnLevel(ref teleportPosition, true, sniperTeleportHeightLimit)) {
					body.position = teleportPosition;
				}
				attachedCamera.sniperOffset = new Vector3(0, 0, 0);
				snapbackTimer = sniperTeleportSnapTime;
			}
			if(snapbackTimer > 0) {
				snapbackTimer -= step;
				attachedCamera.sniperOffset = Vector3.Lerp(attachedCamera.sniperOffset, sniperAimOffset, step / sniperTeleportSnapTime);
			} else {
				snapbackTimer = 0;
				attachedCamera.sniperOffset = Vector3.Lerp(attachedCamera.sniperOffset, sniperAimOffset, step / sniperSnaptoTime);
			}
			if (Input.GetAxisRaw ("Fire2") < 0.5) {
				weaponEquipped = 0;
				snapbackTimer = sniperSnapbackTime;
			}
		}

		if (shotgunCooldown > 0) {
			shotgunCooldown -= step;
		}
		if (handgunCooldown > 0) {
			handgunCooldown -= step;
		}
		if (sniperCooldown > 0) {
			sniperCooldown -= step;
		}

		body.velocity = movement;
	}

	bool teleportOnLevel(ref Vector3 teleportLocation, bool teleportDown, float heightLimit) {
		RaycastHit hit;
		if (Physics.Raycast (teleportLocation, -Vector3.up, out hit)) {
			if (teleportDown) {
				teleportLocation = new Vector3 (teleportLocation.x, hit.point.y + 1.0f, teleportLocation.z);
			}
		} else {
			//You're under the map.  Cast a ray upwards.  If it hits something, put yourself on top of it
			if (Physics.Raycast (teleportLocation, Vector3.up, out hit)) {
				if(hit.distance > heightLimit) {
					return false;
				}
				teleportLocation = new Vector3 (teleportLocation.x, hit.point.y + 1.0f, teleportLocation.z);
			} else {
				return false;
			}
		}
		return true;
	}

	void OnCollisionEnter(Collision collision) {
		Vector3 normal = collision.contacts [0].normal;
		if (normal.y > normal.x && normal.y > normal.z) {
			if(!onGround) {
				dash = 0;
			}
			onGround = true;
			jump = 0;
			jumpsLeft = airJumps;
		}
	}
	void OnCollisionExit(Collision collision) {
		if (body.velocity.y > 0) {
			onGround = false;
		}
		if (dash > 0) {
			onGround = false;
			dash = 0;
		}
	}
}
