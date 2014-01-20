SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

--
-- Database: `d2dbs`
--

-- --------------------------------------------------------

--
-- Table structure for table `charstat`
--

CREATE TABLE IF NOT EXISTS `charstat` (
  `charname` varchar(15) NOT NULL DEFAULT '',
  `accname` varchar(15) DEFAULT NULL,
  `ist` smallint(5) unsigned DEFAULT NULL,
  `lastupdate` int(11) NOT NULL,
  `charclass` tinyint(4) NOT NULL,
  `level` tinyint(4) NOT NULL,
  `version` tinyint(4) NOT NULL,
  PRIMARY KEY (`charname`),
  KEY `accname` (`accname`),
  KEY `version` (`version`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `dc_trigger`
--

CREATE TABLE IF NOT EXISTS `dc_trigger` (
  `date` int(11) NOT NULL DEFAULT '0',
  `counter` int(11) DEFAULT NULL,
  `next_counter` int(11) DEFAULT NULL,
  PRIMARY KEY (`date`),
  KEY `date` (`date`) USING BTREE
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `diablo_clone`
--

CREATE TABLE IF NOT EXISTS `diablo_clone` (
  `date` int(11) NOT NULL DEFAULT '0',
  `realm` varchar(32) NOT NULL DEFAULT '',
  `count` int(11) DEFAULT NULL,
  PRIMARY KEY (`date`,`realm`),
  UNIQUE KEY `date` (`date`,`realm`) USING BTREE
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ecostat`
--

CREATE TABLE IF NOT EXISTS `ecostat` (
  `tm` int(11) NOT NULL,
  `produce` int(11) NOT NULL,
  `exchange` int(11) NOT NULL,
  `version` tinyint(4) NOT NULL,
  PRIMARY KEY (`tm`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `emergency`
--

CREATE TABLE IF NOT EXISTS `emergency` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `date` int(11) NOT NULL,
  `gs_addr` varchar(16) NOT NULL,
  `type` int(11) NOT NULL,
  `param` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `date` (`date`),
  KEY `gs_addr` (`gs_addr`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `itemrecord`
--

CREATE TABLE IF NOT EXISTS `itemrecord` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `date` int(11) NOT NULL,
  `game` varchar(32) NOT NULL,
  `charname` varchar(15) DEFAULT NULL,
  `accname` varchar(15) DEFAULT NULL,
  `ipaddr` varchar(15) DEFAULT NULL,
  `itemchange` text NOT NULL,
  `istchange` smallint(6) DEFAULT NULL,
  `version` tinyint(4) NOT NULL DEFAULT '11',
  PRIMARY KEY (`id`),
  KEY `date` (`date`),
  KEY `version` (`version`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `performance`
--

CREATE TABLE IF NOT EXISTS `performance` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `date` int(11) NOT NULL,
  `gs_addr` varchar(16) NOT NULL,
  `busy_pool` int(11) NOT NULL,
  `idle_pool` int(11) NOT NULL,
  `thread_num` int(11) NOT NULL,
  `handle_num` int(11) NOT NULL,
  `ge_num` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `date` (`date`),
  KEY `gs_addr` (`gs_addr`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `rate_exceeded`
--

CREATE TABLE IF NOT EXISTS `rate_exceeded` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `date` int(11) NOT NULL,
  `gs_addr` varchar(16) NOT NULL,
  `charname` varchar(20) NOT NULL,
  `accname` varchar(20) NOT NULL,
  `ipaddr` varchar(16) NOT NULL,
  `rate` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `gs_addr` (`gs_addr`),
  KEY `date` (`date`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `game`
--

CREATE TABLE IF NOT EXISTS `game` (
  `gid` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(32) NOT NULL,
  `startdate` int(11) NOT NULL,
  `enddate` int(11) NOT NULL,
  `produce` smallint(6) DEFAULT '0',
  `exchange` smallint(6) DEFAULT '0',
  `version` tinyint(4) NOT NULL DEFAULT '11',
  PRIMARY KEY (`gid`),
  KEY `name` (`name`),
  KEY `startdate` (`startdate`),
  KEY `version` (`version`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 ;
