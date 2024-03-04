<?php
require 'vendor/autoload.php';

use Dotenv\Dotenv;
use Ramsey\Uuid\Uuid;

// Load environment variables
$dotenv = Dotenv::createImmutable(__DIR__);
$dotenv->load();

// Database connection
$dbconn = pg_connect("host=".$_ENV['DB_HOST']." port=".$_ENV['DB_PORT']." dbname=".$_ENV['DB_NAME']." user=".$_ENV['DB_USER']." password=".$_ENV['DB_PASS'])
    or die('Could not connect: ' . pg_last_error());

// Ensure the table exists
pg_query($dbconn, "CREATE TABLE IF NOT EXISTS entries (id SERIAL PRIMARY KEY, data TEXT NOT NULL);");

// Insert a new entry with random data
$data = Uuid::uuid4()->toString();
pg_query($dbconn, "INSERT INTO entries (data) VALUES ('$data');");

// Get the count of entries
$result = pg_query($dbconn, "SELECT COUNT(*) AS count FROM entries;");
$row = pg_fetch_assoc($result);

echo "Entry added successfully with random data: $data. Total count: " . $row['count'];

pg_close($dbconn);
