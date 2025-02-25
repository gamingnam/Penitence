using System.Collections.Generic;
using UnityEngine;

//We set our object class to take in a generic where that generic can morph into any class we want 
public class ObjectPooler<T> where T : class
{
    private Queue<T> pool; //We make a Queue where can store any anything, hence why our generic T is replaced with any class types (GameObject,Component,etc.)
    private T prefab; //We taken in any type of prefab
    private Transform parent; //we set where our object is going to spawn

    /// <summary>
    /// Our constructor where initialize what we want in our pool
    /// </summary>
    /// <param name="prefab"> The prefab we want to pool</param>
    /// <param name="initialSize">how many of that prefab we want</param>
    /// <param name="parent">the position we want the prefab to spawn in (which we can leave alone or customize)</param>
    public ObjectPooler(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new Queue<T>();

        for (int i = 0; i < initialSize; i++)
        {
            T obj = CreateNewObject();
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Creates a new object from our pool to instantiate (can be expanded to include other types or classes)
    /// </summary>
    /// <returns>instantiates our object from the pool either as a GameObject or a Component</returns>
    private T CreateNewObject()
    {
        //If we specify our prefab to be a component, we instantiate as a component
        if (prefab is Component componentPrefab)
        {
            Component obj = Object.Instantiate(componentPrefab, parent);
            obj.gameObject.SetActive(false);
            return obj as T;
        }
        else if (prefab is GameObject gameObjectPrefab) //Else we instantiate as a GameObject
        {
            GameObject obj = Object.Instantiate(gameObjectPrefab, parent);
            obj.SetActive(false);
            return obj as T;
        }
        return null;
    }

	/// <summary>
	/// Retrieves an object from the pool, sets its position and rotation, and activates it.
	/// If the pool is empty, a new object is created and added to the pool before retrieval.
	/// </summary>
	/// <param name="position">The position to set for the retrieved object.</param>
	/// <param name="rotation">The rotation to set for the retrieved object.</param>
	/// <returns>The object retrieved from the pool.</returns>
	public T Get(Vector3 position = default, Quaternion rotation = default)
    {
        if (pool.Count == 0)
        {
            pool.Enqueue(CreateNewObject());
        }

        T obj = pool.Dequeue();
        
        if (obj is Component component)
        {
            component.gameObject.SetActive(true);
            component.transform.position = position;
            component.transform.rotation = rotation;
        }
        else if (obj is GameObject gameObject)
        {
            gameObject.SetActive(true);
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
        }

        return obj;
    }


	/// <summary>
	/// Deactivates the given object and returns it to the pool for future reuse.
	/// </summary>
	/// <param name="obj">The object to return to the pool.</param>
	public void ReturnToPool(T obj)
    {
        if (obj is Component component)
        {
            component.gameObject.SetActive(false);
        }
        else if (obj is GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

        pool.Enqueue(obj);
    }
}
