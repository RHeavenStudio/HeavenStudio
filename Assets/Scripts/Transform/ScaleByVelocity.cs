using UnityEngine;

public class ScaleByVelocity : MonoBehaviour
{
	public enum Axis { X, Y }

	public float bias = 1f;
	public float strength = 1f;
	public Axis axis = Axis.Y;
	public float size;

	public new Rigidbody2D rigidbody;

	private Vector2 startScale;

	private void Start()
	{
		startScale = transform.localScale;
	}

	private void Update()
	{
		var velocity = rigidbody.velocity.magnitude;

		/*if (Mathf.Approximately (velocity, 0f))
			return;*/

		var amount = velocity * strength + bias;
		var inverseAmount = 1.0f;
		if (velocity > 0.4f)
			inverseAmount = (1f / amount) * startScale.magnitude;

		switch (axis)
		{
			case Axis.X:
				transform.localScale = new Vector3(amount - 0.414214f, inverseAmount, 1f);
				return;
			case Axis.Y:
				transform.localScale = new Vector3(Mathf.Clamp(inverseAmount, 0.6f, 1f), amount, 1f);
				return;
		}
	}
}