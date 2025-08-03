using UnityEngine;
using Assets.Scripts;
using System.Collections.Generic;

public class HerbivoreSpawn : MonoBehaviour
{
    public GameObject herbivorePrefab, plantPrefab;
    public int count = 20;
    public float radius = 10f;

    private List<GameObject> spawned = new();

    private void Start()
    {
        Debug.Log("HerbivoreSpawn Start");

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * radius;
            GameObject animalGO = Instantiate(herbivorePrefab, pos, Quaternion.identity);

            HerbivoreAnimalProcess process = animalGO.GetComponent<HerbivoreAnimalProcess>();
            process.Initialize(new Animal(AnimalType.Herbivore), herbivorePrefab, plantPrefab);

            spawned.Add(animalGO);
        }
    }
}