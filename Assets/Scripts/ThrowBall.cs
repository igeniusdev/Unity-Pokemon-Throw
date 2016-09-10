using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThrowBall : MonoBehaviour
{

	#region Private_Variables
	private static ThrowBall throwPaperInstance;
	private float factor = 230.0f;
	private float startTime;
	public Vector3 startPos;
	private GameObject currentBall;
	private bool isGettingDirection = false;
	private bool isGameStart = true;
	public bool isCurveThrow = false;
	private Vector3 lastPos;
	private Vector3 lastPokeBallPosition = Vector3.zero;
	private bool isStartRoate = false;
	List<Vector3> ballPos = new List<Vector3> ();
	List<float> ballTime = new List<float> ();

	// Use this for initialization
	float lastAngel = 0f;
	public int angleDirection = 0;
	float rotationSpeed = 2f;
	float totalX = 0f, totalY = 0f;
	int dLeft = 0, dRight = 0;
	#endregion
	
	#region Public_Variables
	
	public Vector3 minThrow;
	public Vector3 maxThrow;
	public GameObject ball;
	private Transform currentBallChild;
	private float dist;
	private Vector3 v3Offset;
	private Plane plane;
	private bool ObjectMouseDown = false;
	public GameObject linkedObject;
	public Transform target;
	public ParticleSystem curveParticle;
	#endregion
	
	#region Unity_Callbacks
	void Awake ()
	{
		throwPaperInstance = this;
	}
	
	void Start ()
	{
		StartCoroutine (GetBallCoroutine ());
	}
	
	void Update ()
	{

		if (currentBall) {
			transform.position = currentBall.transform.position;
		}

		if (currentBall && isGettingDirection) {
			//Debug.Log ("M:" + Input.mousePosition);

			transform.position = currentBall.transform.position;

			float angle = 0f;

			if (currentBallChild != null) {
				Vector3 mouse_pos = Input.mousePosition;
				Vector3 player_pos = Camera.main.WorldToScreenPoint (currentBallChild.localPosition);
				
				mouse_pos.x = mouse_pos.x - player_pos.x;
				mouse_pos.y = mouse_pos.y - player_pos.y;
				
				angle = Mathf.Atan2 (mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;

			}
				
			if (Vector3.Distance (currentBall.transform.position, lastPokeBallPosition) > 0f) {

				if (!isStartRoate) {
					Invoke ("ResetPoint", 0.01f);
				} else {
					Vector3 dir = (currentBall.transform.position - lastPokeBallPosition);
					
					if (dir.x > dir.y) {
						totalX += 0.12f;
					} else {
						if (dir.x != dir.y) {
							totalY += 0.12f;
						}
					}
				
					if (totalX >= 2 && totalY >= 2) {
						isCurveThrow = true;
						curveParticle.gameObject.SetActive (true);
						if (curveParticle.isStopped) {
							curveParticle.Play ();
						}

						curveParticle.transform.position = currentBall.transform.position;
					}
				}

				isStartRoate = true;
				if (angle > 0) {
					if (lastAngel >= angle) {
						dRight++;
					} else {
						dLeft++;
					}
				} else {
					if (lastAngel >= angle) {
						dRight++;
					} else {
						dLeft++;
					}
				}

				if (dLeft < dRight) {
					angleDirection = 1;
					if (currentBallChild != null)
						currentBallChild.transform.Rotate (new Vector3 (0, 0, 40f));
				} else {
					angleDirection = -1;
					if (currentBallChild != null)
						currentBallChild.transform.Rotate (new Vector3 (0, 0, -40f));
				}

			} else {

				if (isStartRoate == true) {
					StartCoroutine (StopRotation (angleDirection, 0.5f));
				}
			}

			lastAngel = angle;
			lastPokeBallPosition = currentBall.transform.position;
		}

	}

	private void ResetPoint ()
	{

		if (!isStartRoate) {
			totalX = 0f;
			totalY = 0f;
			dLeft = 0;
			dRight = 0;
			isCurveThrow = false;

			startPos = currentBall.transform.position;


			ballPos.Clear ();
			
			ballTime.Clear ();
			ballTime.Add (Time.time);
			
			ballPos.Add (currentBall.transform.position);

			curveParticle.Stop ();
			curveParticle.gameObject.SetActive (false);

			ThrowBall.Instance.isCurveThrow = false;
		}
	}

	IEnumerator rotate (Quaternion from, Quaternion to, float t)
	{
		float rate = 1.0f / t;
		float i = 0f;
		while (i<1.0f) {
			i += rate * Time.deltaTime;
			currentBallChild.localRotation = Quaternion.Lerp (from, to, i);
			yield return 0;
		}
	}

	IEnumerator StopRotation (int direction, float t)
	{

		isStartRoate = false;

	
		float rate = 1.0f / t;
		float i = 0f;
		while (i<1.0f) {
			i += rate * Time.deltaTime;
			if (!isStartRoate) {
				if (direction == 1) {
					if (currentBallChild != null)
						currentBallChild.transform.Rotate (new Vector3 (0, 0, 40f) * (Mathf.Lerp (rotationSpeed * totalX, 0, i)) * Time.deltaTime);
				} else {
					if (currentBallChild != null)
						currentBallChild.transform.Rotate (new Vector3 (0, 0, -40f) * (Mathf.Lerp (rotationSpeed * totalX, 0, i)) * Time.deltaTime);
				}
			}

			yield return 0;
		}

		if (!isStartRoate) {
			
			totalX = 0f;
			totalY = 0f;
			dLeft = 0;
			dRight = 0;
			isCurveThrow = false;
			rotationSpeed = 100f;

			curveParticle.Stop ();
			curveParticle.gameObject.SetActive (false);
		}
	}
	
	void OnMouseDown ()
	{
		if (currentBall == null)
			return;
		plane.SetNormalAndPosition (Camera.main.transform.forward, currentBall.transform.position);
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		float dist;
		plane.Raycast (ray, out dist);
		v3Offset = currentBall.transform.position - ray.GetPoint (dist);     
		ObjectMouseDown = true;



		if (!currentBall || !isGameStart)
			return;


		ballPos.Clear ();

		ballTime.Clear ();
		ballTime.Add (Time.time);

		ballPos.Add (currentBall.transform.position);
		
		totalX = 0f;
		totalY = 0f;
		
		dLeft = 0;
		dRight = 0;
		
		isCurveThrow = false;
		
		isGettingDirection = true;
		currentBall.SendMessage ("SetIsThrowed", true, SendMessageOptions.RequireReceiver);
		
		currentBallChild = currentBall.transform.FindChild ("Ball");


		StartCoroutine (GettingDirection ());
	
	}

	IEnumerator GettingDirection ()
	{
		while (isGettingDirection) {
			startTime = Time.time;
			lastPos = startPos;
			startPos = Camera.main.WorldToScreenPoint (currentBall.transform.position);
			startPos.z = currentBall.transform.position.z - Camera.main.transform.position.z;
			startPos = Camera.main.ScreenToWorldPoint (startPos);

			yield return new WaitForSeconds (0.01f);
			
		}
	}

	//drag pokeball
	void OnMouseDrag ()
	{
		if (ObjectMouseDown == true) {

			if (!currentBall)
				return;

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			float dist;
			plane.Raycast (ray, out dist);
			Vector3 v3Pos = ray.GetPoint (dist);
			v3Pos.z = currentBall.transform.position.z;
			v3Offset.z = 0;
			currentBall.transform.position = v3Pos + v3Offset;


			if (ballPos.Count > 0) {
			
				
				if (ballPos.Count <= 4) {
					if (Vector3.Distance (currentBall.transform.position, ballPos [ballPos.Count - 1]) >= 0.01f) {
						ballTime.Add (Time.time);
						ballPos.Add (currentBall.transform.position);
						
					}
				} else {
					if (Vector3.Distance (currentBall.transform.position, ballPos [ballPos.Count - 1]) >= 0.01f) {
						ballTime.RemoveAt (0);
						ballPos.RemoveAt (0);
						ballTime.Add (Time.time);
						ballPos.Add (currentBall.transform.position);
					}
				}
				
			} else {
				ballPos.Add (currentBall.transform.position);
			}

			if (linkedObject != null) { 
				linkedObject.transform.position = v3Pos + v3Offset;
			}
		}
	}
	
	void OnMouseOut ()
	{
		ObjectMouseDown = false;
	}

	//Throw pokeball when mouse up from ball

	void OnMouseUp ()
	{
		curveParticle.Stop ();
		curveParticle.gameObject.SetActive (false);
		isGettingDirection = false;

		if (!currentBall || !isGameStart)
			return;

		var endPos = Input.mousePosition;
		endPos.z = currentBall.transform.position.z - Camera.main.transform.position.z;
		endPos = Camera.main.ScreenToWorldPoint (endPos);

		int ballPositionIndex = ballPos.Count - 2;

		if (ballPositionIndex < 0)
			ballPositionIndex = 0;

		Vector3 force = currentBall.transform.position - ballPos [ballPositionIndex];
		
		if (Vector3.Distance (lastPos, startPos) <= 0.0f) {
			currentBall.SendMessage ("ResetBall", SendMessageOptions.RequireReceiver);
			return;
		}
		
		//if downside
		if (currentBall.transform.position.y <= ballPos [ballPositionIndex].y) {
			currentBall.SendMessage ("ResetBall", SendMessageOptions.RequireReceiver);

			return;
		}

		//if not swipe
		if (force.magnitude < 0.02f) {
			currentBall.SendMessage ("ResetBall", SendMessageOptions.RequireReceiver);
			return;
		}

		force.z = force.magnitude;
		force /= (Time.time - ballTime [ballPositionIndex]);
		force.y /= 2f;
		force.x /= 2f;
	
		force.x = Mathf.Clamp (force.x, minThrow.x, maxThrow.x);
		force.y = Mathf.Clamp (force.y, minThrow.y, maxThrow.y);
		force.z = Mathf.Clamp (force.z, minThrow.z, maxThrow.z);

		//send message ball was thrown
		currentBall.SendMessage ("SetIsThrowed", true, SendMessageOptions.RequireReceiver);
		

		if (isCurveThrow) {
			force.z -= 0.1f;
			if (angleDirection == 1) {
				if (force.z < 2.3f)
					force.z = 2.3f;
				currentBall.SendMessage ("SetCurve", -0.2);
			} else {
				if (force.z < 2.3f)
					force.z = 2.3f;

				currentBall.SendMessage ("SetCurve", 0.2);
			}
		} 

		//get rigidbody
		Rigidbody ballRigidbody = currentBall.GetComponent<Rigidbody> ();
		//enable collider
		currentBall.GetComponent<Collider> ().enabled = true;
		ballRigidbody.useGravity = true;

		ballRigidbody.AddForce (force * factor);

		StartCoroutine (GetBallCoroutine ());
		
		currentBall = null;
		
	}




	#endregion
	
	#region Private_Methods
	#endregion
	
	#region Public_Methods

	/// <summary>
	/// Gets the new ball.
	/// </summary>
	public void GetNewBall ()
	{
		if (currentBall != null)
			Destroy (currentBall);
		currentBall = Instantiate (ball, ball.transform.position, Quaternion.identity) as GameObject;
		//disable collider
		currentBall.GetComponent<Collider> ().enabled = false;
	}
	#endregion
	
	#region Properties
	/// <summary>
	/// Gets the instance.
	/// </summary>
	/// <value>The instance.</value>
	public static ThrowBall Instance {
		get {
			return throwPaperInstance;
		}
	}
	
	/// <summary>
	/// Gets or sets a value indicating whether this instance is getting direction.
	/// </summary>
	/// <value><c>true</c> if this instance is getting direction; otherwise, <c>false</c>.</value>
	public bool IsGettingDirection {
		get {
			return isGettingDirection;
		}set {
			isGettingDirection = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this instance is game start.
	/// </summary>
	/// <value><c>true</c> if this instance is game start; otherwise, <c>false</c>.</value>
	public bool IsGameStart {
		get {
			return isGameStart;
		}set {
			isGameStart = value;
		}
	}


	#endregion
	
	#region Coroutine

	/// <summary>
	/// Gets the ball coroutine.
	/// </summary>
	/// <returns>The ball coroutine.</returns>
	IEnumerator GetBallCoroutine ()
	{
		yield return new WaitForSeconds (2.0f);

		if (currentBall != null)
			Destroy (currentBall);

		currentBall = Instantiate (ball, ball.transform.position, Quaternion.identity) as GameObject;
		//disable collider
		currentBall.GetComponent<Collider> ().enabled = false;

	}
	#endregion
	
	#region RPC
	#endregion
}
