///// שיטה מהירה שכל החישוב נעשה במסד נתונים ומהפרויקט רק שולחים לשם 
///// חשבתי שזה פחות מתאים כי רצו במשימה דווקא שה
///// .NET ינהל את זה 
//using System;
//using System.Data;
//using System.Diagnostics;
//using Microsoft.Data.SqlClient;

//// הגדרת משתנים
//string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=TESTRUTH;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";


//Console.WriteLine("--- Application Started (Top-Level Mode) ---");

//using (SqlConnection connection = new SqlConnection(connectionString))
//{
//    try
//    {
//        connection.Open();
//        Console.WriteLine("--- Database Engine Mode (Extreme Speed) ---");

//        // 1. שליפת הנוסחאות
//        DataTable formulasTable = new DataTable();
//        using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM t_targil", connection))
//        {
//            adapter.Fill(formulasTable);
//        }

//        foreach (DataRow formulaRow in formulasTable.Rows)
//        {
//            int targilId = Convert.ToInt32(formulaRow["targil_id"]);
//            string formula = formulaRow["targil"]?.ToString() ?? "";
//            string tnai = formulaRow["tnai"]?.ToString() ?? "";
//            string falseTargil = formulaRow["targil_false"]?.ToString() ?? "";

//            Console.WriteLine($"Processing Formula {targilId}: {formula}...");
//            Stopwatch sw = Stopwatch.StartNew();

//            // 2. בניית השאילתה בפורמט SQL
//            string sqlExpression = formula;
//            if (!string.IsNullOrEmpty(tnai) && tnai.ToLower() != "null")
//            {
//                sqlExpression = $"CASE WHEN {tnai} THEN {formula} ELSE {falseTargil} END";
//            }

//            // 3. הרצה ישירה ב-SQL
//            string insertSql = $@"
//                INSERT INTO t_results (data_id, targil_id, method, result)
//                SELECT data_id, {targilId}, 'SQL_Engine', {sqlExpression}
//                FROM t_data";

//            using (SqlCommand cmd = new SqlCommand(insertSql, connection))
//            {
//                cmd.CommandTimeout = 120;
//                int rowsAffected = cmd.ExecuteNonQuery();
//                Console.WriteLine($"Successfully processed {rowsAffected} rows.");
//            }

//            sw.Stop();

//            // 4. רישום לוג ביצועים
//            SaveLog(connection, targilId, sw.Elapsed.TotalSeconds, ".NET");
//            Console.WriteLine($"Formula {targilId} Finished. Time: {sw.Elapsed.TotalSeconds:F4}s");
//        }
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Critical Error: {ex.Message}");
//    }
//}

//Console.WriteLine("\nAll processing complete. Press any key...");
//Console.ReadKey();

//// פונקציית עזר - מחוץ לקוד הראשי
//void SaveLog(SqlConnection conn, int targilId, double time, string status)
//{
//    string query = "INSERT INTO t_log (targil_id, method, run_time) VALUES (@id, @method, @time)";
//    using (SqlCommand cmd = new SqlCommand(query, conn))
//    {
//        cmd.Parameters.AddWithValue("@id", targilId);
//        cmd.Parameters.AddWithValue("@method", status);
//        cmd.Parameters.AddWithValue("@time", time);
//        cmd.ExecuteNonQuery();
//    }
//}


