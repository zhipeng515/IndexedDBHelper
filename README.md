# IndexedDB Helper for Unity

This repository contains a Unity helper class for interacting with IndexedDB. It provides a set of methods to open a database, write data, read data, and delete data, and check if data exists, all while maintaining the order of operations through a queue mechanism.

## Features

- Open and manage an IndexedDB database from Unity.
- Write, read, delete, and check for data existence.
- Support for both string and byte array data types.
- Queue mechanism to ensure operations are processed in order.

## Installation

1. Clone the repository or download the files directly.
2. Include the `IndexedDBHelper.cs` C# script in your Unity project.
3. Place the `IndexedDB.jslib` file in the `Plugins/WebGL` directory of your Unity project to ensure it works correctly with WebGL builds.

## Dependencies

This project uses the [Newtonsoft.Json](https://www.newtonsoft.com/json) library for JSON serialization and deserialization. 

## Usage

### C# Code

To use the `IndexedDBHelper` class, create an instance and call the desired methods:

```csharp
IndexedDBHelper dbHelper = IndexedDBHelper.Create("MyDatabase", (success) => {
    if (success) {
        Debug.Log("Database opened successfully!");
    } else {
        Debug.Log("Failed to open database.");
    }
});

// Write data
dbHelper.WriteData("myFile", "Hello, World!", (success) => {
    Debug.Log(success ? "Data saved successfully!" : "Failed to save data.");
});

// Read data
dbHelper.ReadData("myFile", (data) => {
    Debug.Log("Data read: " + data);
});

// Delete data
dbHelper.DeleteData("myFile", (success) => {
    Debug.Log(success ? "Data deleted successfully!" : "Failed to delete data.");
});

// Check if data exists
dbHelper.FindData("myFile", (exists) => {
    Debug.Log(exists ? "Data exists." : "No data found.");
});
