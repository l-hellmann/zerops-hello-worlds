use actix_web::{web, App, HttpResponse, HttpServer, Responder, http::StatusCode};
use dotenv::dotenv;
use std::env;
use tokio_postgres::NoTls;
use uuid::Uuid;

// Define a custom error type that wraps tokio_postgres::Error
struct AppError(tokio_postgres::Error);

impl From<tokio_postgres::Error> for AppError {
    fn from(err: tokio_postgres::Error) -> AppError {
        AppError(err)
    }
}

impl actix_web::error::ResponseError for AppError {
    fn status_code(&self) -> StatusCode {
        // You can decide to return different status codes based on the error
        StatusCode::INTERNAL_SERVER_ERROR
    }
}

async fn add_entry() -> Result<HttpResponse, actix_web::Error> {
    let path = "/"; // Define the path you are handling

    let db_host = env::var("DB_HOST").unwrap_or_else(|_| "localhost".to_string());
    let db_port = env::var("DB_PORT").unwrap_or_else(|_| "5432".to_string());
    let db_user = env::var("DB_USER").expect("DB_USER must be set");
    let db_pass = env::var("DB_PASS").expect("DB_PASS must be set");
    let db_name = env::var("DB_NAME").expect("DB_NAME must be set");

    let conn_str = format!(
        "host={} port={} user={} password={} dbname={}",
        db_host, db_port, db_user, db_pass, db_name
    );

    let (client, connection) = tokio_postgres::connect(&conn_str, NoTls).await.map_err(AppError::from)?;

    tokio::spawn(async move {
        if let Err(e) = connection.await {
            eprintln!("connection error: {}", e);
        }
    });

    client.execute(
        "CREATE TABLE IF NOT EXISTS entries (id SERIAL PRIMARY KEY, data TEXT NOT NULL);",
        &[],
    ).await.map_err(AppError::from)?;

    let data = Uuid::new_v4().to_string();
    client.execute(
        "INSERT INTO entries (data) VALUES ($1);",
        &[&data],
    ).await.map_err(AppError::from)?;

    let row = client.query_one("SELECT COUNT(*) FROM entries;", &[]).await.map_err(AppError::from)?;
    let count: i64 = row.get(0);

    Ok(HttpResponse::Ok().body(format!("Entry added successfully with random data: {}. Total count: {}", data, count)))
}

async fn status() -> impl Responder {
    HttpResponse::Ok().body("UP")
}

async fn not_found() -> impl Responder {
    HttpResponse::NotFound().body("Not Found")
}

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    dotenv().ok();

    println!("Starting server at http://0.0.0.0:8080");

    HttpServer::new(|| {
        App::new()
            .service(web::resource("/").route(web::get().to(add_entry)))
            .service(web::resource("/status").route(web::get().to(status)))
            .default_service(web::route().to(not_found)) // Catch-all for unmatched routes
    })
    .bind("0.0.0.0:8080")?
    .run()
    .await
}
