using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializationManager {
    public const string SAVESFOLDERNAME = "saves";


    public static bool Save(string saveName, object saveData) {
        BinaryFormatter formatter = GetBinaryFormatter();

        if (!Directory.Exists(Application.persistentDataPath + $"/{SAVESFOLDERNAME}")) {
            Directory.CreateDirectory(Application.persistentDataPath + $"/{SAVESFOLDERNAME}");
        }

        string path = (Application.persistentDataPath + $"/{SAVESFOLDERNAME}/{saveName}.save");

        FileStream file = File.Create(path);

        formatter.Serialize(file, saveData);

        file.Close();

        return true;
    }

    public static object Load(string path) {
        if (!File.Exists(path)) {
            return null;
        }

        BinaryFormatter formatter = GetBinaryFormatter();

        FileStream file = File.Open(path, FileMode.Open);

        try {
            object save = formatter.Deserialize(file);
            file.Close();
            return save;
        }
        catch {
            Debug.LogErrorFormat($"Failed to load file at {path}");
            file.Close();
            return null;
        }
    }

    public static BinaryFormatter GetBinaryFormatter() {
        BinaryFormatter formatter = new BinaryFormatter();

        SurrogateSelector selector = new SurrogateSelector();

        // Add surrogates for normally un-serializable data types
        Vector2IntSerializationSurrogate vector2IntSurrogate = new Vector2IntSerializationSurrogate();
        Vector3SerializationSurrogate vector3Surrogate = new Vector3SerializationSurrogate();
        QuaternionSerializationSurrogate quatSurrogate = new QuaternionSerializationSurrogate();

        selector.AddSurrogate(typeof(Vector2Int), new StreamingContext(StreamingContextStates.All), vector2IntSurrogate);
        selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
        selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quatSurrogate);

        formatter.SurrogateSelector = selector;

        return formatter;
    }
}
