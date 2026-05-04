-- Full database rebuild for the File Analysis Website project.
--
-- Run this from the project root with the MySQL command-line client.
-- PowerShell:
-- cmd /c "mysql -u root -p < Database_Schema\full_database_build.sql"
-- Command Prompt, Git Bash, or WSL:
-- mysql -u root -p < Database_Schema/full_database_build.sql
--
-- This file is self-contained. Do not run it through SOURCE/includes.
-- It intentionally drops and recreates the project database.

DROP DATABASE IF EXISTS `file_analysis_website`;
CREATE DATABASE `file_analysis_website`
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;
USE `file_analysis_website`;

-- Tables, seed data, routines, triggers, and views in dependency-safe order.

-- ============================================================================
-- Begin file_analysis_website_roles.sql
-- ============================================================================
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
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `role_id` int unsigned NOT NULL AUTO_INCREMENT,
  `role_name` varchar(30) NOT NULL,
  PRIMARY KEY (`role_id`),
  UNIQUE KEY `role_name` (`role_name`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'admin'),(3,'analyst'),(2,'user');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_roles.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_users.sql
-- ============================================================================
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
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `user_id` int unsigned NOT NULL AUTO_INCREMENT,
  `role_id` int unsigned NOT NULL,
  `last_name` varchar(50) NOT NULL,
  `first_name` varchar(50) NOT NULL,
  `username` varchar(50) NOT NULL,
  `email` varchar(255) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `last_login_at` datetime DEFAULT NULL,
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `username` (`username`),
  UNIQUE KEY `email` (`email`),
  KEY `fk_users_role` (`role_id`),
  CONSTRAINT `fk_users_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,2,'test','test','test','test@gmail.com','9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08','2026-04-04 13:59:34','2026-04-04 13:59:38','2026-04-04 13:59:40'),(2,3,'Sakamoto de Toledo','Gustavo','groodinwar','gustavosakamotox@gmail.com','0dc3c3921b7ab6d5b6622e2487a701fec4a9c314cf68c0439628679909ab96ac','2026-04-29 22:41:25','2026-04-30 21:21:16','2026-04-30 21:21:16');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_users.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_categories.sql
-- ============================================================================
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
-- Table structure for table `categories`
--

DROP TABLE IF EXISTS `categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categories` (
  `category_id` int unsigned NOT NULL AUTO_INCREMENT,
  `category_name` varchar(50) NOT NULL,
  PRIMARY KEY (`category_id`),
  UNIQUE KEY `category_name` (`category_name`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categories`
--

LOCK TABLES `categories` WRITE;
/*!40000 ALTER TABLE `categories` DISABLE KEYS */;
INSERT INTO `categories` VALUES (1,'malware'),(9,'other'),(2,'phishing'),(3,'ransomware'),(4,'spyware'),(5,'trojans'),(6,'viruses'),(7,'worms'),(8,'zero-day');
/*!40000 ALTER TABLE `categories` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_categories.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_severity.sql
-- ============================================================================
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
-- Table structure for table `severity`
--

DROP TABLE IF EXISTS `severity`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `severity` (
  `severity_id` int unsigned NOT NULL AUTO_INCREMENT,
  `severity_name` varchar(20) NOT NULL,
  PRIMARY KEY (`severity_id`),
  UNIQUE KEY `severity_name` (`severity_name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `severity`
--

LOCK TABLES `severity` WRITE;
/*!40000 ALTER TABLE `severity` DISABLE KEYS */;
INSERT INTO `severity` VALUES (4,'critical'),(3,'high'),(1,'low'),(2,'medium');
/*!40000 ALTER TABLE `severity` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_severity.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_incidents.sql
-- ============================================================================
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
-- Table structure for table `incidents`
--

DROP TABLE IF EXISTS `incidents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `incidents` (
  `incident_id` int unsigned NOT NULL AUTO_INCREMENT,
  `created_by_user_id` int unsigned NOT NULL,
  `category_id` int unsigned NOT NULL,
  `severity_id` int unsigned NOT NULL,
  `incident_title` varchar(150) NOT NULL,
  `incident_description` text NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `resolved_at` datetime DEFAULT NULL,
  PRIMARY KEY (`incident_id`),
  KEY `fk_incidents_creator` (`created_by_user_id`),
  KEY `fk_incidents_category` (`category_id`),
  KEY `fk_incidents_severity` (`severity_id`),
  CONSTRAINT `fk_incidents_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`category_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_incidents_creator` FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_incidents_severity` FOREIGN KEY (`severity_id`) REFERENCES `severity` (`severity_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `incidents`
--

LOCK TABLES `incidents` WRITE;
/*!40000 ALTER TABLE `incidents` DISABLE KEYS */;
/*!40000 ALTER TABLE `incidents` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_incidents.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_reports.sql
-- ============================================================================
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
-- Table structure for table `reports`
--

DROP TABLE IF EXISTS `reports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reports` (
  `report_id` int unsigned NOT NULL AUTO_INCREMENT,
  `submitted_by_user_id` int unsigned NOT NULL,
  `incident_id` int unsigned DEFAULT NULL,
  `title` varchar(150) NOT NULL,
  `report_text` text NOT NULL,
  `status` enum('submitted','under_review','linked','closed','rejected') NOT NULL DEFAULT 'submitted',
  `submitted_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`report_id`),
  KEY `fk_reports_submitter` (`submitted_by_user_id`),
  KEY `fk_reports_incident` (`incident_id`),
  CONSTRAINT `fk_reports_incident` FOREIGN KEY (`incident_id`) REFERENCES `incidents` (`incident_id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_reports_submitter` FOREIGN KEY (`submitted_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reports`
--

LOCK TABLES `reports` WRITE;
/*!40000 ALTER TABLE `reports` DISABLE KEYS */;
INSERT INTO `reports` VALUES (5,1,NULL,'Test #1','Hello!','submitted','2026-04-27 22:36:26','2026-04-27 22:36:26');
/*!40000 ALTER TABLE `reports` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `reports_before_update_validate_status` BEFORE UPDATE ON `reports` FOR EACH ROW begin

    if old.status <> new.status then



        if old.status = 'submitted'

           and new.status not in ('under_review', 'linked', 'rejected') then

            signal sqlstate '45000'

                set message_text = 'Invalid report status transition from submitted.';

        end if;



        if old.status = 'under_review'

           and new.status not in ('linked', 'rejected', 'closed') then

            signal sqlstate '45000'

                set message_text = 'Invalid report status transition from under_review.';

        end if;



        if old.status = 'linked'

           and new.status not in ('closed') then

            signal sqlstate '45000'

                set message_text = 'Invalid report status transition from linked.';

        end if;



        if old.status in ('closed', 'rejected')

           and new.status <> old.status then

            signal sqlstate '45000'

                set message_text = 'Closed or rejected reports cannot change status.';

        end if;



    end if;



    if new.status in ('linked', 'closed') and new.incident_id is null then

        signal sqlstate '45000'

            set message_text = 'A linked or closed report must reference an incident.';

    end if;



    if new.status = 'rejected' and new.incident_id is not null then

        signal sqlstate '45000'

            set message_text = 'A rejected report cannot reference an incident.';

    end if;

end */;;
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

