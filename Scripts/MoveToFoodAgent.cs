using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class MoveToFoodAgent : Agent
{
	[SerializeField] private bool isTraining;
	[SerializeField] private float movementSpeed;
	[SerializeField] private GameObject sprite;
	[SerializeField] private GameObject carriedFood;

	private Vector2 smoothedDirection = Vector2.zero;
	private Vector2 nestLocation;
	private bool carryingFood;

	public override void OnEpisodeBegin()
	{
		transform.position = nestLocation;
		carryingFood = false;
		
		// cache the position of the nest
		nestLocation = GameObject.FindGameObjectWithTag("Nest").transform.position;

		FoodManager.Instance.ResetFood();
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(transform.position);
		sensor.AddObservation(carryingFood);
		sensor.AddObservation(nestLocation);
	}

	// Gain manual control over the ant by setting the behaviour type to heuristic only
	public override void Heuristic(in ActionBuffers actionsOut)
	{
		ActionSegment<float> ContinuousActions = actionsOut.ContinuousActions;
		ContinuousActions[0] = Input.GetAxisRaw("Horizontal");
		ContinuousActions[1] = Input.GetAxisRaw("Vertical");
	}

	public override void OnActionReceived(ActionBuffers actions)
	{
		// Reduce reward over time to  incentivize movement
		AddReward(-0.0025f);

		float moveX = actions.ContinuousActions[0];
		float moveY = actions.ContinuousActions[1];

		Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

		// Smooth out the movement direction
		smoothedDirection = Vector2.Lerp(smoothedDirection, moveDirection, 100f);

		// If there's any smoothed movement input, calculate rotation and move the agent
		if (smoothedDirection != Vector2.zero)
		{
			// Calculate the angle of rotation
			float angle = Mathf.Atan2(smoothedDirection.y, smoothedDirection.x) * Mathf.Rad2Deg;

			// Interpolate between the current rotation and the target rotation.
			// We only rotate the sprite that is attached to a child object in order to not mess with the AI.
			Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);
			sprite.transform.rotation = Quaternion.Slerp(sprite.transform.rotation, targetRotation, 5f * Time.deltaTime);
			
			// Move the agent
			if (carryingFood)
			{
				transform.position += new Vector3(smoothedDirection.x, smoothedDirection.y, 0) * Time.deltaTime * movementSpeed / 2;
			}
			else 
			{
				transform.position += new Vector3(smoothedDirection.x, smoothedDirection.y, 0) * Time.deltaTime * movementSpeed;
			}
			
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent<Food>(out Food food) && !carryingFood)
		{
			carryingFood = true;

			Destroy(other.gameObject);

			carriedFood.SetActive(true);

			// Give small reward just for finding food
			AddReward(0.1f);
		}

		// End episode with maximum reward when the ant brings food to the nest
		if (carryingFood && other.TryGetComponent<Nest>(out Nest nest))
		{
			UIManager.Instance.AddFood();
			carryingFood = false;
			SetReward(1f);

			// Animate food being brought back
			LeanTween.cancel(carriedFood);
			carriedFood.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			LeanTween.scale(carriedFood, Vector2.zero, 0.35f).setEaseInBack().setOnComplete( () =>
			{
				carriedFood.SetActive(false);
				carriedFood.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			});

			FoodManager.Instance.SpawnFood();

			if (isTraining)
			{
				EndEpisode();
			}
		}

		// End episode with negative reward if the ant collides with wall
		if (other.TryGetComponent<Border>(out Border border))
		{
			SetReward(-1f);

			if (isTraining)
			{
				EndEpisode();
			}
		}
	}
}
