using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using NCalc; // ספריית החישובים המקצועית שהתקנו
namespace DynamicFormulaCalculator
{
    public class NCalcFormulaProcessor
    {
        //NCalc היא ספרייה חזקה מאוד,
        //אבל בגלל שהיא עושה "Parsing"
        //ניתוח של המחרוזת מחדש בכל שורה ושורה בתוך המיליון,
        //השיטה הזאת לוקחת הרבה יותר זמן .
        // השארתי אותה כקוד על מנת להראות שבדקתי את שתי השיטות ובחרתי את  השילוב של שתיהם  
      
            static void Main(string[] args)
            {

                string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=TESTRUTH;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        Console.WriteLine("Connecting to Database...");

                        //  שליפת טבלת הנוסחאות לזיכרון
                        DataTable formulasTable = new DataTable();
                        new SqlDataAdapter("SELECT * FROM t_targil", connection).Fill(formulasTable);

                        //  שליפת מיליון הרשומות לזיכרון (הנתונים לחישוב)
                        Console.WriteLine("Loading 1,000,000 records... Please wait.");
                        DataTable dataTable = new DataTable();
                        new SqlDataAdapter("SELECT a, b, c, d, data_id FROM t_data", connection).Fill(dataTable);

                        // לולאת הרצה על כל נוסחה בנפרד
                        foreach (DataRow formulaRow in formulasTable.Rows)
                        {
                            int targilId = Convert.ToInt32(formulaRow["targil_id"]);
                            string formula = formulaRow["targil"].ToString();
                            string tnai = formulaRow["tnai"]?.ToString();
                            string Targilfalse = formulaRow["targil_false"]?.ToString();

                            try
                            {
                                Console.WriteLine($"Calculating formula {targilId}: {formula}...");
                                Stopwatch sw = Stopwatch.StartNew();

                                // הרצת החישוב על כל שורה בנתונים
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    // Nclacהכנת הנוסחה החלפת אותיות בערכים והתאמה ל
                                    string finalExpression = PrepareForNCalc(formula, tnai, Targilfalse, row);

                                    // חישוב באמצעות NCalc
                                    Expression e = new Expression(finalExpression);
                                    var result = e.Evaluate();
                                }

                                sw.Stop();
                                // שמירת הצלחה בטבלת הלוגים
                                SaveLog(connection, targilId, sw.Elapsed.TotalSeconds, ".NET_NCalc_Success");
                                Console.WriteLine($"Done! Time: {sw.Elapsed.TotalSeconds}s");
                            }
                            catch (Exception ex)
                            {
                                // טיפול בשגיאה ספציפית לנוסחה אחת (התוכנית לא תעצור!)
                                Console.WriteLine($"Error in formula {targilId}: {ex.Message}");
                                SaveLog(connection, targilId, 0, ".NET_NCalc_Error: " + ex.Message.Substring(0, Math.Min(ex.Message.Length, 20)));
                            }
                        }
                        Console.WriteLine("Process finished. Press any key to exit.");
                        Console.ReadKey();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Critical Error: {ex.Message}");
                    }
                }
            }

            // פונקציה להכנת הנוסחה ל-NCalc (התאמת סינטקס והחלפת ערכים)
            static string PrepareForNCalc(string formula, string tnai, string Targilfalse, DataRow row)
            {
                string expression;

                // NCalc משתמש ב-if(condition, true, false) ולא ב-IIF
                if (!string.IsNullOrEmpty(tnai) && tnai.Trim() != "")
                {
                    expression = $"if({tnai}, {formula}, {Targilfalse})";
                }
                else
                {
                    expression = formula;
                }

                // החלפת המשתנים a,b,c,d בערכים האמיתיים מהשורה הנוכחית
                expression = expression.ToLower()
                                       .Replace("a", row["a"].ToString())
                                       .Replace("b", row["b"].ToString())
                                       .Replace("c", row["c"].ToString())
                                       .Replace("d", row["d"].ToString());

                return expression;
            }

            // פונקציית הלוג המתוקנת שמקבלת 4 פרמטרים
            public static void SaveLog(SqlConnection conn, int targilId, double time, string method)
            {
                string query = "INSERT INTO t_log (targil_id, method, run_time) VALUES (@id, @method, @time)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", targilId);
                    cmd.Parameters.AddWithValue("@method", method);
                    cmd.Parameters.AddWithValue("@time", time);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
