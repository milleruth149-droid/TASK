import pandas as pd
import pyodbc
import time
import numpy as np
import math


# נסי קודם את האופציה הזו (הכי נפוצה):
conn_str = (
    r"DRIVER={ODBC Driver 17 for SQL Server};"
    r"SERVER=(localdb)\MSSQLLocalDB;"
    r"DATABASE=TESTRUTH;"
    r"Trusted_Connection=yes;"
)

# אם זה עדיין נותן שגיאה, תחליפי רק את שורת ה-SERVER לשם הדינמי שקיבלת:
# r"SERVER=np:\\.\pipe\LOCALDB#5999B6A6\tsql\query;"


def run_python_engine():
    try:
        # התחברות למסד
        conn = pyodbc.connect(conn_str)
        print("Connected to database...")

        # שלב א': שליפת נתונים לתוך DataFrame (ה"טבלה" של פיטון)
        formulas_df = pd.read_sql("SELECT * FROM t_targil", conn)
        data_df = pd.read_sql("SELECT * FROM t_data", conn)

        results_list = []

        # שלב ב': מעבר על כל נוסחה
        for _, f_row in formulas_df.iterrows():
            targil_id = f_row['targil_id']
            formula = f_row['targil']
            tnai = f_row.get('tnai')
            targil_false = f_row.get('targil_false')

            start_time = time.time()
            print(f"Processing Formula {targil_id}...")

            # שלב ג': חישוב דינמי באמצעות eval
            # פיטון יודעת להריץ eval על כל ה-DataFrame בבת אחת!
            try:
                # אם יש תנאי, נשתמש ב-where של pandas (מקביל ל-IIF)
                if pd.notna(tnai) and str(tnai).lower() != 'null':
                    # חישוב התנאי, התוצאה החיובית והשלילית
                    mask = data_df.eval(tnai)
                    res_true = data_df.eval(formula)
                    res_false = data_df.eval(targil_false)

                    # מיזוג תוצאות לפי התנאי
                    current_results = res_true.where(mask, res_false)
                else:
                    # חישוב נוסחה פשוטה
                    current_results = data_df.eval(formula)

                # שלב ד': הכנת התוצאות לשמירה
                temp_df = pd.DataFrame({
                    'data_id': data_df['data_id'],
                    'targil_id': targil_id,
                    'method': 'Python',
                    'result': current_results
                })
                results_list.append(temp_df)

            except Exception as e:
                print(f"Error calculating formula {targil_id}: {e}")

            end_time = time.time()
            save_log(conn, targil_id, end_time - start_time, "Python")

        # שלב ה': שמירה מאסיבית חזרה למסד
        if results_list:
            final_results = pd.concat(results_list)
            bulk_insert(conn, final_results)

        conn.close()
        print("All processing complete.")

    except Exception as ex:
        print(f"Critical Error: {ex}")


def bulk_insert(conn, df):
    cursor = conn.cursor()
    # הכנת השאילתה
    insert_sql = "INSERT INTO t_results (data_id, targil_id, method, result) VALUES (?, ?, ?, ?)"
    # המרת ה-DataFrame לרשימה של טאפלים ושליחה ב-Bulk
    values = df.values.tolist()
    cursor.executemany(insert_sql, values)
    conn.commit()


def save_log(conn, targil_id, runtime, status):
    cursor = conn.cursor()
    cursor.execute("INSERT INTO t_log (targil_id, method, run_time) VALUES (?, ?, ?)",
                   (targil_id, status, runtime))
    conn.commit()


if __name__ == "__main__":
    run_python_engine()