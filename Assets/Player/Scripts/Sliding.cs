using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{

	[Header("Reference")]
	public Transform orientation;
	public Transform playerObj;
	private Rigidbody player;
	private PlayerMovement moves;

	[Header("Sliding")]
	public float maxSlideTime;
	public float slideForce;
	private float slideTimer;

	public float slideYScale;
	private float startYScale;

	[Header("Input")]
	public KeyCode slideKey = KeyCode.LeftControl;
	private float horizontalInput;
	private float verticalInput;

	


	// Start is called before the first frame update
	void Start()
	{
		player = GetComponent<Rigidbody>();
		moves = GetComponent<PlayerMovement>();

		startYScale = playerObj.localScale.y;
	}

	// Update is called once per frame
	void Update()
	{
		horizontalInput = Input.GetAxisRaw("Horizontal");
		verticalInput = Input.GetAxisRaw("Vertical");

		if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
			StartSlide();
		if (Input.GetKeyUp(slideKey) &&  moves.sliding)
			StopSlide();

	}

	private void FixedUpdate()
	{
		if (moves.sliding)
			SlidingMovement();

	}


	/// <summary>
	/// 
	/// </summary>
	void StartSlide()
	{
		moves.sliding = true;
		playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

		player.AddForce(Vector3.down * 5f, ForceMode.Impulse);

		slideTimer = maxSlideTime;


	
	}




	void SlidingMovement()
	{
		Vector3 inputDir = orientation.forward * verticalInput + orientation.right* horizontalInput;

		//Sliding normal
		if(!moves.OnSlope() || player.velocity.y > -0.1f)
		{

			player.AddForce(inputDir.normalized * slideForce, ForceMode.Force);

			slideTimer -= Time.deltaTime;
		}

		else
		{
			player.AddForce(moves.GetSlopeMoveDirection(inputDir) * slideForce, ForceMode.Force);

		}

		if (slideTimer <= 0) 
			StopSlide();

	}

	void StopSlide()
	{
		moves.sliding = false;

		playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
	}
}
