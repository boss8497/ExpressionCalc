using System.Text.RegularExpressions;

public class ExpressionCalc {
    private string _expression;
    public string Expression {
        get => _expression;
        set {
            bindValues.Clear();
            
            _expression = value.Replace(" ", "");
            _expression = _expression.ToLowerInvariant();
            var match = StringRegex.Match(_expression);
            while (match.Success) {
                if (methodNames.Any(a => a.Equals(match.Value)) == false) {
                    bindValues[match.Value] = 0;
                }
                match = match.NextMatch();
            }
        }
    }
    private Dictionary<string, double> bindValues       = new();
    public List<string>                BindName         => bindValues.Select(s => s.Key).ToList();
    private char[]                     operatorChar     = { OpenParenthesis, CloseParenthesis, '+', '-', '/', '^', '*' };
    private string[]                   methodNames      = { "power", "sqrt", "floor", "round", "ceiling" };
    
    private static readonly char       ExponentChar     = 'E';
    private static readonly char       OpenParenthesis  = '(';
    private static readonly char       CloseParenthesis = ')';
    private static readonly Regex      StringRegex      = new("[a-zA-Z]+");
    private ExpressionCalc() {
    }

    public ExpressionCalc(string expr) {
        Expression = expr.Replace(" ", "");
    }

    public void Bind(string key, double value) {
        var lowKey = key.ToLowerInvariant();
        if (bindValues.ContainsKey(lowKey)) {
            bindValues[lowKey] = value;
        }
    }

    public double Eval() {
        var lowerExpression = Expression.ToLowerInvariant();

        foreach (var expreValue in bindValues) {
            lowerExpression = lowerExpression.Replace(expreValue.Key, $"{expreValue.Value}");
        }

        lowerExpression = CalcMethod(lowerExpression);
        return Calc(lowerExpression);
    }

    private string CalcMethod(string lowerExpression) {
        var startAlphaIndex = IndexOfAlpha(in lowerExpression, 0);
        if (startAlphaIndex == -1) {
            return string.Empty;
        }

        var parenthesisStart = GetFindCharIndex(in lowerExpression, startAlphaIndex, OpenParenthesis);
        var parenthesisEnd = GetCloseParenthesisIndex(in lowerExpression, parenthesisStart);
        var methodName = lowerExpression.Substring(startAlphaIndex, parenthesisStart - startAlphaIndex);
        
        var methodExpStart = parenthesisStart + 1;
        var methodExpression = lowerExpression.Substring(methodExpStart, parenthesisEnd - methodExpStart);

        if (methodExpression.Any(a => Char.IsLetter(a) && a != ExponentChar)) {
            methodExpression = CalcMethod(methodExpression);
        }
            
        var resultValue = GetMethodValue(methodName, methodExpression);
        var replaceStr = lowerExpression.Substring(startAlphaIndex, parenthesisEnd + 1 - startAlphaIndex);
        lowerExpression = lowerExpression.Replace(replaceStr, resultValue);

        return lowerExpression;
    }

    private string GetMethodValue(string methodName, string methodExpression) {
        switch (methodName) {
            case "power": {
                if (methodExpression.Contains(',')) {
                    var currentIndex = methodExpression.IndexOf(',');
                    var firExpression = methodExpression.Substring(0, currentIndex);
                    currentIndex += 1;
                    var secondExpr = methodExpression.Substring(currentIndex, methodExpression.Length - currentIndex);
                    return $"{Math.Pow(Calc(firExpression), Calc(secondExpr))}";
                }
                return $"{Math.Pow(Calc(methodExpression), 2)}";
            }
            case "sqrt": {
                return $"{Math.Sqrt(Calc(methodExpression))}";
            }
            case "floor": {
                return $"{Math.Floor(Calc(methodExpression))}";
            }
            case "ceiling": {
                return $"{Math.Ceiling(Calc(methodExpression))}";
            }
            case "round": {
                if (methodExpression.Contains(',')) {
                    var currentIndex = methodExpression.IndexOf(',');
                    var firExpression = methodExpression.Substring(0, currentIndex);
                    currentIndex += 1;
                    var secondExpr = methodExpression.Substring(currentIndex, methodExpression.Length - currentIndex);
                    return $"{Math.Round(Calc(firExpression), (int)Calc(secondExpr))}";
                }
                return $"{Math.Round(Calc(methodExpression))}";
            }
            default:
                throw new InvalidOperationException("Unknown Method : " + methodName);
        }
    }

