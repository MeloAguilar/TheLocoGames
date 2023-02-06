using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{

	[Header("Wall Running")]
	public LayerMask whatIsWall;
	public LayerMask whatIsGround;
	public float wallRunForce;
	public float wallRunJumpUpForce;
	public float wallJumpSideForce;
	public float maxWallRunTime;
	private float wallRunTimer;


	[Header("Input")]
	public KeyCode jumpKey = KeyCode.Space;
	public KeyCode upwardsRunKey = KeyCode.LeftShift;
	public KeyCode downwardsRunKey = KeyCode.LeftControl;
	private bool upwardsRunning;
	private bool downwardsRunning;
	private float horizontalInput;
	private float verticalInput;


	[Header("Detection")]
	public float wallCheckDistance;
	public float minJumpHeight;
	private RaycastHit leftWallHit;
	private RaycastHit rightWallHit;
	private bool wallLeft;
	private bool wallRight;


	[Header("Exiting")]
	private bool exitingWall;
	public float exitWallTime;
	private float exitWallTimer;

	[Header("Gravity")]
	public bool useGravity;
	public float gravityounterForce;

	[Header("References")]
	public Transform orientation;
	public Camera cam;
	private PlayerMovement moves;
	private Rigidbody player;


	// Start is called before the first frame update
	void Start()
	{
		player = GetComponent<Rigidbody>();
		moves = GetComponent<PlayerMovement>();

	}

	// Update is called once per frame
	void Update()
	{
		CheckForWall();

		StateMachine();
	}

	private void FixedUpdate()
	{

		//booleano del Script de movimiento del jugador
		if (moves.wallrunning)
			WallRunningMovement();
	}



	/// <summary>
	/// Establece las capacidades del jugador y cuando puede utilizarlas
	/// </summary>
	private void StateMachine()
	{
		horizontalInput = Input.GetAxisRaw("Horizontal");
		verticalInput = Input.GetAxisRaw("Vertical");


		//State 1 -- WallRunning
		if (wallLeft || wallRight && verticalInput > 0 && AboveGround() && !exitingWall)
		{
			if (!moves.wallrunning)
				StartWallRun();


			//COntador par a que el wallrun no sea infinito
			if(wallRunTimer> 0)
				wallRunTimer -= Time.deltaTime;	

			if (wallRunTimer <= 0 && moves.wallrunning)
			{
				exitingWall= true;
				exitWallTimer = exitWallTime;
			}

			//Wall Jumpp
			if (Input.GetKeyDown(jumpKey)) WallJump();

		}
		//State2 - Saliendo del WallRunning
		else if (exitingWall)
		{
			if(moves.wallrunning)
				StopWallRun();
			if(exitWallTimer > 0)
				exitWallTimer -= Time.deltaTime;
			if (exitWallTimer <= 0)
				exitingWall = false;
		}
		else
			StopWallRun();
	}



	/// <summary>
	/// Acciona la capacidad de andar por las paredes
	/// </summary>
	private void StartWallRun()
	{
		moves.wallrunning = true;
		wallRunTimer = maxWallRunTime;

		player.velocity = new Vector3(player.velocity.x, 0f, player.velocity.z);

	
	}



	/// <summary>
	/// Devuelve al jugador a su estado original
	/// </summary>
	private void StopWallRun()
	{
		moves.wallrunning = false;
	}



	/// <summary>
	/// Ofrece al jugador la capacidad de saltar cuando está realizando
	/// un WallRun
	/// </summary>
	private void WallJump()
	{
		exitingWall= true;
		exitWallTimer = exitWallTime;
		Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

		Vector3 forceToAply = transform.up * wallRunJumpUpForce + wallNormal * wallJumpSideForce;

		//Añado la fuerza al cuerpo del persoaje
		player.velocity = new Vector3(player.velocity.x, 0f, player.velocity.z);
		player.AddForce(forceToAply, ForceMode.Impulse);


	}



	/// <summary>
	/// Realiza los cambios necesarios en la gravedad, la velocidad y el vector de movimiento inicial del jugador.
	/// </summary>
	private void WallRunningMovement()
	{

		//Quitamos la gravedad y recogemos la velocidad de nuestro personaje, quitandole la velocidad en y
		player.useGravity = false;
		player.velocity = new Vector3(player.velocity.x, 0f, player.velocity.z);


		//Recogemos la normal del muro y la aplicamos a .Cross para que nos de,
		//junto a el vector que appunta hacia arriba de nuestro personaje
		Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
		Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

		if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
			wallForward = -wallForward;

		//Añadimos fuerza
		player.AddForce(wallForward * wallRunForce, ForceMode.Force);

		//Para pegarnos al muro
		player.AddForce(-wallNormal * 100, ForceMode.Force);
	}

	/// <summary>
	/// Comprueba que se encuentre tocando el suelo
	/// </summary>
	/// <returns>
	///		true : si el rayo lanzado no ha tocado el suelo
	///		
	///		false : si el rayo alcanzado ha tocado el suelo
	/// </returns>
	private bool AboveGround()
	{
		return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
	}





	/// <summary>
	/// Comprueba que haya una pared al rededor del jugador
	/// </summary>
	private void CheckForWall()
	{
		wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
		wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);

	}
}