-- ============================================================================
-- End file_analysis_website_reports.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_files.sql
-- ============================================================================
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
-- Table structure for table `files`
--

DROP TABLE IF EXISTS `files`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `files` (
  `file_id` int unsigned NOT NULL AUTO_INCREMENT,
  `file_name` varchar(255) NOT NULL,
  `file_path` varchar(500) NOT NULL,
  `file_hash` char(64) NOT NULL,
  `uploaded_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`file_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `files`
--

LOCK TABLES `files` WRITE;
/*!40000 ALTER TABLE `files` DISABLE KEYS */;
/*!40000 ALTER TABLE `files` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_files.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_comments.sql
-- ============================================================================
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
-- Table structure for table `comments`
--

DROP TABLE IF EXISTS `comments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `comments` (
  `comment_id` int unsigned NOT NULL AUTO_INCREMENT,
  `incident_id` int unsigned NOT NULL,
  `user_id` int unsigned NOT NULL,
  `comment_text` text NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`comment_id`),
  KEY `fk_comments_incident` (`incident_id`),
  KEY `fk_comments_user` (`user_id`),
  CONSTRAINT `fk_comments_incident` FOREIGN KEY (`incident_id`) REFERENCES `incidents` (`incident_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_comments_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `comments`
--

LOCK TABLES `comments` WRITE;
/*!40000 ALTER TABLE `comments` DISABLE KEYS */;
/*!40000 ALTER TABLE `comments` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_comments.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_report_files.sql
-- ============================================================================
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
-- Table structure for table `report_files`
--

DROP TABLE IF EXISTS `report_files`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `report_files` (
  `report_id` int unsigned NOT NULL,
  `file_id` int unsigned NOT NULL,
  PRIMARY KEY (`report_id`,`file_id`),
  KEY `fk_report_files_file` (`file_id`),
  CONSTRAINT `fk_report_files_file` FOREIGN KEY (`file_id`) REFERENCES `files` (`file_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_report_files_report` FOREIGN KEY (`report_id`) REFERENCES `reports` (`report_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `report_files`
--

LOCK TABLES `report_files` WRITE;
/*!40000 ALTER TABLE `report_files` DISABLE KEYS */;
/*!40000 ALTER TABLE `report_files` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_report_files.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_incident_files.sql
-- ============================================================================
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
-- Table structure for table `incident_files`
--

DROP TABLE IF EXISTS `incident_files`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `incident_files` (
  `incident_id` int unsigned NOT NULL,
  `file_id` int unsigned NOT NULL,
  PRIMARY KEY (`incident_id`,`file_id`),
  KEY `fk_incident_files_file` (`file_id`),
  CONSTRAINT `fk_incident_files_file` FOREIGN KEY (`file_id`) REFERENCES `files` (`file_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_incident_files_incident` FOREIGN KEY (`incident_id`) REFERENCES `incidents` (`incident_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `incident_files`
--

LOCK TABLES `incident_files` WRITE;
/*!40000 ALTER TABLE `incident_files` DISABLE KEYS */;
/*!40000 ALTER TABLE `incident_files` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

-- ============================================================================
-- End file_analysis_website_incident_files.sql
-- ============================================================================

-- ============================================================================
-- Begin file_analysis_website_routines.sql
-- ============================================================================
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

-- ============================================================================
-- End file_analysis_website_routines.sql
-- ============================================================================

-- Demo application data.
-- These accounts are mirrored by the EF Core runtime seeder. The password
-- values below are SHA-256 hashes accepted by the app's legacy-login fallback;
-- the app rehashes them with ASP.NET Identity after first login.
INSERT INTO `users` (
    `user_id`,
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
    (10, 1, 'User', 'Admin', 'admin', 'admin@example.com', '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    (11, 3, 'Analyst', 'Analyst', 'analyst', 'analyst@example.com', '84a6ed197836ce9fe88ed4cd036a048c7b01ce048ba35d8f9b2f7cf6bbc2970a', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    (12, 2, 'User', 'Uma', 'user', 'user@example.com', 'bc5848f227cc161eb5f68dfe98cb13110a9c843ce69e953a88107d865583d397', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

INSERT INTO `incidents` (
    `incident_id`,
    `created_by_user_id`,
    `category_id`,
    `severity_id`,
    `incident_title`,
    `incident_description`,
    `created_at`,
    `updated_at`,
    `resolved_at`
)
VALUES
    (10, 11, 2, 3, 'Credential phishing campaign', 'Multiple users reported password reset emails pointing to an external collection site.', CURRENT_TIMESTAMP - INTERVAL 3 DAY, CURRENT_TIMESTAMP - INTERVAL 1 DAY, NULL),
    (11, 11, 9, 2, 'VPN brute-force activity', 'Authentication logs show repeated failed VPN attempts against one account.', CURRENT_TIMESTAMP - INTERVAL 2 DAY, CURRENT_TIMESTAMP - INTERVAL 1 DAY, NULL),
    (12, 10, 3, 4, 'Ransomware note discovered', 'A demo ransomware note was uploaded for triage and containment practice.', CURRENT_TIMESTAMP - INTERVAL 4 DAY, CURRENT_TIMESTAMP - INTERVAL 1 DAY, CURRENT_TIMESTAMP - INTERVAL 1 DAY);

INSERT INTO `reports` (
    `report_id`,
    `submitted_by_user_id`,
    `incident_id`,
    `title`,
    `report_text`,
    `status`,
    `submitted_at`,
    `updated_at`
)
VALUES
    (10, 12, 10, 'Suspicious password reset email', 'A user received an email asking them to reset their password through an unknown link.', 'linked', CURRENT_TIMESTAMP - INTERVAL 4 DAY, CURRENT_TIMESTAMP - INTERVAL 3 DAY),
    (11, 12, NULL, 'Repeated VPN login failures', 'Several failed VPN login attempts were observed against a single account.', 'under_review', CURRENT_TIMESTAMP - INTERVAL 2 DAY, CURRENT_TIMESTAMP - INTERVAL 1 DAY);

INSERT INTO `comments` (
    `comment_id`,
    `incident_id`,
    `user_id`,
    `comment_text`,
    `created_at`,
    `updated_at`
)
VALUES
    (10, 10, 11, 'Initial triage completed. Blocking indicators and preserving email evidence.', CURRENT_TIMESTAMP - INTERVAL 2 DAY, CURRENT_TIMESTAMP - INTERVAL 2 DAY),
    (11, 10, 10, 'Escalated to admin review for tenant-wide mailbox search.', CURRENT_TIMESTAMP - INTERVAL 1 DAY, CURRENT_TIMESTAMP - INTERVAL 1 DAY),
    (12, 11, 11, 'Waiting for additional logs before closing this investigation.', CURRENT_TIMESTAMP - INTERVAL 1 DAY, CURRENT_TIMESTAMP - INTERVAL 1 DAY);

INSERT INTO `files` (
    `file_id`,
    `file_name`,
    `file_path`,
    `file_hash`,
    `uploaded_at`
)
VALUES
    (10, 'phishing-email.eml', 'files/demo_phishing-email.eml', 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa', CURRENT_TIMESTAMP - INTERVAL 3 DAY),
    (11, 'vpn-auth-log.txt', 'files/demo_vpn-auth-log.txt', 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb', CURRENT_TIMESTAMP - INTERVAL 2 DAY),
    (12, 'ransom-note.txt', 'files/demo_ransom-note.txt', 'cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc', CURRENT_TIMESTAMP - INTERVAL 1 DAY);

INSERT INTO `report_files` (`report_id`, `file_id`)
VALUES
    (10, 10),
    (11, 11);

INSERT INTO `incident_files` (`incident_id`, `file_id`)
VALUES
    (10, 10),
    (11, 11),
    (12, 12);

-- CRUD examples are kept separate because they intentionally roll back.
-- Run Database_Schema/crud_examples.sql after the rebuild when you want examples.

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
