mergeInto(LibraryManager.library, {
    OpenDatabase: function(dbName, version, identifier, gameObjectName, callbackMethod) {
        var dbNameStr = UTF8ToString(dbName);    
        var versionNum = version;                
        var idInt = identifier; 
        var goName = UTF8ToString(gameObjectName); 
        var callback = UTF8ToString(callbackMethod); 

        var request = indexedDB.open(dbNameStr, versionNum);

        request.onupgradeneeded = function(event) {
            var db = event.target.result;
            if (!db.objectStoreNames.contains('fileData')) {
                var store = db.createObjectStore('fileData', { keyPath: 'id' });
                store.createIndex('idIndex', 'id', { unique: true });
            }
        };

        request.onsuccess = function(event) {
            var db = event.target.result;
            Module['dbInstance'] = db;  
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: true, message: "Database opened" }));
        };

        request.onerror = function(event) {
            console.error("IndexedDB error:", event);
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Error opening database" }));
        };
    },

    WriteData: function(key, value, identifier, gameObjectName, callbackMethod) {
        var keyStr = UTF8ToString(key);
        var valueStr = UTF8ToString(value);
        var idInt = identifier; 
        var goName = UTF8ToString(gameObjectName);
        var callback = UTF8ToString(callbackMethod);

        if (!Module['dbInstance']) {
            console.error("Database is not opened");
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Database not opened" }));
            return;
        }

        var transaction = Module['dbInstance'].transaction(['fileData'], 'readwrite');
        var store = transaction.objectStore('fileData');
        var data = { id: keyStr, value: valueStr };
        var putRequest = store.put(data);

        putRequest.onsuccess = function() {
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: true, message: "Data saved successfully" }));
        };

        putRequest.onerror = function(event) {
            console.error("Error saving data", event);
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Error saving data" }));
        };
    },

    ReadData: function(key, identifier, gameObjectName, callbackMethod) {
        var keyStr = UTF8ToString(key);
        var idInt = identifier; 
        var goName = UTF8ToString(gameObjectName);
        var callback = UTF8ToString(callbackMethod);

        if (!Module['dbInstance']) {
            console.error("Database is not opened");
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Database not opened" }));
            return;
        }

        var transaction = Module['dbInstance'].transaction(['fileData'], 'readonly');
        var store = transaction.objectStore('fileData');
        var getRequest = store.get(keyStr);

        getRequest.onsuccess = function(event) {
            var result = event.target.result;
            if (result) {
                Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: true, message: result.value }));
            } else {
                Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "No data found" }));
            }
        };

        getRequest.onerror = function(event) {
            console.error("Error loading data", event);
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Error loading data" }));
        };
    },

    DeleteData: function(key, identifier, gameObjectName, callbackMethod) {
        var keyStr = UTF8ToString(key);
        var idInt = identifier; 
        var goName = UTF8ToString(gameObjectName);    
        var callback = UTF8ToString(callbackMethod);  

        if (!Module['dbInstance']) {
            console.error("Database is not opened");
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Database not opened" }));
            return;
        }

        var transaction = Module['dbInstance'].transaction(['fileData'], 'readwrite');
        var store = transaction.objectStore('fileData');
        var deleteRequest = store.delete(keyStr);  

        deleteRequest.onsuccess = function(event) {
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: true, message: "Data deleted successfully" }));
        };

        deleteRequest.onerror = function(event) {
            console.error("Error deleting file", event);
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Error deleting file" }));
        };
    },

    FindData: function(key, identifier, gameObjectName, callbackMethod) {
        var keyStr = UTF8ToString(key);
        var idInt = identifier; 
        var goName = UTF8ToString(gameObjectName);   
        var callback = UTF8ToString(callbackMethod); 
    
        if (!Module['dbInstance']) {
            console.error("Database is not opened");
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Database not opened" }));
            return;
        }
    
        var transaction = Module['dbInstance'].transaction(['fileData'], 'readonly');
        var store = transaction.objectStore('fileData');
        var countRequest = store.count(keyStr);
        countRequest.onsuccess = function(event) {
            var exists = event.target.result > 0;
            if (exists) {
                Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: true, message: "Data exists" }));
            } else {
                Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, message: "No data found" }));
            }
        };
    
        countRequest.onerror = function(event) {
            console.error("Error checking file", event);
            Module['SendMessage'](goName, callback, JSON.stringify({ id: idInt, success: false, error: "Error checking file" }));
        };
    },

    SyncIndexedDB: function () {
        FS.syncfs(false, function (err) {
            if (err) console.log("syncfs error: " + err);
        });
    }
});