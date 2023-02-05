using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour {
	private static readonly int IsWalkingBool = Animator.StringToHash("isWalking");
	private static readonly int MoveXInt = Animator.StringToHash("moveX");
	private static readonly int MoveYInt = Animator.StringToHash("moveY");

	[SerializeField]
	private float walkSpeed = 10f;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private Animator animator;

    private Vector2 moveInput;
	private Rigidbody rb;

	[SerializeField]
	private GameObject rootPrefab;
	private Generator rootGen;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
	{
		Move();
	}

	private void Move()
	{
		Vector3 velocity = new Vector3(moveInput.x * walkSpeed, rb.velocity.y, moveInput.y * walkSpeed);
		rb.velocity = transform.TransformDirection(velocity);

		bool shouldWalk = moveInput.magnitude > 0.01f;
		animator.SetBool(IsWalkingBool, shouldWalk);

		if (shouldWalk) {
			animator.SetFloat(MoveXInt, moveInput.x);
			animator.SetFloat(MoveYInt, moveInput.y);
		}

		if (velocity.x < 0)
		{
			sprite.flipX = true;
		} else if (velocity.x > 0) {
        sprite.flipX = false;
    }
	}

	void OnMove(InputValue value)
	{
		moveInput = value.Get<Vector2>();
	}

	void OnFire(InputValue value)
	{
		if (rootGen != null)
		{
			rootGen.enabled = false;
			rootGen = null;
		}
		else
		{
			rootGen = Instantiate(rootPrefab, transform.position, Quaternion.identity).GetComponent<Generator>();
			rootGen.GrowRoots();
		}
	}
}
