﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float walkingSpeed;
	public float jumpVelocity;
	public float groundedTimerMax;
	public float jumpedTimerMax;
	public GameObject impactParticles;

	[HideInInspector]
	public int facingDir;
	[HideInInspector]
	public float movement;
	[HideInInspector]
	public Vector2 otherMovement;

	private float speed;
	private bool isGrounded;
	private float groundedTimer;
	private float jumpedTimer;
	private Vector2 groundNormal;
	private Vector2 playerSize;

	private Rigidbody2D rb2d;
	private LayerMask groundLayerMask;
	private Collider2D groundCollider;
	private Animator animator;
	private CameraShake cameraShake;


	void Start() {
		rb2d = GetComponent<Rigidbody2D>();
		groundLayerMask = LayerMask.GetMask("Enemy", "Ground");
		animator = GetComponent<Animator>();
		cameraShake = Camera.main.GetComponent<CameraShake>();

		speed = walkingSpeed;
		playerSize = GetComponent<CapsuleCollider2D>().size;
		jumpedTimer = jumpedTimerMax;
		groundedTimer = groundedTimerMax;
	}

	void Update() {
		movement = Input.GetAxis("Horizontal") * speed;
		rb2d.gravityScale = 1.0f;
		bool wasGrounded = isGrounded;
		isGrounded = checkIfGrounded();
		if (!wasGrounded && isGrounded && groundedTimer > 0.2f) {
			StartCoroutine(cameraShake.Shake(0.1f, 0.2f, 1.0f));
			animator.SetTrigger("Land");
		}

		int inputDir = (int)Input.GetAxisRaw("Horizontal");
		if (inputDir != 0) facingDir = inputDir;

		if (isGrounded) {
			groundedTimer = 0.0f;
			if (movement == 0.0f) rb2d.gravityScale = 0.0f;
			if (movement * groundNormal.x < 0.0f) movement *= groundNormal.y;
			if(jumpedTimer > jumpedTimerMax + 0.1f) otherMovement.x = 0.0f;
		}
		else {
			groundedTimer += Time.deltaTime;
		}

		if (Input.GetKeyDown(KeyCode.C)) {
			jumpedTimer = 0.0f;
		}
		if (groundedTimer < groundedTimerMax && jumpedTimer < jumpedTimerMax) Jump();
		if (Input.GetKeyUp(KeyCode.C) && rb2d.velocity.y > 0.0f) {
			rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y * 0.6f);
		}

		jumpedTimer += Time.deltaTime;
	}

	void FixedUpdate() {
		rb2d.velocity = new Vector2(movement + otherMovement.x, rb2d.velocity.y);
	}

	void OnCollisionStay2D(Collision2D collision) {
		if(collision.collider == groundCollider) {
			Vector2 highest = collision.contacts[0].normal.normalized;
			for(int i = 1; i < collision.contactCount; i++) {
				Vector2 current = collision.contacts[i].normal.normalized;
				if (current.y > highest.y) highest = current;
			}
			if(highest.y > 0.25f)
				groundNormal = highest;
			else
				groundNormal = Vector2.zero;
		}
	}

	bool checkIfGrounded() {
		groundCollider = Physics2D.OverlapCircle((Vector2)transform.position + Vector2.down * playerSize.y * 0.25f, playerSize.x * 0.48f, groundLayerMask);
		if (groundCollider && groundCollider.isTrigger) groundCollider = null;
		return (bool)groundCollider;
	}

	void Jump() {
		jumpedTimer = jumpedTimerMax;
		groundedTimer = groundedTimerMax;

		Vector2 jumpDir = new Vector2(groundNormal.x * 0.45f, groundNormal.y * 2.0f).normalized;
		rb2d.velocity = new Vector2(rb2d.velocity.x, jumpDir.y * jumpVelocity);
		otherMovement.x = jumpDir.x * jumpVelocity;
	}

	public void InstantiateLandParticles() {
		Instantiate(impactParticles, transform.position + Vector3.down * playerSize.y * 0.5f, Quaternion.identity);
	}
}
