using UnityEngine;
using Assets.Scripts;
using System.Collections.Generic;

public class PredatorySpawn : MonoBehaviour
{
    public GameObject predatoryPrefab, herbivorePrefab;
    public int count = 5;
    public float radius = 10f;

    private List<GameObject> spawned = new();

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * radius;
            GameObject animalGO = Instantiate(predatoryPrefab, pos, Quaternion.identity);

            PredatoryAnimalProcess process = animalGO.GetComponent<PredatoryAnimalProcess>();
            process.Initialize(new Animal(AnimalType.Predatory), predatoryPrefab, herbivorePrefab);

            spawned.Add(animalGO);
        }
    }
}