USE `file_analysis_website`;

-- Demo accounts for application testing.
-- Password hashes are SHA-256 values accepted by the app's legacy-login fallback.
-- The app rehashes each password with ASP.NET Identity after first login.
INSERT INTO `users` (
    `role_id`,
    `last_name`,
    `first_name`,
    `username`,
    `email`,
    `password_hash`,
    `created_at`,
    `updated_at`
)
VALUES
    (
        (SELECT `role_id` FROM `roles` WHERE `role_name` = 'admin' LIMIT 1),
        'User',
        'Admin',
        'admin',
        'admin@example.com',
        '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121',
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    ),
    (
        (SELECT `role_id` FROM `roles` WHERE `role_name` = 'analyst' LIMIT 1),
        'Analyst',
        'Analyst',
        'analyst',
        'analyst@example.com',
        '84a6ed197836ce9fe88ed4cd036a048c7b01ce048ba35d8f9b2f7cf6bbc2970a',
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    ),
    (
        (SELECT `role_id` FROM `roles` WHERE `role_name` = 'user' LIMIT 1),
        'User',
        'Uma',
        'user',
        'user@example.com',
        'bc5848f227cc161eb5f68dfe98cb13110a9c843ce69e953a88107d865583d397',
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
ON DUPLICATE KEY UPDATE
    `role_id` = VALUES(`role_id`),
    `last_name` = VALUES(`last_name`),
    `first_name` = VALUES(`first_name`),
    `email` = VALUES(`email`),
    `password_hash` = VALUES(`password_hash`),
    `updated_at` = CURRENT_TIMESTAMP;
