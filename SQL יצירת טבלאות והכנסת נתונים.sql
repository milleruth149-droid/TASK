--- שלב א' יצירת טבלאות 
-- 1. טבלת נתונים
CREATE TABLE t_data (
    data_id INT PRIMARY KEY IDENTITY(1,1),
    a FLOAT NOT NULL,
    b FLOAT NOT NULL,
    c FLOAT NOT NULL,
    d FLOAT NOT NULL
);

-- 2. טבלת נוסחאות
CREATE TABLE t_targil (
    targil_id INT PRIMARY KEY IDENTITY(1,1),
    targil VARCHAR(MAX) NOT NULL,
    tnai VARCHAR(MAX) NULL,
    targil_false VARCHAR(MAX) NULL
);

-- 3. טבלת תוצאות
CREATE TABLE t_results (
    results_id INT PRIMARY KEY IDENTITY(1,1),
    data_id INT FOREIGN KEY REFERENCES t_data(data_id),
    targil_id INT FOREIGN KEY REFERENCES t_targil(targil_id),
    method VARCHAR(50) NOT NULL,
    result FLOAT
);

-- 4. טבלת לוג ביצועים
CREATE TABLE t_log (
    log_id INT PRIMARY KEY IDENTITY(1,1),
    targil_id INT FOREIGN KEY REFERENCES t_targil(targil_id),
    method VARCHAR(50) NOT NULL,
  run_time FLOAT -- זמן בשניות
);


---  שלב ב' הכנסת נתונים 

--טבלת הנתונים
INSERT INTO t_data (a, b, c, d)
SELECT TOP 1000000
    -- כדי למנוע שיהיו אפסים הוספתי מספר קטן להגרלה
    --הגרלת מספר עשרוני רנדומלי בין 0.1 ל-100.1
    (ABS(CHECKSUM(NEWID()) % 10000) / 100.0) + 0.1 AS a,
    (ABS(CHECKSUM(NEWID()) % 10000) / 100.0) + 0.1 AS b,
    (ABS(CHECKSUM(NEWID()) % 10000) / 100.0) + 0.1 AS c,
    (ABS(CHECKSUM(NEWID()) % 10000) / 100.0) + 0.1 AS d
FROM sys.all_objects a
CROSS JOIN sys.all_objects b;
GO
-- בדיקה איך הנתונים נראים 
SELECT * FROM t_data




---טבלת הנוסחאות
INSERT INTO t_targil (targil, tnai, targil_false)
VALUES 
-- נוסחאות פשוטות
('a + b', NULL, NULL),
('c * 2', NULL, NULL),
('a - b', NULL, NULL),
('d / 4', NULL, NULL),

-- נוסחאות מורכבות
('(a + b) * 8', NULL, NULL),
('SQRT(POWER(c, 2) + POWER(d, 2))', NULL, NULL), -- חישוב שורש ריבועי של סכום ריבועים
('LOG(b) + c', NULL, NULL),                      -- חישוב לוגריתם טבעי של שדה, ועוד שדה אחר
('ABS(a - b)', NULL, NULL),                      -- ערך מוחלטחישוב ערך מוחלט של הפרש בין שני שדות

-- נוסחאות עם תנאים
('b * 2', 'a > 5', 'b / 2'),
('a + 1', 'b < 10', 'd - 1'),
('1', 'a = c', '0'); -- ב-SQL משתמשים ב-= אחד להשוואה
GO

-- בדיקה שהנתונים נכנסו
SELECT * FROM t_targil;
--