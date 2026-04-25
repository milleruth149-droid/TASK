// שיטה מהירה שמכירה רק נוסחאות פשוטות 

using System;
using System.Data;
using System.Globalization;

namespace DynamicFormulaCalculator
{
    public static class FastComputeEngine
    {
        private static readonly DataTable _helper = new DataTable();

        public static object Evaluate(string expr, DataRow row)
        {
            string finalExpr = expr;

            foreach (DataColumn col in row.Table.Columns)
            {
                if (row[col] != DBNull.Value)
                {
                    string val = Convert.ToDouble(row[col]).ToString(CultureInfo.InvariantCulture);
                    finalExpr = finalExpr.Replace(col.ColumnName.ToLower(), val);
                }
            }

            return _helper.Compute(finalExpr, "");
        }
    }
}