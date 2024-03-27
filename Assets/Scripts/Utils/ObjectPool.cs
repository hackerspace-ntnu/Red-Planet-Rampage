using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private T template;
    private System.Func<T> requestTemplate;

    private List<T> pool;
    private ObjectPoolHolder parent;
    private int nextIndex = 0;
    private int capacity;

    private ObjectPool(int capacity)
    {
        this.capacity = capacity;
        pool = new List<T>(capacity);
        var holderInstance = new GameObject();
        holderInstance.name = "ObjectPoolHolder";
        parent = holderInstance.AddComponent<ObjectPoolHolder>();
    }

    /// <summary>
    /// Create an object pool with a template object.
    /// </summary>
    /// <param name="template">Template object that the pool should instantiate</param>
    /// <param name="capacity">Maximum capacity of the pool</param>
    public ObjectPool(T template, int capacity = 70) : this(capacity)
    {
        this.template = template;
    }

    /// <summary>
    /// Create an object pool with a template function.
    /// </summary>
    /// <param name="requestTemplate">Function that returns a template object that the pool can instantiate</param>
    /// <param name="capacity">Maximum capacity of the pool</param>
    public ObjectPool(System.Func<T> requestTemplate, int capacity = 70) : this(capacity)
    {
        this.requestTemplate = requestTemplate;
    }

    private T Create()
    {
        if (template != null)
            return GameObject.Instantiate(template);
        if (requestTemplate == null)
            throw new System.Exception("No proper object template or creation method provided to the pool!");
        return GameObject.Instantiate(requestTemplate());
    }

    /// <summary>
    /// Creates a new instance or retrieves one from the pool,
    /// prioritizing existing instances.
    /// Remember to reset the instance, as it may have been in use elsewhere.
    /// </summary>
    /// <returns>Usable instance of the template object</returns>
    public T Get()
    {
        var hasReturnedInstances = parent.transform.childCount > 0;
        if (hasReturnedInstances)
        {
            // Use stored instance.
            var instance = parent.transform.GetChild(0).gameObject.GetComponent<T>();
            instance.transform.parent = null;
            instance.gameObject.SetActive(true);
            return instance;
        }

        var isNotYetFull = pool.Count < capacity;
        if (isNotYetFull)
        {
            var instance = Create();
            pool.Add(instance);
            return instance;
        }
        else
        {
            var instance = pool[nextIndex];
            nextIndex = (nextIndex + 1) % capacity;

            if (!instance)
            {
                // Instance might have been destroyed together with its parent.
                instance = Create();
            }
            else
            {
                // Otherwise, ensure it is active and not parented anywhere anymore.
                instance.transform.parent = null;
                instance.gameObject.SetActive(true);
            }

            return instance;
        }
    }

    /// <summary>
    /// Creates a new instance or retrieves one from the pool,
    /// prioritizing existing instances.
    /// Returns it to the pool after its lifetime has passed.
    /// Remember to reset the instance, as it may have been in use elsewhere.
    /// </summary>
    /// <param name="lifetime">Time until the object should be returned to the pool</param>
    /// <returns>Usable instance of the template object</returns>
    public T GetAndReturnLater(float lifetime)
    {
        var instance = Get();
        parent.StartCoroutine(ReturnLater(instance, lifetime));
        return instance;
    }

    private IEnumerator ReturnLater(T instance, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Return(instance);
    }

    /// <summary>
    /// Return an instance to the pool.
    /// </summary>
    /// <param name="instance">Instance to return</param>
    public void Return(T instance)
    {
        if (!instance)
            return;
        instance.gameObject.SetActive(false);
        instance.transform.parent = parent.transform;
    }
}
