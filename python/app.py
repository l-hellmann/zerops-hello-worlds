from flask import Flask, request, jsonify
from dotenv import load_dotenv
import os
import psycopg2
from psycopg2.extras import RealDictCursor
import uuid

# Load environment variables from .env file if present
load_dotenv()

app = Flask(__name__)

# Database connection details
db_host = os.getenv('DB_HOST', 'localhost')
db_port = os.getenv('DB_PORT', '5432')
db_name = os.getenv('DB_NAME', 'mydatabase')
db_user = os.getenv('DB_USER', 'myuser')
db_pass = os.getenv('DB_PASS', 'mypassword')

@app.route('/')
def add_entry():
    # Connect to the database
    conn = psycopg2.connect(host=db_host, port=db_port, dbname=db_name, user=db_user, password=db_pass)
    cur = conn.cursor(cursor_factory=RealDictCursor)

    # Insert new entry with random data
    random_data = str(uuid.uuid4())
    cur.execute("INSERT INTO entries (data) VALUES (%s)", (random_data,))

    # Retrieve count of all entries
    cur.execute("SELECT COUNT(*) as count FROM entries")
    entry_count = cur.fetchone()['count']

    # Commit and close
    conn.commit()
    cur.close()
    conn.close()

    return jsonify(message="Entry added successfully with random data.", data=random_data, count=entry_count)

@app.route('/status')
def status():
    return jsonify(status="UP")

if __name__ == '__main__':
    app.run(debug=True)
