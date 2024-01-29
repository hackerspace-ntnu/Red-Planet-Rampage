using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private T template;
    private List<T> pool;
    private int nextIndex = 0;
    private int capacity;

    public ObjectPool(T template, int capacity = 70)
    {
        this.template = template;
        this.capacity = capacity;
        pool = new List<T>(capacity);
    }

    public T Get()
    {
        var isNotYetFull = pool.Count < capacity;
        if (isNotYetFull)
        {
            var instance = GameObject.Instantiate(template);
            pool.Add(instance);
            return instance;
        }
        else
        {
            var instance = pool[nextIndex];
            // Instance may be destroyed with its parent
            if (!instance)
                instance = GameObject.Instantiate(template);

            nextIndex = (nextIndex + 1) % capacity;
            return instance;
        }
    }
}
