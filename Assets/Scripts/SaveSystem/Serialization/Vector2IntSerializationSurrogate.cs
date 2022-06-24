using System.Runtime.Serialization;
using UnityEngine;

public class Vector2IntSerializationSurrogate : ISerializationSurrogate {
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
        Vector2Int v2Int = (Vector2Int)obj;
        info.AddValue("x", v2Int.x);
        info.AddValue("y", v2Int.y);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
        Vector2Int v2Int = (Vector2Int)obj;
        v2Int.x = (int)info.GetValue("x", typeof(int));
        v2Int.y = (int)info.GetValue("y", typeof(int));
        obj = v2Int;
        return obj;
    }
}
