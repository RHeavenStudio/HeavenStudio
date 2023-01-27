// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Rigidbody2D ) )]
[RequireComponent( typeof( CircleCollider2D ) )]
[AddComponentMenu( "" )]
public class AmplifyColorTriggerProxy2D : AmplifyColorTriggerProxyBase
{
	private CircleCollider2D circleCollider;
	private Rigidbody2D rigidBody;

	void Start()
	{
		circleCollider = GetComponent<CircleCollider2D>();
		circleCollider.radius = 0.01f;
		circleCollider.isTrigger = true;

		rigidBody = GetComponent<Rigidbody2D>();
		rigidBody.gravityScale = 0;
		rigidBody.isKinematic = true;
	}

	void LateUpdate()
	{
		transform.position = Reference.position;
		transform.rotation = Reference.rotation;
	}
}
