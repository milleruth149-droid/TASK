
using System;
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using DynamicFormulaCalculator;



// חיבור למסד
string connectionString =
    @"Server=(localdb)\MSSQLLocalDB;Database=TESTRUTH;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

Console.WriteLine("--- HYBRID ENGINE v2 ---");

using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();


    //מכניס לטבלה את כל הנוסחאות
    DataTable formulasTable = new DataTable();
    new SqlDataAdapter("SELECT * FROM t_targil", connection).Fill(formulasTable);
    //מכניס לטבלה את כל הנתונים
    DataTable dataTable = new DataTable();
    new SqlDataAdapter("SELECT * FROM t_data", connection).Fill(dataTable);

    
    DataTable resultsBuffer = new DataTable();
    resultsBuffer.Columns.Add("data_id", typeof(int));
    resultsBuffer.Columns.Add("targil_id", typeof(int));
    resultsBuffer.Columns.Add("method", typeof(string));
    resultsBuffer.Columns.Add("result", typeof(double));



    //מעבר על כל הנוסחאות 
    foreach (DataRow formulaRow in formulasTable.Rows)
    {
        int targilid = (int)formulaRow["targil_id"];
        string formula = formulaRow["targil"].ToString();
        string tnai = formulaRow["tnai"]?.ToString();
        string falseT = formulaRow["targil_false"]?.ToString();

        Console.WriteLine($"\nFormula {targilid}");

        Stopwatch sw = Stopwatch.StartNew();

        bool useFast = FormulaClassifier.IsFastCompatible(formula);

        string prepared = formula;

        if (!string.IsNullOrEmpty(tnai))
        {
            prepared = $"IIF({tnai}, {formula}, {falseT})";
        }



        // מעדכן  את השיטה 
        string methodType = useFast ? ".NET_FAST" : ".NET_SLOW";

        foreach (DataRow row in dataTable.Rows)
        {
            object result;


            // אם הצליח את המהירה הולך אליה
            if (useFast)
            {
                result = FastComputeEngine.Evaluate(prepared, row);
            }

            //הולך לאיטית
            else
            {
                result = NCalcEngine.Evaluate(formula, tnai, falseT, row);
            }



            resultsBuffer.Rows.Add(
                (int)row["data_id"],
                targilid,
                methodType,
                Convert.ToDouble(result)
            );
        }

        using (SqlBulkCopy bulk = new SqlBulkCopy(connection))
        {
            bulk.DestinationTableName = "t_results";

            bulk.ColumnMappings.Add("data_id", "data_id");
            bulk.ColumnMappings.Add("targil_id", "targil_id");
            bulk.ColumnMappings.Add("method", "method");
            bulk.ColumnMappings.Add("result", "result");

            bulk.WriteToServer(resultsBuffer);
        }

        resultsBuffer.Clear();

        sw.Stop();

        
       
        SaveLog(connection, targilid, sw.Elapsed.TotalSeconds, methodType);


        Console.WriteLine($"Done in {sw.Elapsed.TotalSeconds:F2}s ({methodType})");
    }
}

Console.WriteLine("Finished");
Console.ReadKey();

 

// שמירה בטבלת הלוג
void SaveLog(SqlConnection conn, int targilId, double time, string status)
{
    string query = "INSERT INTO t_log (targil_id, method, run_time) VALUES (@id, @method, @time)";
    using (SqlCommand cmd = new SqlCommand(query, conn))
    {
        cmd.Parameters.AddWithValue("@id", targilId);
        cmd.Parameters.AddWithValue("@method", status);
        cmd.Parameters.AddWithValue("@time", time);
        cmd.ExecuteNonQuery();
    }
}


// בסוף שומר במקום אחר יעיל יותר 
//void SaveResult(SqlConnection conn, int dataId, int targilId, double result, string method)
//{
//    // הערה: למען המהירות, בעתיד כדאי להשתמש ב-SqlBulkCopy, כרגע זה INSERT פשוט
//    string query = "INSERT INTO t_results (data_id, targil_id, method, result) VALUES (@dId, @tId, @method, @res)";
//    using (SqlCommand cmd = new SqlCommand(query, conn))
//    {
//        cmd.Parameters.AddWithValue("@dId", dataId);
//        cmd.Parameters.AddWithValue("@tId", targilId);
//        cmd.Parameters.AddWithValue("@method", method);
//        cmd.Parameters.AddWithValue("@res", result);
//        cmd.ExecuteNonQuery();
//    }
//}