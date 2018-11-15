using System;
using System.Collections.Generic;
using System.Web;

namespace deeper.db
{
    public class parameter
    {
        public enum typeParameter { none, integer, text, real };

        typeParameter _type = typeParameter.none;
        string _name = "";
        object _value = null;

        public parameter(string name) { _name = name; }
        public parameter(string name, object value) { _name = name; _value = value; }
        public parameter(string name, int intValue) { _name = name; IntValue = intValue; }
        public parameter(string name, string textValue) { _name = name; TextValue = textValue; }
        public parameter(string name, double realValue) { _name = name; RealValue = realValue; }

        public string Name { get { return _name; } set { _name = value; } }
        public object Value { get { return _value; } }
        public int IntValue { get { return (int)_value; } set { _type = typeParameter.integer; _value = value; } }
        public string TextValue { get { return (string)_value; } set { _type = typeParameter.text; _value = value; } }
        public double RealValue { get { return (double)_value; } set { _type = typeParameter.real; _value = value; } }
        public bool IsNull { get { return _type == typeParameter.none; } }
        public typeParameter Type { get { return _type; } }
    }
}