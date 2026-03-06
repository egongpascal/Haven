-- =============================================================================
-- Muster points table for PostgreSQL
-- Run against the same database as emergencies
-- Safe to run multiple times (idempotent)
-- =============================================================================

CREATE TABLE IF NOT EXISTS muster_points (
    id              VARCHAR(36) PRIMARY KEY,
    incident_id     VARCHAR(36) NOT NULL,
    name            VARCHAR(255) NOT NULL,
    latitude        DOUBLE PRECISION NOT NULL,
    longitude       DOUBLE PRECISION NOT NULL,
    radius_metres   DOUBLE PRECISION NOT NULL DEFAULT 100,
    created_by      VARCHAR(36) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    resolved_at     TIMESTAMPTZ,
    total_members   INT NOT NULL DEFAULT 0,
    arrived_count   INT NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_muster_points_incident_id ON muster_points(incident_id);
