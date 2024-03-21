using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
	public static FoodManager Instance;

	[SerializeField] private GameObject food;
	[SerializeField] private int numberOfstartingFood;

	private List<GameObject> foods = new List<GameObject>();
	
	private void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		ResetFood();
	}

	public void SpawnFood()
	{
		StartCoroutine(SpawnFoodAfterRandomDelay());
	}

	public void ResetFood()
	{
		// Destroy leftover food from previous episode
		foreach(GameObject food in foods)
		{
			Destroy(food);
		}

		UIManager.Instance.ResetFood();

		for (int i = 0; i < numberOfstartingFood; i++)
		{
			GameObject newFood = Instantiate(food, Vector2.zero, Quaternion.identity);
			newFood.transform.position = new Vector3(Random.Range(-12, 12), Random.Range(-7, 12), 0);
			foods.Add(newFood);
		}
	}

	private IEnumerator SpawnFoodAfterRandomDelay()
	{
		yield return new WaitForSeconds(Random.Range(0f, 2f));
		
		GameObject newFood = Instantiate(food, Vector2.zero, Quaternion.identity);

		// Animate
		newFood.transform.localScale = Vector2.zero;
		LeanTween.scale(newFood, Vector2.one, 0.35f).setEaseOutBack();
		newFood.transform.position = new Vector3(Random.Range(-12, 12), Random.Range(-7, 12), 0);

		foods.Add(newFood);

		yield return null;
	}
}
