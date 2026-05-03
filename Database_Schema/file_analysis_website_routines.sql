CREATE DATABASE IF NOT EXISTS `file_analysis_website`
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;
USE `file_analysis_website`;

DROP VIEW IF EXISTS `vw_incident_dashboard`;
DROP VIEW IF EXISTS `vw_report_queue`;
DROP VIEW IF EXISTS `vw_user_activity_summary`;
DROP VIEW IF EXISTS `vw_file_evidence_summary`;
DROP VIEW IF EXISTS `vw_incident_counts_by_severity`;
DROP VIEW IF EXISTS `vw_incident_counts_by_category`;
DROP VIEW IF EXISTS `vw_report_counts_by_status`;
DROP VIEW IF EXISTS `vw_user_report_counts`;

DROP PROCEDURE IF EXISTS `create_incident_from_report`;
DROP PROCEDURE IF EXISTS `close_incident`;
DROP PROCEDURE IF EXISTS `assign_report_to_incident`;
DROP PROCEDURE IF EXISTS `add_comment_to_incident`;

DROP FUNCTION IF EXISTS `get_incident_status`;
DROP FUNCTION IF EXISTS `get_severity_rank`;

DROP TRIGGER IF EXISTS `incidents_before_insert_validate_creator_role`;
DROP TRIGGER IF EXISTS `comments_before_insert_prevent_resolved`;

DELIMITER ;;

CREATE FUNCTION `get_incident_status`(p_resolved_at DATETIME)
RETURNS VARCHAR(20)
DETERMINISTIC
NO SQL
BEGIN
    RETURN CASE
        WHEN p_resolved_at IS NULL THEN 'open'
        ELSE 'resolved'
    END;
END ;;

CREATE FUNCTION `get_severity_rank`(p_severity_name VARCHAR(30))
RETURNS TINYINT UNSIGNED
DETERMINISTIC
NO SQL
BEGIN
    RETURN CASE LOWER(p_severity_name)
        WHEN 'low' THEN 1
        WHEN 'medium' THEN 2
        WHEN 'high' THEN 3
        WHEN 'critical' THEN 4
        ELSE 0
    END;
END ;;

CREATE PROCEDURE `create_incident_from_report`(
    IN p_report_id INT UNSIGNED,
    IN p_analyst_id INT UNSIGNED,
    IN p_category_id INT UNSIGNED,
    IN p_severity_id INT UNSIGNED,
    IN p_incident_title VARCHAR(150),
    IN p_incident_description TEXT
)
MODIFIES SQL DATA
BEGIN
    DECLARE v_incident_id INT UNSIGNED;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    INSERT INTO `incidents` (
        `created_by_user_id`,
        `category_id`,
        `severity_id`,
        `incident_title`,
        `incident_description`
    )
    VALUES (
        p_analyst_id,
        p_category_id,
        p_severity_id,
        p_incident_title,
        p_incident_description
    );

    SET v_incident_id = LAST_INSERT_ID();

    UPDATE `reports`
    SET `incident_id` = v_incident_id,
        `status` = 'linked'
    WHERE `report_id` = p_report_id;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Report does not exist.';
    END IF;

    COMMIT;
END ;;

CREATE PROCEDURE `close_incident`(
    IN p_incident_id INT UNSIGNED
)
MODIFIES SQL DATA
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    UPDATE `incidents`
    SET `resolved_at` = COALESCE(`resolved_at`, CURRENT_TIMESTAMP),
        `updated_at` = CURRENT_TIMESTAMP
    WHERE `incident_id` = p_incident_id;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Incident does not exist.';
    END IF;

    UPDATE `reports`
    SET `status` = 'closed'
    WHERE `incident_id` = p_incident_id
      AND `status` = 'linked';

    COMMIT;
END ;;

CREATE PROCEDURE `assign_report_to_incident`(
    IN p_report_id INT UNSIGNED,
    IN p_incident_id INT UNSIGNED
)
MODIFIES SQL DATA
BEGIN
    UPDATE `reports`
    SET `incident_id` = p_incident_id,
        `status` = 'linked'
    WHERE `report_id` = p_report_id;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Report does not exist.';
    END IF;
