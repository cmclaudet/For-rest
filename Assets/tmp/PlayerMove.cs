using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour {
	private static readonly int IsWalkingBool = Animator.StringToHash("isWalking");
	private static readonly int MoveXInt = Animator.StringToHash("moveX");
	private static readonly int MoveYInt = Animator.StringToHash("moveY");

	[SerializeField]
	private float walkSpeed = 10f;

	[SerializeField]
	private float timeTillRootsGrow = 3f;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private float maxTimeTillNextInput = 0.5f;

	[SerializeField]
	private float wiggleTime = 3f;

  private Vector2 moveInput;
	private Rigidbody rb;

	[SerializeField]
	private GameObject rootPrefab;
	private Generator rootGen;

	private Coroutine scheduledRootGrowth;
	private Coroutine breakFree;
	private bool isPlayerRooted;

	private Vector2 lastMoveInput;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		TryScheduleRootGrowth(); ;
	}

	// Update is called once per frame
	void Update() {
		Move();
	}

	private void Move()
	{
		bool isTryingToWalk = moveInput.magnitude > 0.01f;

		if (isPlayerRooted) {
			if (isTryingToWalk && breakFree == null) {
				breakFree = StartCoroutine(TryBreakFree());
			}
			return;
		}

		Vector3 velocity = new Vector3(moveInput.x * walkSpeed, rb.velocity.y, moveInput.y * walkSpeed);
		rb.velocity = transform.TransformDirection(velocity);

		animator.SetBool(IsWalkingBool, isTryingToWalk);

		if (isTryingToWalk && !isPlayerRooted) {
			CancelScheduledRootGrowth();
			animator.SetFloat(MoveXInt, moveInput.x);
			animator.SetFloat(MoveYInt, moveInput.y);
		} else {
			TryScheduleRootGrowth ();
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
		ToggleGrowRoots();
	}

	IEnumerator TryBreakFree() {
		float timeSinceBreakFree = 0;
		float timeSinceLastDifferentInput = 0;

		while (timeSinceBreakFree < wiggleTime) {
			if (moveInput.magnitude > 0.01f) {
				if (lastMoveInput != Vector2.zero) {
					if (IsDifferentDirection(lastMoveInput, moveInput)) {
						lastMoveInput = moveInput;
						timeSinceLastDifferentInput = 0;
					} else {
						timeSinceLastDifferentInput += Time.deltaTime;
					}
				} else {
					lastMoveInput = moveInput;
					timeSinceLastDifferentInput = 0;
				}
			} else {
				timeSinceLastDifferentInput += Time.deltaTime;
			}

			if (timeSinceLastDifferentInput >= maxTimeTillNextInput) {
				timeSinceBreakFree = 0;
			} else {
				timeSinceBreakFree += Time.deltaTime;
			}

			yield return null;
		}

		BreakFree();
	}

	private void BreakFree() {
		lastMoveInput = Vector2.zero;
		isPlayerRooted = false;
		breakFree = null;

		ToggleGrowRoots();
	}

	private bool IsDifferentDirection(Vector2 direction1, Vector2 direction2) {
		if (direction1.x >= 0 && (direction2.x < 0) ||
		    direction1.x <= 0 && (direction2.x > 0) ||
		    direction1.y >= 0 && (direction2.y < 0) ||
		    direction1.y <= 0 && (direction2.y > 0)) {
			return true;
		}

		return false;
	}

	void TryScheduleRootGrowth() {
		if (scheduledRootGrowth == null) {
			scheduledRootGrowth = StartCoroutine(ScheduleRootGrowth());
		}
	}

	private IEnumerator ScheduleRootGrowth() {
		yield return new WaitForSeconds(timeTillRootsGrow);
		ToggleGrowRoots();
		isPlayerRooted = true;
	}

	private void CancelScheduledRootGrowth() {
		if (scheduledRootGrowth != null) {
			StopCoroutine(scheduledRootGrowth);
			scheduledRootGrowth = null;
		}
	}

	private void ToggleGrowRoots() {
		if (rootGen != null) {
			rootGen.Stop();
            rootGen.enabled = false;
			rootGen = null;
		} else {
			rootGen = Instantiate(rootPrefab, transform.position, Quaternion.identity).GetComponent<Generator>();
			rootGen.GrowRoots();
		}
	}
}
