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
            Vector2 pos = GetRandomPositionWithinCamera();
            GameObject animalGO = Instantiate(predatoryPrefab, pos, Quaternion.identity);

            PredatoryAnimalProcess process = animalGO.GetComponent<PredatoryAnimalProcess>();
            process.Initialize(new Animal(AnimalType.Predatory), predatoryPrefab, herbivorePrefab);

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