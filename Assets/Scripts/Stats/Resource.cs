using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class Resource : Stat
    {
        private float _baseMaxValue;
        private float _modifiedMaxValue;

        public float BaseMaxValue { get { return _baseMaxValue; } set { _baseMaxValue = value; } }
        public float ModifiedMaxValue
        {
            get
            {
                _modifiedMaxValue = ApplyModifiers(BaseMaxValue, MaxValueModifiers);
                return _modifiedMaxValue;
            }
        }
        public override float ModifiedValue
        {
            get
            {
                _modifiedValue = Mathf.Min(ApplyModifiers(BaseValue, BaseValueModifiers), ModifiedMaxValue);
                return _modifiedValue;
            }
        }

        public List<StatModifier> MaxValueModifiers { get { return _maxValueModifiers; } }

        protected List<StatModifier> _maxValueModifiers;

        public Resource(string name, float baseValue = 5f, float baseMaxValue = 10f) : base(name, baseValue)
        {
            _baseMaxValue = BaseMaxValue;
            _maxValueModifiers = new List<StatModifier>();
        }

        private float CapToMax()
        {
            float finalValue = ApplyModifiers(_baseValue, _baseValueModifiers);
            if (_baseMaxValue >= 0)
            {
                finalValue = Mathf.Min(finalValue, ModifiedMaxValue);
            }

            return finalValue;
        }

}