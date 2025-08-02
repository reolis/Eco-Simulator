using UnityEngine;
using Assets.Scripts;

public class HerbivoreAnimalProcess : MonoBehaviour
{
    private Animal animal;
    private GameObject animalPrefab, plantPrefab;
    private float timeSinceLastDecision = 0f;
    private float decisionInterval = 1f;

    private Vector2 currentDirection = Vector2.zero;
    private float directionChangeTimer = 0f;
    private float directionChangeInterval = 2f;

    public void Initialize(Animal newAnimal, GameObject animalSt, GameObject plantSt)
    {
        animal = newAnimal;
        animalPrefab = animalSt;
        plantPrefab = plantSt;
    }

    void Update()
    {
        if (animal == null || animal.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        float delta = Time.deltaTime;
        animal.Delta = delta;
        animal.UpdateStates(delta);

        Animal child = animal.TryReproduce();
        if (child != null)
        {
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle.normalized * 1f;
            GameObject childGO = Instantiate(animalPrefab, spawnPos, Quaternion.identity);

            HerbivoreAnimalProcess childProcess = childGO.GetComponent<HerbivoreAnimalProcess>();
            childProcess.Initialize(child, animalPrefab, plantPrefab);
        }

        timeSinceLastDecision += delta;

        if (timeSinceLastDecision >= decisionInterval)
        {
            AnimalAI.Action next = (animal.TypeOfAnimal == AnimalType.Herbivore && animal.Hunger > 40)
                ? AnimalAI.Action.EatPlant
                : AnimalAI.Action.Wander;

            ExecuteAction(next);
            timeSinceLastDecision = 0f;
        }

        MoveLogic();
    }

    private void ExecuteAction(AnimalAI.Action action)
    {
        switch (action)
        {
            case AnimalAI.Action.Wander:
                animal.CurrentState = Animal.AnimalState.Wander;
                break;
            case AnimalAI.Action.Sleep:
                animal.CurrentState = Animal.AnimalState.Idle;
                break;
            case AnimalAI.Action.EatPlant:
                animal.CurrentState = Animal.AnimalState.Eating;
                TryEatNearbyPlant();
                break;
        }
    }

    private void MoveLogic()
    {
        if (animal.CurrentState == Animal.AnimalState.Dying || animal.CurrentState == Animal.AnimalState.Idle)
            return;

        directionChangeTimer += Time.deltaTime;

        if (directionChangeTimer >= directionChangeInterval || currentDirection == Vector2.zero)
        {
            currentDirection = Random.insideUnitCircle.normalized;
            directionChangeTimer = 0f;
        }

        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 5f);

        float speedModifier = (animal.CurrentState == Animal.AnimalState.Eating) ? 0.3f : 1f;

        transform.Translate(currentDirection * animal.Speed * speedModifier * Time.deltaTime, Space.World);
    }

    private void TryEatNearbyPlant()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var col in nearby)
        {
            PlantView plantView = col.GetComponent<PlantView>();
            if (plantView != null)
            {
                float nutrition = plantView.BeEaten();
                animal.Hunger = Mathf.Max(0f, animal.Hunger - nutrition);
                animal.CurrentState = Animal.AnimalState.Eating;
                break;
            }
        }
    }
}