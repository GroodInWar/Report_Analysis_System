USE `file_analysis_website`;

DROP TRIGGER IF EXISTS `reports_before_update_validate_status`;

DELIMITER ;;
CREATE TRIGGER `reports_before_update_validate_status`
BEFORE UPDATE ON `reports`
FOR EACH ROW
BEGIN
    IF OLD.status <> NEW.status THEN
        IF OLD.status = 'submitted'
           AND NEW.status NOT IN ('under_review', 'linked', 'rejected') THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Invalid report status transition from submitted.';
        END IF;

        IF OLD.status = 'under_review'
           AND NEW.status NOT IN ('linked', 'rejected', 'closed') THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Invalid report status transition from under_review.';
        END IF;

        IF OLD.status = 'linked'
           AND NEW.status NOT IN ('under_review', 'closed') THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Invalid report status transition from linked.';
        END IF;

        IF OLD.status IN ('closed', 'rejected')
           AND NEW.status <> OLD.status THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Closed or rejected reports cannot change status.';
        END IF;
    END IF;

    IF NEW.status IN ('linked', 'closed') AND NEW.incident_id IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A linked or closed report must reference an incident.';
    END IF;
END ;;
DELIMITER ;
