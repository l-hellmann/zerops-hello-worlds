<?php
require 'vendor/autoload.php';

use Dotenv\Dotenv;
use Ramsey\Uuid\Uuid;

if (file_exists(__DIR__ . '/.env')) {
    $dotenv = Dotenv::createImmutable(__DIR__);
    $dotenv->load();
}

$path = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH);

if ($path == '/status') {
    echo json_encode(['status' => 'UP']);
    exit;
}

$dbconn = pg_connect(
    "host=" . getenv('DB_HOST', true) ?: 'localhost' .
    " port=" . getenv('DB_PORT', true) ?: '5432' .
    " dbname=" . getenv('DB_NAME', true) ?: 'mydatabase' .
    " user=" . getenv('DB_USER', true) ?: 'myuser' .
    " password=" . getenv('DB_PASS', true) ?: 'mypassword'
) or die('Could not connect: ' . pg_last_error());

pg_query($dbconn, "CREATE TABLE IF NOT EXISTS entries (id SERIAL PRIMARY KEY, data TEXT NOT NULL);");

$data = Uuid::uuid4()->toString();
pg_query($dbconn, "INSERT INTO entries (data) VALUES ('$data');");

$result = pg_query($dbconn, "SELECT COUNT(*) AS count FROM entries;");
$row = pg_fetch_assoc($result);

echo "Entry added successfully with random data: $data. Total count: " . $row['count'];

pg_close($dbconn);
