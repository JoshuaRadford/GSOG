[System.Serializable]
public class StatMod {
    public enum ModType {
        Additive = 100,
        Multiplicative = 200,
        Override = 300,
    }

    public object Source { get; set; }
    public float Value { get; set; }
    public ModType Type { get; set; }

    public StatMod(ModType modType, float value, object source) {
        Type = modType;
        Value = value;
        Source = source;
    }

    /* 
        * Requires Value and Type. Calls the "Main" constructor and sets Order and Source to their default values: (int)type and null, respectively.
        * 
        * */
    public StatMod(ModType type, float value) : this(type, value, null) { }
}