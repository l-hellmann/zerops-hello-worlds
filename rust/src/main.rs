use actix_web::{web, App, HttpResponse, HttpServer, Responder};
use dotenv::dotenv;
use std::env;
use tokio_postgres::{NoTls, Error};
use uuid::Uuid;

async fn add_entry() -> Result<impl Responder, Error> {
    let db_host = env::var("DB_HOST").unwrap_or_else(|_| "localhost".to_string());
    let db_port = env::var("DB_PORT").unwrap_or_else(|_| "5432".to_string());
    let db_user = env::var("DB_USER").expect("DB_USER must be set");
    let db_pass = env::var("DB_PASS").expect("DB_PASS must be set");
    let db_name = env::var("DB_NAME").expect("DB_NAME must be set");

    let conn_str = format!(
        "host={} port={} user={} password={} dbname={}",
        db_host, db_port, db_user, db_pass, db_name
    );

    let (client, connection) = tokio_postgres::connect(&conn_str, NoTls).await?;

    tokio::spawn(async move {
        if let Err(e) = connection.await {
            eprintln!("connection error: {}", e);
        }
    });

    client.execute(
        "CREATE TABLE IF NOT EXISTS entries (id SERIAL PRIMARY KEY, data TEXT NOT NULL);",
        &[],
    ).await?;

    let data = Uuid::new_v4().to_string();
    client.execute(
        "INSERT INTO entries (data) VALUES ($1);",
        &[&data],
    ).await?;

    let row = client.query_one("SELECT COUNT(*) FROM entries;", &[]).await?;
    let count: i64 = row.get(0);

    Ok(HttpResponse::Ok().body(format!("Entry added successfully with random data: {}. Total count: {}", data, count)))
}

async fn status() -> impl Responder {
    HttpResponse::Ok().body("UP")
}

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    dotenv().ok();

    println!("Starting server at http://0.0.0.0:8080");

    HttpServer::new(|| {
        App::new()
            .route("/", web::get().to(add_entry))
            .route("/status", web::get().to(status))
    })
    .bind("0.0.0.0:8080")?
    .run()
    .await
}
