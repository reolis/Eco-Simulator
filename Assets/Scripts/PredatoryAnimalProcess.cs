using Assets.Scripts;
using UnityEngine;

public class PredatoryAnimalProcess : MonoBehaviour
{
    private Animal animal;
    private GameObject animalPrefab, targetPrefab;
    private float timeSinceLastDecision = 0f;
    private float decisionInterval = 1f;

    private Vector2 currentDirection = Vector2.zero;
    private float directionChangeTimer = 0f;
    private float directionChangeInterval = 2f;
    private Transform targetAnimal;

    public void Initialize(Animal newAnimal, GameObject animalSt, GameObject herbivoreSt)
    {
        animal = newAnimal;
        animalPrefab = animalSt;
        targetPrefab = herbivoreSt;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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

            PredatoryAnimalProcess childProcess = childGO.GetComponent<PredatoryAnimalProcess>();
            childProcess.Initialize(child, animalPrefab, targetPrefab);
        }

        if ((animal.CurrentState == Animal.AnimalState.SeekFood || animal.CurrentState ==
            Animal.AnimalState.Chasing) && targetAnimal != null)
        {
            float dist = Vector2.Distance(transform.position, targetAnimal.position);
            if (dist <= 0.5f)
            {
                ExecuteAction(AnimalAI.Action.EatAnimal);
            }
        }

        timeSinceLastDecision += delta;

        if (timeSinceLastDecision >= decisionInterval)
        {
            AnimalAI.Action next;
            if (animal.TypeOfAnimal == AnimalType.Predatory && animal.Hunger > 65)
            {
                next = AnimalAI.Action.Chase;
            }
            else
            {
                next = AnimalAI.Action.SeekFood;
            }

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
            case AnimalAI.Action.EatAnimal:
                animal.CurrentState = Animal.AnimalState.Eating;
                TryEatNearbyAnimal();
                break;
            case AnimalAI.Action.SeekFood:
                animal.CurrentState = Animal.AnimalState.SeekFood;
                SeekFood();
                break;
        }
    }

    private void MoveLogic()
    {
        if (animal.CurrentState == Animal.AnimalState.Dying || animal.CurrentState == Animal.AnimalState.Idle)
            return;

        if (animal.CurrentState != Animal.AnimalState.SeekFood)
        {
            directionChangeTimer += Time.deltaTime;

            if (directionChangeTimer >= directionChangeInterval || currentDirection == Vector2.zero)
            {
                currentDirection = Random.insideUnitCircle.normalized;
                directionChangeTimer = 0f;
            }
        }

        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 5f);

        float speedModifier = (animal.CurrentState == Animal.AnimalState.Eating) ? 0.3f : 1f;
        transform.Translate(currentDirection * animal.Speed * speedModifier * Time.deltaTime, Space.World);
    }

    private void TryEatNearbyAnimal()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var col in nearby)
        {
            HerbivoreView animalView = col.GetComponent<HerbivoreView>();
            if (animalView != null)
            {
                float nutrition = animalView.BeEaten();
                animal.Hunger = Mathf.Max(0f, animal.Hunger - nutrition);
                animal.CurrentState = Animal.AnimalState.Eating;
                break;
            }
        }
    }

    private void SeekFood()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, animal.VisionField);

        float minDist = float.MaxValue;
        targetAnimal = null;

        foreach (var col in nearby)
        {
            HerbivoreView target = col.GetComponent<HerbivoreView>();
            if (target != null)
            {
                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    targetAnimal = target.transform;
                }
            }
        }

        if (targetAnimal != null)
        {
            Vector2 dir = (targetAnimal.position - transform.position).normalized;
            currentDirection = dir;
        }
        else
        {
            animal.CurrentState = Animal.AnimalState.Wander;
            currentDirection = Random.insideUnitCircle.normalized;
        }
    }
}
