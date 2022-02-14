using UnityEngine;

    public enum ModifierOperationType
    { 
        Additive = 100,
        Multiplicative = 200,
        Override = 300,
    }
    public class StatModifier
    { 
        private object _source;
        private float _value;
        private ModifierOperationType _type;
        private int _order;
        private bool _useCustomOrder;

        public object Source { get { return _source; } set { _source = value; } }
        public float Value { get { return _value; } set { _value = value; } }
        public ModifierOperationType Type { get { return _type; } set { _type = value; } }
        public int Order { get { return _order; } set { _order = value; } }
        public bool UseCustomOrder { get { return _useCustomOrder; } set { _useCustomOrder = value; } }

        public StatModifier(ModifierOperationType _type, float _value, object _source, int _order, bool _useCustomOrder)
        {
            this._type = _type;
            this._value = _value;
            this._source = _source;
            this._order = _order;
            this._useCustomOrder = _useCustomOrder;
        }

        /* 
         * Requires Value and Type. Calls the "Main" constructor and sets Order and Source to their default values: (int)type and null, respectively.
         * 
         * */
        public StatModifier(ModifierOperationType type, float value) : this(type, value, null, (int)type, false) { }

        // Requires Value, Type and Order. Sets Source to its default value: null
        public StatModifier(ModifierOperationType type, float value, int order) : this(type, value, null, order, true) { }

        // Requires Value, Type and Source. Sets Order to its default value: (int)Type
        public StatModifier(ModifierOperationType type, float value, object source) : this(type, value, null, (int)type, false) { }
    }