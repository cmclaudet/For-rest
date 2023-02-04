using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour {
	private static readonly int IsWalkingTrigger = Animator.StringToHash("isWalking");

	[SerializeField]
	private float walkSpeed = 10f;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private Animator animator;

    private Vector2 moveInput;
	private Rigidbody rb;

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

		animator.SetBool(IsWalkingTrigger, Mathf.Abs(moveInput.x) > 0.01f);

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
}
