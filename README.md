# MyCarApp

## Deployment configuration

This project is configured so that all production secrets and configuration values are supplied by Render environment variables.

### Render environment variables

Set the following variables in Render for production/staging deployments:

- `DB_CONNECTION_STRING` — full Postgres connection string.
- `DATABASE_URL` — alternative to `DB_CONNECTION_STRING`; compatible with Supabase/Render-style URIs.
- `JWT_SECRET` — strong secret key used to sign JWT tokens.
- `JWT_ISSUER` — expected JWT issuer (default: `MyCarApp`).
- `JWT_AUDIENCE` — expected JWT audience (default: `MyCarAppUsers`).
- `JWT_EXPIRY_IN_DAYS` — token lifetime in days (default: `7`).
- `CLOUDINARY_CLOUD_NAME` — Cloudinary cloud name.
- `CLOUDINARY_API_KEY` — Cloudinary API key.
- `CLOUDINARY_API_SECRET` — Cloudinary API secret.

> Do not store production credentials in source files. `src/MyCarApp.Api/appsettings.json` is intentionally kept free of production secrets.

## Local development

Local-only configuration is stored in `src/MyCarApp.Api/appsettings.Development.json`.

This file is used when the app runs in the Development environment and currently contains local Docker database settings only.

### Example local environment file

A sample `.env.example` is included in the repository. Copy it to `.env` for local environment testing if needed.

### Local database settings

The default local database configuration is:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mycarapp_db;Username=mycarapp_user;Password=mycarapp_pass;SSL Mode=Prefer;Trust Server Certificate=true"
  }
}
```

## Notes

- `src/MyCarApp.Api/Program.cs` prefers environment variables first, then falls back to configuration.
- For production, never commit live secrets to `appsettings.json`.
- `appsettings.Development.json` is already excluded from source control via `.gitignore`.
