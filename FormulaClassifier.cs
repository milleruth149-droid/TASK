using System;
using System.Text.RegularExpressions;

namespace DynamicFormulaCalculator
{
    public static class FormulaClassifier
    {//מחלקת עזר בלבד 
        public static bool IsFastCompatible(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return true;

            formula = formula.ToLower();

            // כל מה ש-Compute לא תומך בו
            string[] slowKeywords =
            {
                "sqrt", "power", "pow", "log", "exp",
                "sin", "cos", "tan"
            };

            foreach (var key in slowKeywords)
            {
                if (formula.Contains(key))
                    return false;
            }

            // בדיקה בסיסית שאין פונקציות מורכבות
            if (Regex.IsMatch(formula, @"[a-zA-Z_]+\s*\("))
                return false;

            return true;
        }
    }
}