    private int GetCloseParenthesisIndex(in string str, int startIndex) {
        var count = 0;
        for (int i = startIndex; i < str.Length; ++i) {
            var currentChar = str[i];
            if (currentChar == CloseParenthesis) --count;
            else if (currentChar == OpenParenthesis) ++count;

            if (count <= 0) {
                return i;
            }
        }
        return -1;
    }

    private int IndexOfAlpha(in string str, int startIndex) {
        for (int i = startIndex; i < str.Length; ++i) {
            var currentChar = str[i];
            if (Char.IsNumber(currentChar) || currentChar == ExponentChar || currentChar == '.' ||
                operatorChar.Any(r => r == currentChar)) continue;
            return i;
        }

        return -1;
    }

    private int GetFindCharIndex(in string str, int startIndex, char find) {
        for (int i = startIndex; i < str.Length; ++i) {
            var currentChar = str[i];
            if (currentChar == find)
                return i;
        }

        return -1;
    }

    private void GetToken(in string exp, out List<string> tokens) {
        tokens = new List<string>();
        if (exp.Any(a => operatorChar.Any(r => r == a)) == false) {
            tokens.Add(exp);
        }

        int startIndex = 0;
        var subString = string.Empty;
        for (int i = 0; i < exp.Length; ++i) {
            var currentChar = exp[i];

            var endLenght = 0;
            if (operatorChar.Any(r => r == currentChar) && exp[i - 1 <= 0 ? i : i - 1] != ExponentChar) {
                endLenght = i - startIndex;
                subString = exp.Substring(startIndex, endLenght == 0 ? 1 : endLenght);
                tokens.Add(subString);
                if (endLenght != 0) {
                    subString = exp.Substring(i, 1);
                    tokens.Add(subString);
                }
                startIndex = i + 1;
            }
            else if (exp.Length == i + 1 && exp.Length - startIndex > 0) {
                endLenght = i - startIndex;
                subString = exp.Substring(startIndex, endLenght + 1);
                tokens.Add(subString);
                startIndex = i + 1;
            }
        }
    }

    private double Calc(string exp) {
        var stackOperator = new Stack<string>();
        var stackValue    = new Stack<double>();
        
        GetToken(exp, out var tokens);

        foreach (var token in tokens) {
            if (operatorChar.Any(a => token.Contains(a))) {
                if (stackOperator.Count == 0) {
                    stackOperator.Push(token);
                }
                else {
                    switch (token) {
                        case "(":
                            stackOperator.Push(token);
                            break;
                        
                        case ")":
                            while (stackOperator.Count > 0) {
                                var oper = stackOperator.Pop();
                                if ("(".Equals(oper)) break;
                                var val = Calc(stackValue.Pop(), stackValue.Pop(), oper);
                                stackValue.Push(val);
                            }
                            break;
                        
                        default:
                            var previousPriority = OperatorPriority(stackOperator.Peek());
                            var currentPriority = OperatorPriority(token);
                            if (previousPriority >= currentPriority) {
                                var value1 = stackValue.Pop();
                                var value2 = stackValue.Pop();
                                stackValue.Push(Calc(value1, value2, stackOperator.Pop()));
                                stackOperator.Push(token);
                            }
                            else {
                                stackOperator.Push(token);
                            }
                            break;
                    }
                    
                }
            }
            else {
                stackValue.Push(Convert.ToDouble(token));
            }
        }

        while (stackOperator.Count > 0) {
            var value = Calc(stackValue.Pop(), stackValue.Pop(), stackOperator.Pop());
            stackValue.Push(value);
        }

        return stackValue.Pop();
    }

    private double Calc(double val1, double val2, string oper) {
        if (oper == "+") return val1 + val2;
        if (oper == "-") return val2 - val1;
        if (oper == "*") return val1 * val2;
        if (oper == "*") return val1 * val2;
        if (oper == "/") return val2 / val1;
        if (oper == "^") return Math.Pow(val2, val1);
        throw new InvalidOperationException("Unknown Operation : " + oper);
    }

    private int OperatorPriority(string oper) {
        if (oper == "(" || oper == ")") return 0;
        if (oper == "-" || oper == "+") return 1;
        if (oper == "*" || oper == "/" || oper == "^") return 2;
        throw new InvalidOperationException("Unknown Operation " + oper);
    }
}