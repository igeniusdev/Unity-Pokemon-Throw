using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{

	#region Private_Variables
	// Use this for initialization
	private bool isInFloor = false;
	private Rigidbody rigid;
	private bool isThrowed = false;
	private Vector3 initialPosition;
	private Quaternion initialRotation;
	public Vector3 initialScale;
	private IEnumerator jumpCoroutine;
	private bool isRotate = false;
	private Transform childBall;
	private bool isCurve = false;
	private float curveFactore = 0f;
	private float throwPos = 0f;
	private float windFactor = 1f;
	private bool isWind = false;
	private bool isNormalThrow = false;
	private bool isRotateLeft = true;
	private bool isParticleFollowBall = false;
	public Camera cam;
	public float objectScale = 1.0f; 

	#endregion
	
	#region Public_Variables
	#endregion
	
	#region Unity_Callbacks
	
	void Start ()
	{
		rigid = GetComponent<Rigidbody> ();
		//initalize gradually jump coroutine
		jumpCoroutine = GraduallyJumpCoroutine (0.3f);
		
		childBall = transform.FindChild ("Ball");
		
		StartCoroutine (jumpCoroutine);


		// record initial scale, use this as a basis
		initialScale = transform.localScale; 
		
		// if no specific camera, grab the default camera
		if (cam == null)
			cam = Camera.main; 

	}
	
	void Update ()
	{

		//set Scale as per plane distance
		if (!isThrowed) {
			Plane plane = new Plane (cam.transform.forward, cam.transform.position); 
			float dist = plane.GetDistanceToPoint (transform.position); 
			transform.localScale = initialScale * dist * objectScale; 
		}

		//Rotate Ball When Dragg
		if (isRotate) {
			if (isRotateLeft)
				childBall.Rotate (Vector3.right * Time.deltaTime * 660);
			else
				childBall.Rotate (-Vector3.right * Time.deltaTime * 660);
		}

	}
	
	void FixedUpdate ()
	{

		//apply wind after some distance ball gone
		float dist = 0f;
		if (ThrowBall.Instance.target) {
			dist = (ThrowBall.Instance.target.position.z - transform.position.z);
		}

		//if throw is cureve then and then only apply wind
		if (isCurve && dist <= (throwPos - (throwPos / 9.5f))) {
			if (!isWind)
				StartCoroutine (wind (0.5f));
			transform.Translate (Vector3.right * -curveFactore * windFactor * Time.deltaTime);
		}
	}
	
	void OnEnable ()
	{
		//set default position when object enable
		initialPosition = transform.position;
	}


	void OnCollisionEnter (Collision colliderInfo)
	{

		//if object will not hit directly to target
		if (isInFloor) {
			if (colliderInfo.gameObject.CompareTag ("target")) {
				isCurve = false;
			}

		}

		//if object hit directly to taget
		if (!isInFloor) {

			if (colliderInfo.gameObject.CompareTag ("floor")) {
				isInFloor = true;
				StartCoroutine (stop ());
			} 

			//if hit target then got pokemon
			if (colliderInfo.gameObject.CompareTag ("target")) {

				isInFloor = true;

				colliderInfo.gameObject.SetActive (false);

				StartCoroutine (GotPokemonCoroutine (colliderInfo.gameObject));

			}

		}
	}

	#endregion
	
	#region Private_Methods
	/// <summary>
	/// Sets the is throwed.
	/// </summary>
	/// <param name="flag">If set to <c>true</c> flag.</param>
	private void SetIsThrowed (bool flag)
	{
		if (jumpCoroutine != null) {
			StopCoroutine (jumpCoroutine);
			jumpCoroutine = null;
		}
		isThrowed = flag;
		//if object was getting throwing direction then don't rotate
		if (ThrowBall.Instance.IsGettingDirection)
			isRotate = false;
		else
			isRotate = true;

		throwPos = (ThrowBall.Instance.target.position.z - transform.position.z);

	}

	/// <summary>
	/// Resets the ball.
	/// </summary>
	public void ResetBall ()
	{
		isThrowed = false;
		StopAllCoroutines ();

		//ball move to initial position
		StartCoroutine (MoveBackToInitialPositionCoroutine (0.3f));
	}

	/// <summary>
	/// Sets the curve.
	/// </summary>
	/// <param name="cFactore">C factore.</param>
	private void SetCurve (float cFactore)
	{
		curveFactore = cFactore;
		isCurve = true;
	}

	/// <summary>
	/// Sets the normal throw.
	/// </summary>
	/// <param name="cFactore">C factore.</param>
	private void SetNormalThrow (float cFactore)
	{
		curveFactore = cFactore;
		isNormalThrow = true;
	}
	#endregion
	
	#region Public_Methods
	#endregion
	
	#region Properties
	#endregion
	
	#region Coroutine

	/// <summary>
	/// Gots the pokemon coroutine.
	/// </summary>
	/// <returns>The pokemon coroutine.</returns>
	/// <param name="collidedObject">Collided object.</param>
	IEnumerator GotPokemonCoroutine (GameObject collidedObject)
	{
		yield return new WaitForSeconds (0.5f);
		childBall.gameObject.SetActive (false);
		collidedObject.SetActive (true);
		Destroy (gameObject);

	}

	/// <summary>
	/// Wind the specified t.
	/// </summary>
	/// <param name="t">T.</param>
	IEnumerator wind (float t)
	{
		isWind = true;
		float rate = 1.0f / t;
		float i = 0f;
		while (i<1.0f) {
			i += rate * Time.deltaTime;
			windFactor = Mathf.Lerp (1, 26, i);
			yield return 0;
		}
	}

	/// <summary>
	/// Winds the reverse.
	/// </summary>
	/// <returns>The reverse.</returns>
	/// <param name="t">T.</param>
	IEnumerator windReverse (float t)
	{
		isWind = true;
		float rate = 1.0f / t;
		float i = 0f;
		while (i<1.0f) {
			i += rate * Time.deltaTime;
			windFactor = Mathf.Lerp (26, 0, i);
			//Debug.Log("reverse:"+windFactor);
			yield return 0;
		}
		//isWind=false;
	}

	/// <summary>
	/// Stop this instance.
	/// </summary>
	IEnumerator stop ()
	{
		isRotate = false;
		yield return new WaitForSeconds (0.5f);
		Destroy (gameObject);
		
	}

	/// <summary>
	/// Graduallies the jump coroutine.
	/// </summary>
	/// <returns>The jump coroutine.</returns>
	/// <param name="tm">Tm.</param>
	IEnumerator GraduallyJumpCoroutine (float tm)
	{

		while (!isThrowed) {

			yield return new WaitForSeconds (0.5f);
		
			transform.position = initialPosition;

			if (ThrowBall.Instance.IsGameStart) {


				isRotateLeft = !isRotateLeft;
				isRotate = true;
				float i = 0f;
				float rate = 1.0f / tm;
				Vector3 from = initialPosition;
				Vector3 to = new Vector3 (from.x, from.y + 0.3f, from.z);
	
				while (i<1.0f) {
					i += rate * Time.deltaTime;
					transform.position = Vector3.Lerp (from, to, i);
					//transform.localScale = Vector3.Lerp (fromScale, toScale, i);
					yield return 0f;
				}


				i = 0f;
				rate = 1.0f / (tm / 0.7f);

				Vector3 bump = from;
				bump.y -= 0.05f;

				while (i<1.0f) {
					i += rate * Time.deltaTime;
					transform.position = Vector3.Lerp (to, bump, i);
					//	transform.localScale = Vector3.Lerp (toScale, fromScale, i);
					yield return 0f;
				}

				isRotate = false;

				i = 0f;
				rate = 1.0f / (tm / 1.1f);

				while (i<1.0f) {
					i += rate * Time.deltaTime;
					transform.position = Vector3.Lerp (bump, from, i);
					yield return 0f;
				}



			}
		}
	}

	/// <summary>
	/// Moves the back to initial position coroutine.
	/// </summary>
	/// <returns>The back to initial position coroutine.</returns>
	/// <param name="tm">Tm.</param>
	IEnumerator MoveBackToInitialPositionCoroutine (float tm)
	{
		float i = 0f;
		float rate = 1.0f / tm;
		Vector3 from = transform.position;
		Vector3 to = initialPosition;
		while (i<1.0f) {
			i += rate * Time.deltaTime;
			transform.position = Vector3.Lerp (from, to, i);
			yield return 0f;
		}

		transform.position = initialPosition;
		childBall.localRotation = Quaternion.identity;
		isRotate = false;
	
		//initalize gradually jump coroutine
		jumpCoroutine = GraduallyJumpCoroutine (0.3f);
		
		StartCoroutine (jumpCoroutine);
	}
	#endregion
	
	#region RPC
	#endregion

}
