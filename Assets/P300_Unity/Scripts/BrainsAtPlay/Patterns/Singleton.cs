using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T m_instance = default(T);
    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject go = null;
                if (GameObject.Find("Singleton") == false)
                {
                    go = new GameObject("Singleton");
                }
                if (go != null)
                    m_instance = go.AddComponent<T>();
            }
            return m_instance;
        }
    }
}