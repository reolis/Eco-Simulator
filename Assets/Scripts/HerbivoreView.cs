using UnityEngine;

namespace Assets.Scripts
{
    public class HerbivoreView : MonoBehaviour
    {
        public Animal AnimalModel { get; private set; }

        public void Initialize(Animal model)
        {
            AnimalModel = model;
        }

        void Update()
        {
            if (AnimalModel.IsDead && AnimalModel != null)
            {
                Destroy(gameObject);
            }
        }

        public float BeEaten()
        {
            if (AnimalModel == null)
            {
                Destroy(gameObject);
                return 0f;
            }

            float nutrition = AnimalModel.BeEaten();
            Destroy(gameObject);
            return nutrition;
        }
    }
}