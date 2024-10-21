using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;

public class IndexedDBHelper : MonoBehaviour
{
    public static bool enableLog = false;

    // Define a helper class for parsing callback results
    [Serializable]
    private class CallbackResult
    {
        public int id;
        public bool success;
        public string message;
        public string error;
    }

    [DllImport("__Internal")]
    private static extern void OpenDatabase(string dbName, int version, int identifier, string gameObjectName, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void WriteData(string fileName, string value, int identifier, string gameObjectName, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void ReadData(string fileName, int identifier, string gameObjectName, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void DeleteData(string fileName, int identifier, string gameObjectName, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void FindData(string fileName, int identifier, string gameObjectName, string callbackMethod);
    
    [DllImport("__Internal")]
    public static extern void SyncIndexedDB();

    public static IndexedDBHelper instance;

    // Operation queue and completion identifier
    private Queue<(Action operation, Delegate callback, int identifier)> operationQueue = new Queue<(Action, Delegate, int)>();
    private Dictionary<int, CallbackResult> completedOperations = new Dictionary<int, CallbackResult>(); // Store completed operations' identifiers and return data
    private int nextId = 0;

    private void Start()
    {
        StartCoroutine(ProcessQueue()); // Ensure the coroutine runs continuously
    }

    public static IndexedDBHelper Create(string dbName, Action<bool> action)
    {
        var go = new GameObject("IndexedDBHelper");
        instance = go.AddComponent<IndexedDBHelper>();
        instance.OpenDB(dbName, action);
        return instance;
    }

    // Open database
    public void OpenDB(string dbName, Action<bool> action)
    {
        int currentId = nextId++;
        AddToQueue(currentId, () => OpenDatabase(dbName, 1, currentId, gameObject.name, "OnDatabaseOpened"), action);
    }

    // Write data
    public void WriteData(string fileName, string value, Action<bool> action)
    {
        int currentId = nextId++;
        AddToQueue(currentId, () => WriteData(fileName, value, currentId, gameObject.name, "OnDataSaved"), action);
    }

    public void WriteData(string fileName, byte[] value, Action<bool> action)
    {
        int currentId = nextId++;
        string base64String = Convert.ToBase64String(value);
        AddToQueue(currentId, () => WriteData(fileName, base64String, currentId, gameObject.name, "OnDataSaved"), action);
    }

    // Read data
    public void ReadData(string fileName, Action<string> action)
    {
        int currentId = nextId++;
        AddToQueue(currentId, () => ReadData(fileName, currentId, gameObject.name, "OnDataLoaded"), action);
    }

    public void ReadData(string fileName, Action<byte[]> action)
    {
        int currentId = nextId++;
        AddToQueue(currentId, () => ReadData(fileName, currentId, gameObject.name, "OnDataLoaded"), action);
    }

    // Delete data
    public void DeleteData(string fileName, Action<bool> action)
    {
        int currentId = nextId++;
        AddToQueue(currentId, () => DeleteData(fileName, currentId, gameObject.name, "OnDataDeleted"), action);
    }

    // Check if data exists
    public void FindData(string fileName, Action<bool> action)
    {
        int currentId = nextId++;
        AddToQueue(currentId, () => FindData(fileName, currentId, gameObject.name, "OnDataFound"), action);
    }

    // Add operation to the queue (generic)
    private void AddToQueue<T>(int id, Action operation, Action<T> callback = null)
    {
        operationQueue.Enqueue((operation, callback, id));
        if(enableLog) {
            Debug.Log($"AddToQueue {operation} {callback} {id}");
        }
    }

    // Callback after database opened
    public void OnDatabaseOpened(string message)
    {
        HandleCallback(message);
    }

    // Callback after data saved
    public void OnDataSaved(string data)
    {
        HandleCallback(data);
    }

    // Callback after data loaded
    public void OnDataLoaded(string data)
    {
        HandleCallback(data);
    }

    // Callback after data deleted
    public void OnDataDeleted(string data)
    {
        HandleCallback(data);
    }

    // Callback after data existence checked
    public void OnDataFound(string data)
    {
        HandleCallback(data);
    }

    // Handle callback
    private void HandleCallback(string data)
    {
        var result = JsonConvert.DeserializeObject<CallbackResult>(data);
        completedOperations[result.id] = result; // Store return data

        if (enableLog)
        {
            Debug.Log($"Callback Handled - ID: {result.id}, " +
                    $"Success: {result.success}, " +
                    $"Message: {result.message}, " +
                    $"Error: {result.error}");
        }
    }

    // Coroutine: Process operations in the queue and ensure callback order is consistent
    private IEnumerator ProcessQueue()
    {
        while (true)
        {
            if (operationQueue.Count > 0)
            {
                // Get operation from the queue
                var (operation, callback, identifier) = operationQueue.Dequeue();
                operation.Invoke(); // Execute operation

                // Wait for the JavaScript callback to complete before executing the corresponding callback
                CallbackResult data = null;
                while (!completedOperations.TryGetValue(identifier, out data))
                {
                    yield return null; // Wait for callback completion
                }

                // Execute the corresponding callback, passing data returned from JavaScript
                if (callback != null && data != null)
                {
                    InvokeCallback(callback, data); // Call the callback based on the specific type
                }

                // Remove processed operation
                completedOperations.Remove(identifier);
            }
            else
            {
                // If the queue is empty, pause for a frame and then check the queue again
                yield return null;
            }
        }
    }

    // Invoke generic callback
    private void InvokeCallback(Delegate callback, CallbackResult result)
    {
        // Handle based on the type of callback
        if (callback is Action<byte[]> byteArrayCallback)
        {
            byte[] byteArray = null;
            if(result.message != null) {
                byteArray = Convert.FromBase64String(result.message);
            }
            byteArrayCallback.Invoke(byteArray);
        }
        else if (callback is Action<string> stringCallback)
        {
            stringCallback.Invoke(result.message);
        }
        else if (callback is Action<bool> boolCallback)
        {
            boolCallback.Invoke(result.success);
        }
    }
}