CREATE DATABASE  IF NOT EXISTS `file_analysis_website` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `file_analysis_website`;
-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: file_analysis_website
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Temporary view structure for view `vw_incident_dashboard`
--

DROP TABLE IF EXISTS `vw_incident_dashboard`;
/*!50001 DROP VIEW IF EXISTS `vw_incident_dashboard`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `vw_incident_dashboard` AS SELECT 
 1 AS `incident_id`,
 1 AS `incident_title`,
 1 AS `incident_description`,
 1 AS `category_name`,
 1 AS `severity_name`,
 1 AS `created_by_name`,
 1 AS `created_by_username`,
 1 AS `incident_status`,
 1 AS `linked_report_count`,
 1 AS `comment_count`,
 1 AS `incident_file_count`,
 1 AS `created_at`,
 1 AS `updated_at`,
 1 AS `resolved_at`*/;
SET character_set_client = @saved_cs_client;

--
-- Final view structure for view `vw_incident_dashboard`
--

/*!50001 DROP VIEW IF EXISTS `vw_incident_dashboard`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`root`@`localhost` SQL SECURITY DEFINER */
/*!50001 VIEW `vw_incident_dashboard` AS select `i`.`incident_id` AS `incident_id`,`i`.`incident_title` AS `incident_title`,`i`.`incident_description` AS `incident_description`,`c`.`category_name` AS `category_name`,`s`.`severity_name` AS `severity_name`,concat(`u`.`first_name`,' ',`u`.`last_name`) AS `created_by_name`,`u`.`username` AS `created_by_username`,`get_incident_status`(`i`.`resolved_at`) AS `incident_status`,count(distinct `r`.`report_id`) AS `linked_report_count`,count(distinct `cm`.`comment_id`) AS `comment_count`,count(distinct `ifi`.`file_id`) AS `incident_file_count`,`i`.`created_at` AS `created_at`,`i`.`updated_at` AS `updated_at`,`i`.`resolved_at` AS `resolved_at` from ((((((`incidents` `i` join `users` `u` on((`i`.`created_by_user_id` = `u`.`user_id`))) join `categories` `c` on((`i`.`category_id` = `c`.`category_id`))) join `severity` `s` on((`i`.`severity_id` = `s`.`severity_id`))) left join `reports` `r` on((`i`.`incident_id` = `r`.`incident_id`))) left join `comments` `cm` on((`i`.`incident_id` = `cm`.`incident_id`))) left join `incident_files` `ifi` on((`i`.`incident_id` = `ifi`.`incident_id`))) group by `i`.`incident_id`,`i`.`incident_title`,`i`.`incident_description`,`c`.`category_name`,`s`.`severity_name`,`u`.`first_name`,`u`.`last_name`,`u`.`username`,`i`.`created_at`,`i`.`updated_at`,`i`.`resolved_at` */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Dumping events for database 'file_analysis_website'
--

--
-- Dumping routines for database 'file_analysis_website'
--
-- TODO: Add more database reporting objects:
-- views for report queues, user activity summaries, or file evidence summaries;
-- aggregation views/queries for counts by severity/category/status/user;
-- procedures such as close_incident, assign_report_to_incident, or add_comment_to_incident;
-- functions such as get_incident_age_days() or get_severity_rank();
-- triggers for incident creator role validation, resolved-incident comment prevention,
-- update auditing, or file hash validation.
/*!50003 DROP FUNCTION IF EXISTS `get_incident_status` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` FUNCTION `get_incident_status`(p_resolved_at datetime) RETURNS varchar(20) CHARSET utf8mb4
    DETERMINISTIC
begin
    if p_resolved_at is null then
        return 'open';
    else
        return 'resolved';
    end if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `create_incident_from_report` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `create_incident_from_report`(
    in p_report_id int unsigned,
    in p_analyst_id int unsigned,
    in p_category_id int unsigned,
    in p_severity_id int unsigned,
    in p_incident_title varchar(150),
    in p_incident_description text
)
begin
    declare v_incident_id int unsigned;

    start transaction;

    insert into incidents (
        created_by_user_id,
        category_id,
        severity_id,
        incident_title,
        incident_description
    )
    values (
        p_analyst_id,
        p_category_id,
        p_severity_id,
        p_incident_title,
        p_incident_description
    );

    set v_incident_id = last_insert_id();

    update reports
    set incident_id = v_incident_id,
        status = 'linked'
    where report_id = p_report_id;

    commit;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed
