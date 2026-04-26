import pandas as pd
import pyodbc
import time
import numpy as np


# =========================
# חיבור למסד נתונים
# =========================
conn_str = (
    r"DRIVER={ODBC Driver 17 for SQL Server};"
    r"SERVER=(localdb)\MSSQLLocalDB;"
    r"DATABASE=TESTRUTH;"
    r"Trusted_Connection=yes;"
)


# =========================
# פונקציות מותרות בנוסחאות
# =========================
FUNCTION_REGISTRY = {
    "SQRT": np.sqrt,
    "LOG": np.log,
    "ABS": np.abs,
    "POWER": np.power,
    "AVG": np.mean,
    "SUM": np.sum,
}


# =========================
# eval בטוח
# =========================
def safe_eval(expression, context):
    try:
        return eval(
            expression,
            {"__builtins__": None},
            {**FUNCTION_REGISTRY, **context}
        )
    except Exception as e:
        # מחזיר שגיאה במקום קריסה
        return str(e)


# =========================
# מנוע ראשי
# =========================
def run_engine():

    conn = pyodbc.connect(conn_str)
    print(" התחברנו למסד")

    formulas_df = pd.read_sql("SELECT * FROM t_targil", conn)
    data_df = pd.read_sql("SELECT * FROM t_data", conn)

    results = []

    #  זמן כולל
    total_start = time.time()

    context = data_df.to_dict(orient="series")

    # =========================
    # מעבר על כל נוסחה
    # =========================
    for _, row in formulas_df.iterrows():

        targil_id = row["targil_id"]
        formula = row["targil"]

        print(f"▶ מחשב תרגיל {targil_id}")

        #  זמן התחלה
        start_time = time.perf_counter()

        result = safe_eval(formula, context)

        #  זמן סיום
        end_time = time.perf_counter()
        runtime = end_time - start_time

        # =========================
        # בדיקת הצלחה / שגיאה
        # =========================
        if isinstance(result, str):
            #  שגיאה
            print(f" תרגיל {targil_id} נכשל")
            print(f"   סיבה: {result}")
            print(f"   זמן: {runtime:.4f} שניות")

            save_log(conn, targil_id, runtime, "FAIL")

            continue

        #  הצלחה
        print(f" תרגיל {targil_id} הסתיים בזמן: {runtime:.4f} שניות")

        temp = pd.DataFrame({
            "data_id": data_df["data_id"],
            "targil_id": targil_id,
            "method": "Python",
            "result": result
        })

        results.append(temp)

        save_log(conn, targil_id, runtime, "Python")

    # =========================
    # שמירה למסד
    # =========================
    if results:
        final_df = pd.concat(results)
        bulk_insert(conn, final_df)

    #  זמן כולל
    total_time = time.time() - total_start

    print("===================================")
    print(" כל התרגילים הסתיימו")
    print(f" זמן כולל: {total_time:.4f} שניות")
    print("===================================")

    conn.close()


# =========================
# שמירה למסד (תוצאות)
# =========================
def bulk_insert(conn, df):
    cursor = conn.cursor()

    cursor.executemany(
        "INSERT INTO t_results (data_id, targil_id, method, result) VALUES (?, ?, ?, ?)",
        df.values.tolist()
    )

    conn.commit()


# =========================
# שמירת לוג
# =========================
def save_log(conn, targil_id, runtime, status):
    cursor = conn.cursor()

    cursor.execute(
        "INSERT INTO t_log (targil_id, method, run_time) VALUES (?, ?, ?)",
        (targil_id, status, runtime)
    )

    conn.commit()


# =========================
# הרצה
# =========================
if __name__ == "__main__":
    run_engine()