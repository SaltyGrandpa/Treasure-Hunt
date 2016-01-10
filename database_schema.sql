SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

--
-- Database: `SeniorProject`
--

-- --------------------------------------------------------

--
-- Table structure for table `augmented`
--

CREATE TABLE IF NOT EXISTS `augmented` (
  `id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Scene ID',
  `name` varchar(128) NOT NULL DEFAULT 'Unavailable' COMMENT 'Scene Name',
  `description` varchar(256) NOT NULL DEFAULT 'No description available' COMMENT 'Scene Description',
  `data` longtext NOT NULL COMMENT 'Serialized Scene Data',
  `longitude` float NOT NULL COMMENT 'Longitude of Scene',
  `latitude` float NOT NULL COMMENT 'Latitude of Scene',
  `votes_up` int(11) NOT NULL DEFAULT '0' COMMENT 'Up Votes of Scene',
  `votes_down` int(11) NOT NULL DEFAULT '0' COMMENT 'Down Votes of Scene',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=20 ;
