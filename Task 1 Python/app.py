from flask import Flask, jsonify
from flask_cors import CORS
import pyodbc
app = Flask(__name__)
CORS(app)

# חיבו למסד
conn = pyodbc.connect(
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=(localdb)\\MSSQLLocalDB;"
    "DATABASE=TESTRUTH;"
    "Trusted_Connection=yes;"
)


# GET מביא את הנתונים מהשרת 
cursor = conn.cursor()
@app.route('/results')
def get_results():
    cursor.execute("""
        SELECT method, targil_id, run_time
        FROM t_log
    """)

    rows = cursor.fetchall()

    data = []

    for r in rows:
        data.append({
            "methodName": r.method,
            "targilId": r.targil_id,
            "runTime": r.run_time
        })
    data.sort(key=lambda x: x["runTime"])
    return jsonify(data)

if __name__ == '__main__':
    app.run(port=5000, debug=True)