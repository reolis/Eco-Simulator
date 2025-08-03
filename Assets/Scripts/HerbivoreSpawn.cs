using UnityEngine;
using Assets.Scripts;
using System.Collections.Generic;

public class HerbivoreSpawn : MonoBehaviour
{
    public GameObject herbivorePrefab, plantPrefab;
    public int count = 30;
    public float radius = 10f;

    private List<GameObject> spawned = new();

    private void Start()
    {
        Debug.Log("HerbivoreSpawn Start");

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomPositionWithinCamera();
            GameObject animalGO = Instantiate(herbivorePrefab, pos, Quaternion.identity);

            HerbivoreAnimalProcess process = animalGO.GetComponent<HerbivoreAnimalProcess>();
            process.Initialize(new Animal(AnimalType.Herbivore), herbivorePrefab, plantPrefab);

            spawned.Add(animalGO);
        }
    }

    private Vector2 GetRandomPositionWithinCamera()
    {
        Camera cam = Camera.main;
        Vector2 min = cam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 max = cam.ViewportToWorldPoint(new Vector2(1, 1));

        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        return new Vector2(x, y);
    }
}