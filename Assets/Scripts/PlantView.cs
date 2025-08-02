using UnityEngine;

namespace Assets.Scripts
{
    public class PlantView : MonoBehaviour
    {
        public Plant PlantModel { get; private set; }

        public void Initialize(Plant model)
        {
            PlantModel = model;
        }

        void Update()
        {
            PlantModel?.UpdatePlant(Time.deltaTime);

            if (!PlantModel?.IsAlive ?? true)
            {
                Destroy(gameObject);
            }
        }

        public float BeEaten()
        {
            float nutrition = PlantModel.BeEaten();
            Destroy(gameObject);
            return nutrition;
        }
    }
}