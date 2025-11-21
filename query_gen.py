import pandas as pd
import os
import sqlite3

# Adjust file name to match your C# output
current = os.getcwd()
file = os.path.join(current, 'score_csv.txt')
""" 
# Read the CSV file
try:
    scoreboard = pd.read_csv(file)
    print(scoreboard)
except FileNotFoundError:
    print(f"File not found: {file}")
"""   

# Connect to your database
con = sqlite3.connect("PongGameDB.db")
cur = con.cursor()

# Read the SQL file
with open("scoreboard.sql", "r", encoding="utf-8") as sql_file:
    sql_script = sql_file.read()

# Execute the SQL script
cur.executescript(sql_script)

# Commit and verify
con.commit()

# Verify table creation
res = cur.execute("SELECT name FROM sqlite_master WHERE type='table';")
print(res.fetchall())

#TODO: Insert data from CSV to the database table

# Close the connection
con.close()