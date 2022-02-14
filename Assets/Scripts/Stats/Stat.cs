using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

    public class Stat
    {
        protected string _name;
        protected float _baseValue;
        protected float _modifiedValue;
        public virtual string Name { get { return _name; } set { _name = value; } }
        public virtual float BaseValue { get { return _baseValue; } set { _baseValue = value; } }
        public virtual float ModifiedValue
        {
            get
            {
                _modifiedValue = ApplyModifiers(_baseValue, _baseValueModifiers);
                return _modifiedValue;
            }
        }
        protected List<StatModifier> _baseValueModifiers;
        public virtual List<StatModifier> BaseValueModifiers { get { return _baseValueModifiers; } }

        public Stat(string name, float baseValue = 5f)
        {        
            _name = name;
            _baseValue = baseValue;
            _baseValueModifiers = new List<StatModifier>();
        }

        public void AddModifier(StatModifier modToAdd, List<StatModifier> addTo)
        {
            addTo.Add(modToAdd);
        }

        public void AddMultipleModifiers(List<StatModifier> modsToAdd, List<StatModifier> addTo)
        {
            foreach(StatModifier mod in modsToAdd)
            {
                addTo.Add(mod);
            }
        }

        public bool RemoveModifier(StatModifier modToRemove, List<StatModifier> removeFrom)
        {
            if(removeFrom.Remove(modToRemove))
            {
                return true;
            }
            return false;
        }

        public bool RemoveModifierFromSource(object source, List<StatModifier> removeFrom)
        {
            bool removed = false;
            for(int i = removeFrom.Count;  i >= 0; i--)
            {
                if(removeFrom[i] == source)
                {
                    removed = true;
                    removeFrom.RemoveAt(i);
                }
            }
            return removed;
        }

        protected float ApplyModifiers(float _base, List<StatModifier> statModifiers)
        {
            float finalValue = _base;

            statModifiers.Sort((x, y) => x.Type.CompareTo(y.Type));

            for(int i = 0; i < statModifiers.Count; i++)
            {
                StatModifier modifier = statModifiers[i];
                if(modifier.Type == ModifierOperationType.Additive)
                {
                    finalValue += modifier.Value;
                }
                else if(modifier.Type != ModifierOperationType.Multiplicative)
                {
                    finalValue *= modifier.Value;
                }
            }

            return finalValue;
        }
    }