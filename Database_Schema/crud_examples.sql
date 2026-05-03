USE `file_analysis_website`;

START TRANSACTION;

SET @crud_suffix = LEFT(REPLACE(UUID(), '-', ''), 8);

INSERT INTO `roles` (`role_name`)
VALUES (CONCAT('role_', @crud_suffix));
SET @crud_role_id = LAST_INSERT_ID();

INSERT INTO `categories` (`category_name`)
VALUES (CONCAT('category_', @crud_suffix));
SET @crud_category_id = LAST_INSERT_ID();

INSERT INTO `severity` (`severity_name`)
VALUES (CONCAT('sev_', @crud_suffix));
SET @crud_severity_id = LAST_INSERT_ID();

SET @crud_user_role_id = COALESCE((
    SELECT `role_id`
    FROM `roles`
    WHERE `role_name` IN ('analyst', 'admin')
    ORDER BY FIELD(`role_name`, 'analyst', 'admin')
    LIMIT 1
), @crud_role_id);

INSERT INTO `users` (
    `role_id`,
    `last_name`,
    `first_name`,
    `username`,
    `email`,
    `password_hash`
)
VALUES (
    @crud_user_role_id,
    'Example',
    'Crud',
    CONCAT('crud_user_', @crud_suffix),
    CONCAT('crud_user_', @crud_suffix, '@example.com'),
    SHA2(CONCAT('crud_password_', @crud_suffix), 256)
);
SET @crud_user_id = LAST_INSERT_ID();

INSERT INTO `incidents` (
    `created_by_user_id`,
    `category_id`,
    `severity_id`,
    `incident_title`,
    `incident_description`
)
VALUES (
    @crud_user_id,
    @crud_category_id,
    @crud_severity_id,
    CONCAT('CRUD incident ', @crud_suffix),
    'Incident row used by CRUD examples.'
);
SET @crud_incident_id = LAST_INSERT_ID();

INSERT INTO `reports` (
    `submitted_by_user_id`,
    `title`,
    `report_text`
)
VALUES (
    @crud_user_id,
    CONCAT('CRUD report ', @crud_suffix),
    'Report row used by CRUD examples.'
);
SET @crud_report_id = LAST_INSERT_ID();

INSERT INTO `files` (`file_name`, `file_path`, `file_hash`)
VALUES
    (CONCAT('crud_', @crud_suffix, '_a.txt'), CONCAT('/tmp/crud_', @crud_suffix, '_a.txt'), SHA2(CONCAT(@crud_suffix, '_a'), 256)),
    (CONCAT('crud_', @crud_suffix, '_b.txt'), CONCAT('/tmp/crud_', @crud_suffix, '_b.txt'), SHA2(CONCAT(@crud_suffix, '_b'), 256));
SET @crud_file_id_a = LAST_INSERT_ID();
SET @crud_file_id_b = @crud_file_id_a + 1;

INSERT INTO `comments` (`incident_id`, `user_id`, `comment_text`)
VALUES (@crud_incident_id, @crud_user_id, 'Comment row used by CRUD examples.');
SET @crud_comment_id = LAST_INSERT_ID();

INSERT INTO `report_files` (`report_id`, `file_id`)
VALUES (@crud_report_id, @crud_file_id_a);

INSERT INTO `incident_files` (`incident_id`, `file_id`)
VALUES (@crud_incident_id, @crud_file_id_a);

-- SELECT examples
SELECT `role_id`, `role_name`
FROM `roles`
WHERE `role_id` = @crud_role_id;

SELECT `user_id`, `username`, `email`, `role_id`
FROM `users`
WHERE `user_id` = @crud_user_id;

SELECT `category_id`, `category_name`
FROM `categories`
WHERE `category_id` = @crud_category_id;

SELECT `severity_id`, `severity_name`
FROM `severity`
WHERE `severity_id` = @crud_severity_id;

SELECT `incident_id`, `incident_title`, `created_by_user_id`, `category_id`, `severity_id`
FROM `incidents`
WHERE `incident_id` = @crud_incident_id;

SELECT `report_id`, `title`, `status`, `submitted_by_user_id`
FROM `reports`
WHERE `report_id` = @crud_report_id;

SELECT `file_id`, `file_name`, `file_path`, `file_hash`
FROM `files`
WHERE `file_id` IN (@crud_file_id_a, @crud_file_id_b);

SELECT `comment_id`, `incident_id`, `user_id`, `comment_text`
FROM `comments`
WHERE `comment_id` = @crud_comment_id;

SELECT `report_id`, `file_id`
FROM `report_files`
WHERE `report_id` = @crud_report_id
  AND `file_id` = @crud_file_id_a;

SELECT `incident_id`, `file_id`
FROM `incident_files`
WHERE `incident_id` = @crud_incident_id
  AND `file_id` = @crud_file_id_a;

-- UPDATE examples
UPDATE `roles`
SET `role_name` = CONCAT('role_up_', @crud_suffix)
WHERE `role_id` = @crud_role_id;

UPDATE `users`
SET `last_login_at` = CURRENT_TIMESTAMP
WHERE `user_id` = @crud_user_id;

UPDATE `categories`
SET `category_name` = CONCAT('category_updated_', @crud_suffix)
WHERE `category_id` = @crud_category_id;

UPDATE `severity`
SET `severity_name` = CONCAT('sev_up_', @crud_suffix)
WHERE `severity_id` = @crud_severity_id;

UPDATE `incidents`
SET `incident_description` = 'Updated incident row used by CRUD examples.'
WHERE `incident_id` = @crud_incident_id;

UPDATE `reports`
SET `status` = 'under_review'
WHERE `report_id` = @crud_report_id;

UPDATE `files`
SET `file_path` = CONCAT('/tmp/updated_crud_', @crud_suffix, '_a.txt')
WHERE `file_id` = @crud_file_id_a;

UPDATE `comments`
SET `comment_text` = 'Updated comment row used by CRUD examples.'
WHERE `comment_id` = @crud_comment_id;

UPDATE `report_files`
SET `file_id` = @crud_file_id_b
WHERE `report_id` = @crud_report_id
  AND `file_id` = @crud_file_id_a;

UPDATE `incident_files`
SET `file_id` = @crud_file_id_b
WHERE `incident_id` = @crud_incident_id
  AND `file_id` = @crud_file_id_a;

-- DELETE examples
DELETE FROM `comments`
WHERE `comment_id` = @crud_comment_id;

DELETE FROM `report_files`
WHERE `report_id` = @crud_report_id
  AND `file_id` = @crud_file_id_b;

DELETE FROM `incident_files`
WHERE `incident_id` = @crud_incident_id
  AND `file_id` = @crud_file_id_b;

DELETE FROM `reports`
WHERE `report_id` = @crud_report_id;

DELETE FROM `incidents`
WHERE `incident_id` = @crud_incident_id;

DELETE FROM `files`
WHERE `file_id` IN (@crud_file_id_a, @crud_file_id_b);

DELETE FROM `users`
WHERE `user_id` = @crud_user_id;

DELETE FROM `severity`
WHERE `severity_id` = @crud_severity_id;

DELETE FROM `categories`
WHERE `category_id` = @crud_category_id;

DELETE FROM `roles`
WHERE `role_id` = @crud_role_id;

ROLLBACK;
