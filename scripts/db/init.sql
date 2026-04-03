-- HrSaas Database Initialization Script
-- Creates schemas for module separation

-- Schemas (one per module for clean separation)
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS tenant;
CREATE SCHEMA IF NOT EXISTS employee;
CREATE SCHEMA IF NOT EXISTS leave;
CREATE SCHEMA IF NOT EXISTS billing;

-- Outbox table for reliable event publishing (shared across modules)
CREATE TABLE IF NOT EXISTS public.outbox_messages (
    id UUID PRIMARY KEY,
    type TEXT NOT NULL,
    content JSONB NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ,
    error TEXT,
    lock_token UUID,
    locked_until TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_outbox_unprocessed
    ON public.outbox_messages(occurred_at)
    WHERE processed_at IS NULL;
