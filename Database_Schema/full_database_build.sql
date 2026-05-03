-- Full database rebuild for the File Analysis Website project.
--
-- Run this from the project root with:
-- mysql -u root -p < Database_Schema/full_database_build.sql
--
-- This script intentionally drops and recreates the project database.

DROP DATABASE IF EXISTS `file_analysis_website`;
CREATE DATABASE `file_analysis_website`
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;
USE `file_analysis_website`;

-- Tables and seed data in foreign-key-safe order.

SOURCE Database_Schema/file_analysis_website_roles.sql;
SOURCE Database_Schema/file_analysis_website_users.sql;
SOURCE Database_Schema/file_analysis_website_categories.sql;
SOURCE Database_Schema/file_analysis_website_severity.sql;
SOURCE Database_Schema/file_analysis_website_incidents.sql;
SOURCE Database_Schema/file_analysis_website_reports.sql;
SOURCE Database_Schema/file_analysis_website_files.sql;
SOURCE Database_Schema/file_analysis_website_comments.sql;
SOURCE Database_Schema/file_analysis_website_report_files.sql;
SOURCE Database_Schema/file_analysis_website_incident_files.sql;

-- Functions, procedures, views, and project triggers.
SOURCE Database_Schema/file_analysis_website_routines.sql;

-- CRUD examples are kept separate because they intentionally roll back.
-- Run this after the rebuild when you want to demonstrate insert/select/update/delete:
-- SOURCE Database_Schema/crud_examples.sql;

-- Smoke-check the rebuilt schema.
SELECT 'roles' AS `object_name`, COUNT(*) AS `row_count` FROM `roles`
UNION ALL SELECT 'users', COUNT(*) FROM `users`
UNION ALL SELECT 'categories', COUNT(*) FROM `categories`
UNION ALL SELECT 'severity', COUNT(*) FROM `severity`
UNION ALL SELECT 'reports', COUNT(*) FROM `reports`
UNION ALL SELECT 'incidents', COUNT(*) FROM `incidents`
UNION ALL SELECT 'files', COUNT(*) FROM `files`;

SELECT
    ROUTINE_TYPE AS `object_type`,
    ROUTINE_NAME AS `object_name`
FROM information_schema.ROUTINES
WHERE ROUTINE_SCHEMA = 'file_analysis_website'
ORDER BY ROUTINE_TYPE, ROUTINE_NAME;

SELECT
    TABLE_NAME AS `view_name`
FROM information_schema.VIEWS
WHERE TABLE_SCHEMA = 'file_analysis_website'
ORDER BY TABLE_NAME;
