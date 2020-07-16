using UnityEngine;

namespace Game
{
    public class Character
    {
        public string Name { get; set; }

        public GameObject GetModelPrefab()
        {
            return Resources.Load<GameObject>("Characters/" + Name + "/Model/model");
        }

        public GameObject GetPlayerPrefab()
        {
            return Resources.Load<GameObject>(GetPlayerPrefabPath());
        }

        public string GetPlayerPrefabPath()
        {
            return "Characters/" + Name + "/player";
        }
    }
}