END ;;

CREATE PROCEDURE `add_comment_to_incident`(
    IN p_incident_id INT UNSIGNED,
    IN p_user_id INT UNSIGNED,
    IN p_comment_text TEXT
)
MODIFIES SQL DATA
BEGIN
    INSERT INTO `comments` (
        `incident_id`,
        `user_id`,
        `comment_text`
    )
    VALUES (
        p_incident_id,
        p_user_id,
        p_comment_text
    );
END ;;

CREATE TRIGGER `incidents_before_insert_validate_creator_role`
BEFORE INSERT ON `incidents`
FOR EACH ROW
BEGIN
    DECLARE v_authorized_creator_count INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_authorized_creator_count
    FROM `users` AS `u`
    INNER JOIN `roles` AS `r`
        ON `u`.`role_id` = `r`.`role_id`
    WHERE `u`.`user_id` = NEW.`created_by_user_id`
      AND `r`.`role_name` IN ('admin', 'analyst');

    IF v_authorized_creator_count = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Only analysts or admins can create incidents.';
    END IF;
END ;;

CREATE TRIGGER `comments_before_insert_prevent_resolved`
BEFORE INSERT ON `comments`
FOR EACH ROW
BEGIN
    DECLARE v_resolved_at DATETIME;

    SELECT `resolved_at`
    INTO v_resolved_at
    FROM `incidents`
    WHERE `incident_id` = NEW.`incident_id`;

    IF v_resolved_at IS NOT NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Comments cannot be added to resolved incidents.';
    END IF;
END ;;

DELIMITER ;

CREATE VIEW `vw_incident_dashboard` AS
SELECT
    `i`.`incident_id`,
    `i`.`incident_title`,
    `i`.`incident_description`,
    `c`.`category_name`,
    `s`.`severity_name`,
    `get_severity_rank`(`s`.`severity_name`) AS `severity_rank`,
    CONCAT(`u`.`first_name`, ' ', `u`.`last_name`) AS `created_by_name`,
    `u`.`username` AS `created_by_username`,
    `get_incident_status`(`i`.`resolved_at`) AS `incident_status`,
    COUNT(DISTINCT `r`.`report_id`) AS `linked_report_count`,
    COUNT(DISTINCT `cm`.`comment_id`) AS `comment_count`,
    COUNT(DISTINCT `ifi`.`file_id`) AS `incident_file_count`,
    `i`.`created_at`,
    `i`.`updated_at`,
    `i`.`resolved_at`
FROM `incidents` AS `i`
INNER JOIN `users` AS `u`
    ON `i`.`created_by_user_id` = `u`.`user_id`
INNER JOIN `categories` AS `c`
    ON `i`.`category_id` = `c`.`category_id`
INNER JOIN `severity` AS `s`
    ON `i`.`severity_id` = `s`.`severity_id`
LEFT JOIN `reports` AS `r`
    ON `i`.`incident_id` = `r`.`incident_id`
LEFT JOIN `comments` AS `cm`
    ON `i`.`incident_id` = `cm`.`incident_id`
LEFT JOIN `incident_files` AS `ifi`
    ON `i`.`incident_id` = `ifi`.`incident_id`
GROUP BY
    `i`.`incident_id`,
    `i`.`incident_title`,
    `i`.`incident_description`,
    `c`.`category_name`,
    `s`.`severity_name`,
    `u`.`first_name`,
    `u`.`last_name`,
    `u`.`username`,
    `i`.`created_at`,
    `i`.`updated_at`,
    `i`.`resolved_at`;

CREATE VIEW `vw_report_queue` AS
SELECT
    `r`.`report_id`,
    `r`.`title`,
    `r`.`report_text`,
    `r`.`status`,
    `r`.`submitted_at`,
    `r`.`updated_at`,
    `u`.`user_id` AS `submitted_by_user_id`,
    `u`.`username` AS `submitted_by_username`,
    `i`.`incident_id`,
    `i`.`incident_title`,
    COUNT(DISTINCT `rf`.`file_id`) AS `attached_file_count`
