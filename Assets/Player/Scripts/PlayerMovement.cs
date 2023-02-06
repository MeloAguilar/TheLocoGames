using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

	[Header("Movement")]
	private float moveSpeed;
	public float walkSpeed;
	public float sprintSpeed;
	public float slideSpeed;

	float desireMoveSpeed;
	float lastdesireMoveSpeed;

	public float groundDrag;

	public float speedIncreaseMultiplier;
	public float slopeDecreaseMultiplier;


	public float wallrunSpeed;
	public Vector3 checkPoint;
	private Vector3 startCheckPint;


	
	[Header("Jump")]
	public float jumpForce;
	public float jumpCooldown;
	public float airMultiplier;
	bool readyToJump;


	[Header("Slope Handling")]
	public float maxSlopeAngle;
	private RaycastHit slopeHit;
	private bool exitingSlope;

	[Header("Player Health")]
	public float hearts = 6;
	private float countingHearts;

	[Header("Crouching")]
	public float crouchSpeed;
	public float crouchYScale;
	private float startYScale;

	[Header("Key Bindings")]
	public KeyCode jumpKey = KeyCode.Space;
	public KeyCode sprintKey = KeyCode.LeftShift;
	public KeyCode crouchKey = KeyCode.LeftControl;

	[Header("Ground Check")]
	public float playerHeight;
	public LayerMask whatIsGround;
	 bool grounded;

	[Header("References")]
	public Transform orientation;

	Vector3 moveDirection;
	Rigidbody player;
	float horizontalInput;
	float verticalInput;


	public bool sliding;
	public bool crouching;
	public bool wallrunning;

	public MovementState state;

	private bool canMove = true; //If player is not hitted
	private bool isStuned = false;
	private bool wasStuned = false; //If player was stunned before get stunned another time
	private float pushForce;
	private Vector3 pushDir;



	//Controls every state of the player
	public enum MovementState
	{
		walking,
		sliding,
		sprinting,
		crouching,
		wallrunning,
		air
	}


	private void Awake()
	{


		player = GetComponent<Rigidbody>();
		checkPoint = player.transform.position;
		startCheckPint = checkPoint;
		player.freezeRotation = true;
		countingHearts = hearts;
		Cursor.lockState = CursorLockMode.Locked;
	}
	// Start is called before the first frame update
	void Start()
	{



		readyToJump = true;

		startYScale = transform.localScale.y;
	}

	// Update is called once per frame
	void Update()
	{
		//Compuebo si está en el suelo
		IsGrounded();
		//Compruebo el input del jugador
		MyInput();
		//Controlo la velocidad para normalizarla de 0 a desireMoveSpeed
		SpeedControl();
		//Establezco el estado en el que se encuentra el rigidBody
		StateHandler();




		if (grounded)
		{
			player.drag = groundDrag;
		}
		else
		{
			player.drag = 0;
		}
	}



	private void FixedUpdate()
	{

		//Volvemos al ultimo CheckPoint si la posicion en y del vector del player es menor que -1
		if (player.position.y < -1)
		{
			transform.position = checkPoint;

			countingHearts -= 1;
		}

		//si se nos acaban los corazones volvemos al primer checkPoint
		else if (countingHearts < 1)
		{
			transform.position = startCheckPint;
			countingHearts = hearts;
		}
		MovePlayer();




	}



	/// <summary>
	/// Comprueba y establece el estado del jugador
	/// para saber en que accion se encuentra en cada momento
	/// </summary>
	void StateHandler()
	{

		//Mode - WallRun

		if (wallrunning)
		{
			state = MovementState.wallrunning;

			desireMoveSpeed = wallrunSpeed;
		}

		//Mode - Sliding
		else if (sliding)
		{
			state = MovementState.sliding;

			if (OnSlope() && player.velocity.y < 0.1f)
			{
				desireMoveSpeed = slideSpeed;
			}
			else
			{
				desireMoveSpeed = sprintSpeed;
			}
		}

		//Mode - Crouching
		else if (Input.GetKeyDown(crouchKey))
		{
			state = MovementState.crouching;

			desireMoveSpeed = crouchSpeed;
		}

		//Mode - Sprinting
		else if (IsGrounded() && Input.GetKey(sprintKey))
		{
			state = MovementState.sprinting;
			desireMoveSpeed = sprintSpeed;
		}

		//Mode - Walking
		else if (IsGrounded())
		{
			state = MovementState.walking;
			desireMoveSpeed = walkSpeed;
		}

		//Mode - Air
		else
		{
			state = MovementState.air;

		}


		//Comprueba que la velocidad deseada no haya cambiado drásticamente.
		if (Mathf.Abs(desireMoveSpeed - lastdesireMoveSpeed) > 4f && moveSpeed !=0)
		{
			StopAllCoroutines();
			StartCoroutine(SmoothlyLerpdesireMoveSpeed());
		}
		else
		{
			moveSpeed = desireMoveSpeed;
		}
		lastdesireMoveSpeed = desireMoveSpeed;
	}



	/// <summary>
	/// Corrutina que comprueba que la velocidad del usuario sea constante y decrezca en función de la diferencia 
	/// entre su velocidad actual y la que queremos conseguir al final de cada movimiento
	/// </summary>
	/// <returns></returns>
	private IEnumerator SmoothlyLerpdesireMoveSpeed()
	{
		float time = 0;
		float differnece = Mathf.Abs(desireMoveSpeed - moveSpeed);
		float startValue = moveSpeed;

		while (time < differnece)
		{
			moveSpeed = Mathf.Lerp(startValue, desireMoveSpeed, time / differnece);
			if (OnSlope())
			{
				float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
				float slopeAngleIncrease = 1 + (slopeAngle / 90f);

				time += Time.deltaTime * speedIncreaseMultiplier * slopeDecreaseMultiplier * slopeAngleIncrease;

			}

			time += Time.deltaTime * speedIncreaseMultiplier;

			yield return null;
		}

	}






	/// <summary>
	/// Comprueba que el componente rigidBody del player esté tocando una superficie
	/// </summary>
	/// <returns></returns>
	bool IsGrounded()
	{
		//Ground check
		grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
		return grounded;
	}

	/// <summary>
	/// Método que recoge el input del jugador e invoca 
	/// </summary>
	private void MyInput()
	{
		horizontalInput = Input.GetAxisRaw("Horizontal");
		verticalInput = Input.GetAxisRaw("Vertical");


		//Saltar
		if (Input.GetKey(jumpKey) && readyToJump && grounded)
		{
			readyToJump = false;

			Jump();

			Invoke(nameof(ResetJump), jumpCooldown);
		}

		//Agacharse
		if (Input.GetKeyDown(crouchKey))
		{
			transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

			player.AddForce(Vector3.down * 7f, ForceMode.Impulse);
		}

		//Parar de Agacharse
		if (Input.GetKeyUp(crouchKey))
		{
			transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
		}


	}


	/// <summary>
	/// Método que añade las fuerzas en el sentido necesario 
	/// según la accion que esté ralizando el jugaodr
	/// </summary>
	void MovePlayer()
	{
		moveDirection =  orientation.forward * verticalInput + orientation.right * horizontalInput;

		if (OnSlope() && !exitingSlope)
		{
			player.AddForce(GetSlopeMoveDirection(moveDirection) * desireMoveSpeed * 20f, ForceMode.Force);
			if (player.velocity.y > 0)
			{
				player.AddForce(Vector3.down * 60f, ForceMode.Force);
			}
		}
		else
		if (grounded)
			player.AddForce(moveDirection.normalized * desireMoveSpeed * 10f, ForceMode.Force);

		else if (!grounded)
			player.AddForce(moveDirection.normalized * desireMoveSpeed * 10f * airMultiplier, ForceMode.Force);

		//apagar gravedad si estamos en plano inclinado
		player.useGravity = !OnSlope();
	}


	/// <summary>
	/// Método que se encarga de controlar la velocidad del jugador 
	/// para que no se sumen velocidades cuando se está andando en diagonal
	/// </summary>
	void SpeedControl()
	{
		//Limitar velocidad en planos inclinados
		if (OnSlope() && !exitingSlope)
		{
			if (player.velocity.magnitude > desireMoveSpeed)
			
				player.velocity = player.velocity.normalized * desireMoveSpeed;
			
		}
		//Limitar la velocidad en el plano o en el aire
		else
		{
			Vector3 flatVel = new Vector3(player.velocity.x, 0f, player.velocity.z);


			//limita la velocidad si se necesita
			if (flatVel.magnitude > desireMoveSpeed)
			{
				Vector3 limitedVelocity = flatVel.normalized * desireMoveSpeed;
				player.velocity = new Vector3(limitedVelocity.x, player.velocity.y, limitedVelocity.z);

			}
		}


	}



	/// <summary>
	/// Se asegura de que la velocidad en y sea 0 
	/// y aplica un impulso sobre el rigidBody del jugador en ese eje con la 
	/// fuerza igual a multiplicar la componente del vector "arriba" y la fuerza de salto
	/// </summary>
	void Jump()
	{
		exitingSlope = true;

		player.velocity = new Vector3(player.velocity.x, 0f, player.velocity.z);


		player.AddForce(transform.up * jumpForce, ForceMode.Impulse);




	}



	/// <summary>
	/// Devuelve el estado a true del
	/// boolean que comprueba que se pueda saltar
	/// </summary>
	void ResetJump()
	{
		readyToJump = true;

		exitingSlope = false;
	}



	public void LoadCheckPoint()
	{
		transform.position = checkPoint;
	}


	/// <summary>
	/// Comprueba si donde se posiciona el personaje es un plano inclinado o recto 
	/// </summary>
	/// <returns></returns>
	public bool OnSlope()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
		{
			float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
			return angle > maxSlopeAngle && angle != 0;
		}

		return false;
	}


	/// <summary>
	/// Mueve el vector de direccion para que y se encuentre perpendicular al plano que intentamos subir
	/// </summary>
	/// <returns></returns>
	public Vector3 GetSlopeMoveDirection(Vector3 direction)
	{
		return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
	}

}
