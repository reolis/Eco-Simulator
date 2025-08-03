using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class AnimalAI
    {
        public enum Action
        {
            Wander,
            EatPlant,
            RunFromThreat,
            Sleep,
            SeekFood,
            Chase,
            EatAnimal
        }

        private Dictionary<Action, float> actionWeights = new()
        {
            { Action.Wander, 1f },
            { Action.EatPlant, 1f },
            { Action.RunFromThreat, 1f },
            { Action.Sleep, 1f },
            { Action.SeekFood, 1f },
            { Action.Chase, 1f },
            { Action.EatAnimal, 1f },
        };

        public float Hunger { get; private set; } = 0f;
        public float Energy { get; private set; } = 150f;
        public float HP { get; private set; } = 300f;
        public bool IsThreatNearby { get; set; } = false;

        public Action DecideAction()
        {
            if (IsThreatNearby)
            {
                return Action.RunFromThreat;
            }

            actionWeights[Action.EatPlant] = Mathf.Lerp(1f, 10f, Mathf.InverseLerp(30f, 150f, Hunger));
            actionWeights[Action.Wander] = Mathf.Lerp(1f, 0.1f, Mathf.InverseLerp(30f, 150f, Hunger));
            actionWeights[Action.SeekFood] = Mathf.Lerp(1f, 10f, Mathf.InverseLerp(30f, 150f, Hunger));
            actionWeights[Action.Chase] = Mathf.Lerp(1f, 10f, Mathf.InverseLerp(100f, 300f, Hunger));
            actionWeights[Action.EatAnimal] = Mathf.Lerp(1f, 10f, Mathf.InverseLerp(100f, 300f, Hunger));

            return actionWeights
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .First();
        }

        public void Learn(Action action, float reward)
        {
            if (!actionWeights.ContainsKey(action)) return;

            actionWeights[action] += reward;
            actionWeights[action] = Mathf.Clamp(actionWeights[action], 0f, 10f);
        }

        public void Tick(float deltaTime)
        {
            Hunger += deltaTime * 5;
            Energy -= deltaTime * 3;
        }

        public void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.Wander:
                    Energy -= 2f;
                    break;
                case Action.EatPlant:
                    Hunger -= 20f;
                    Energy -= 1f;
                    Learn(action, +1f);
                    break;
                case Action.RunFromThreat:
                    Energy -= 5f;
                    Learn(action, +0.5f);
                    break;
                case Action.Sleep:
                    Energy += 10f;
                    Learn(action, +0.2f);
                    break;
                case Action.SeekFood:
                    Energy -= 10f;
                    Learn(action, +1.5f);
                    break;
                case Action.Chase:
                    Energy -= 20f;
                    Learn(action, +1f);
                    break;
                case Action.EatAnimal:
                    Energy -= 20f;
                    Learn(action, +2f);
                    break;
            }
        }
    }
}
