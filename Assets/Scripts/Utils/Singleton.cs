using UnityEngine;

namespace XDPaint.Utils
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected void Awake()
        {
            CacheInstance();
        }

        private void CacheInstance()
        {
            if (Instance == null)
            {
                Instance = GetComponent<T>();
            }
            else
            {
                var type = typeof(T).ToString();
                Debug.LogWarning("Singleton<" + type + "> instance created already.");
                DestroyImmediate(this);
            }
        }
    }
}