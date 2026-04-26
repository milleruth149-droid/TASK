
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using NCalc;

namespace DynamicFormulaCalculator
{
    public static class NCalcEngine
    {
        //  פונקציות דינמיות שיטה איטית 
        private static readonly Dictionary<string, Func<double[], double>> _functions =
            new Dictionary<string, Func<double[], double>>(StringComparer.OrdinalIgnoreCase)
            {
                ["sqrt"] = args => Math.Sqrt(args[0]),
                ["power"] = args => Math.Pow(args[0], args[1]),
                ["pow"] = args => Math.Pow(args[0], args[1]),
                ["log"] = args => Math.Log(args[0]),
                ["abs"] = args => Math.Abs(args[0]),
                ["exp"] = args => Math.Exp(args[0]),
                ["sin"] = args => Math.Sin(args[0]),
                ["cos"] = args => Math.Cos(args[0]),
                ["tan"] = args => Math.Tan(args[0]),
                ["round"] = args => Math.Round(args[0], (int)args[1])
            };

        public static object Evaluate(string formula, string tnai, string falseTargil, DataRow row)
        {
            string expression;

            // תנאי
            if (!string.IsNullOrEmpty(tnai))
                expression = $"if({tnai}, {formula}, {falseTargil})";
            else
                expression = formula;

            //  החלפת משתנים בצורה בטוחה (Regex)
            expression = ReplaceVariables(expression, row);

            Expression e = new Expression(expression);

            //  מנגנון פונקציות דינמי
            e.EvaluateFunction += (name, args) =>
            {
                if (_functions.TryGetValue(name, out var func))
                {
                    double[] parameters = new double[args.Parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                        parameters[i] = Convert.ToDouble(args.Parameters[i].Evaluate());

                    args.Result = func(parameters);
                }
                else
                {
                    throw new Exception($"Function not supported: {name}");
                }
            };

            return e.Evaluate();
        }

        private static string ReplaceVariables(string expression, DataRow row)
        {
            return Regex.Replace(expression, @"\b[a-d]\b", match =>
            {
                string col = match.Value;
                return Convert.ToDouble(row[col]).ToString(System.Globalization.CultureInfo.InvariantCulture);
            });
        }
    }
}