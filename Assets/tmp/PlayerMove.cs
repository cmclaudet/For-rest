using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
	[SerializeField]
	private float walkSpeed = 10f;

	[SerializeField]
	private SpriteRenderer sprite;

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
