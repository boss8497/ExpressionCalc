using System;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public class Number {
    public string expression;
    
    public Number() { }
    
    public Number(string _expression) {
        expression = _expression;
    }
    
    public double Eval<T>(T data) {
        var expr = new ExpressionCalc(expression);
        var classType = typeof(T);
        
        object GetFieldObject(T item, string fieldName) {
            var lowerFieldName = fieldName.ToLowerInvariant();
            foreach (var proper in classType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (proper.Name.ToLowerInvariant().Equals(lowerFieldName)) {
                    return proper.GetValue(item, null);
                }
            }

            foreach (var field in classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (field.Name.ToLowerInvariant().Equals(lowerFieldName)) {
                    return field.GetValue(item);
                }
            }
            throw new Exception("NotSet Binder");
        }
        
        foreach (var name in expr.BindName) {
            var resultValue = GetFieldObject(data, name);
            expr.Bind(name, Convert.ToDouble(resultValue));
        }
        
        return expr.Eval();
    }

    public void Eval_CustomMethod<T>(T item, Number num, string methodName) {
        var method = item.GetType().GetMethod(methodName);
        if (method == null) {
            return;
        }
        method.Invoke(item, new object[] { item, num});
    }
}