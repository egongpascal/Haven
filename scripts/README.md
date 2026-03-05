# Haven Database Scripts

## Emergency/SOS storage

**Default: PostgreSQL** (same database as groups). Set `EmergencyStorage: "Postgres"` in appsettings (default).

1. Run `001_create_emergencies_table.sql` against your Postgres database (same DB as groups)
2. Backend uses `PostgresEmergencyRepository` when `EmergencyStorage` is "Postgres" and `PostgresGroups` is configured

**Alternative: MongoDB.** Set `EmergencyStorage: "Mongo"` to use MongoDB instead:
- Database: `haven_location_db`, collection: `emergencies`
- Connection string: `MongoLocations` in appsettings.json
- No table creation required

## Running the emergencies script

```bash
# From project root, against your Postgres DB (same as groups)
psql "Host=your-host;Port=5432;Database=haven_0ipb;Username=your-user;Password=your-pass" -f scripts/001_create_emergencies_table.sql
```

Or use a GUI (pgAdmin, DBeaver) to run the SQL file against your database.

## Schema reference (Postgres)

| Column        | Type           | Description                          |
|---------------|----------------|--------------------------------------|
| id            | VARCHAR(36)    | GUID, primary key                    |
| group_id      | UUID           | UUID of the group                    |
| user_id       | VARCHAR(36)    | User who triggered SOS               |
| latitude      | DOUBLE PRECISION | Trigger location                  |
| longitude     | DOUBLE PRECISION | Trigger location                  |
| emergency_type| VARCHAR(50)    | "SOS"                                |
| timestamp     | TIMESTAMPTZ    | When triggered (UTC)                 |
| status        | VARCHAR(20)    | "Active" or "Resolved"               |
| resolved_at   | TIMESTAMPTZ    | When resolved (nullable)             |