FROM `reports` AS `r`
INNER JOIN `users` AS `u`
    ON `r`.`submitted_by_user_id` = `u`.`user_id`
LEFT JOIN `incidents` AS `i`
    ON `r`.`incident_id` = `i`.`incident_id`
LEFT JOIN `report_files` AS `rf`
    ON `r`.`report_id` = `rf`.`report_id`
GROUP BY
    `r`.`report_id`,
    `r`.`title`,
    `r`.`report_text`,
    `r`.`status`,
    `r`.`submitted_at`,
    `r`.`updated_at`,
    `u`.`user_id`,
    `u`.`username`,
    `i`.`incident_id`,
    `i`.`incident_title`;

CREATE VIEW `vw_user_activity_summary` AS
SELECT
    `u`.`user_id`,
    `u`.`username`,
    `r`.`role_name`,
    COUNT(DISTINCT `rp`.`report_id`) AS `submitted_report_count`,
    COUNT(DISTINCT `i`.`incident_id`) AS `created_incident_count`,
    COUNT(DISTINCT `c`.`comment_id`) AS `comment_count`,
    `u`.`last_login_at`
FROM `users` AS `u`
INNER JOIN `roles` AS `r`
    ON `u`.`role_id` = `r`.`role_id`
LEFT JOIN `reports` AS `rp`
    ON `u`.`user_id` = `rp`.`submitted_by_user_id`
LEFT JOIN `incidents` AS `i`
    ON `u`.`user_id` = `i`.`created_by_user_id`
LEFT JOIN `comments` AS `c`
    ON `u`.`user_id` = `c`.`user_id`
GROUP BY
    `u`.`user_id`,
    `u`.`username`,
    `r`.`role_name`,
    `u`.`last_login_at`;

CREATE VIEW `vw_file_evidence_summary` AS
SELECT
    `f`.`file_id`,
    `f`.`file_name`,
    `f`.`file_path`,
    `f`.`file_hash`,
    `f`.`uploaded_at`,
    COUNT(DISTINCT `rf`.`report_id`) AS `linked_report_count`,
    COUNT(DISTINCT `ifi`.`incident_id`) AS `linked_incident_count`
FROM `files` AS `f`
LEFT JOIN `report_files` AS `rf`
    ON `f`.`file_id` = `rf`.`file_id`
LEFT JOIN `incident_files` AS `ifi`
    ON `f`.`file_id` = `ifi`.`file_id`
GROUP BY
    `f`.`file_id`,
    `f`.`file_name`,
    `f`.`file_path`,
    `f`.`file_hash`,
    `f`.`uploaded_at`;

CREATE VIEW `vw_incident_counts_by_severity` AS
SELECT
    `s`.`severity_id`,
    `s`.`severity_name`,
    `get_severity_rank`(`s`.`severity_name`) AS `severity_rank`,
    COUNT(`i`.`incident_id`) AS `incident_count`
FROM `severity` AS `s`
LEFT JOIN `incidents` AS `i`
    ON `s`.`severity_id` = `i`.`severity_id`
GROUP BY
    `s`.`severity_id`,
    `s`.`severity_name`;

CREATE VIEW `vw_incident_counts_by_category` AS
SELECT
    `c`.`category_id`,
    `c`.`category_name`,
    COUNT(`i`.`incident_id`) AS `incident_count`
FROM `categories` AS `c`
LEFT JOIN `incidents` AS `i`
    ON `c`.`category_id` = `i`.`category_id`
GROUP BY
    `c`.`category_id`,
    `c`.`category_name`;

CREATE VIEW `vw_report_counts_by_status` AS
SELECT
    `r`.`status`,
    COUNT(*) AS `report_count`
FROM `reports` AS `r`
GROUP BY `r`.`status`;

CREATE VIEW `vw_user_report_counts` AS
SELECT
    `u`.`user_id`,
    `u`.`username`,
    COUNT(`r`.`report_id`) AS `report_count`
FROM `users` AS `u`
LEFT JOIN `reports` AS `r`
    ON `u`.`user_id` = `r`.`submitted_by_user_id`
GROUP BY
    `u`.`user_id`,
    `u`.`username`;
