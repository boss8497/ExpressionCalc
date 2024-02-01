var data = new Data("Portpolio", 2, 2);

var number = new Number("10 + power((Level + 1) * 2, 2)");
var resultValue = number.Eval(data);

Console.WriteLine($"{resultValue}");