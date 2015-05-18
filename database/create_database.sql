CREATE TABLE `users` (
  `Id` varchar(50) NOT NULL,
  `UserName` varchar(100) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `PasswordHash` varchar(250) NOT NULL,
  `SecurityStamp` varchar(250) DEFAULT NULL,
  `FirstName` varchar(50) DEFAULT NULL,
  `LastName` varchar(50) DEFAULT NULL,
  PRIMARY KEY  (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
CREATE TABLE `externallogins` (
  `Id` varchar(50) NOT NULL,
  `UserId` varchar(50) NOT NULL,
  `LoginProvider` varchar(250) NOT NULL,
  `ProviderKey` varchar(250) NOT NULL,
  PRIMARY KEY  (`Id`),
  KEY `FK_UserId` (`UserId`),
  CONSTRAINT `FK_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
CREATE TABLE `roles` (
  `Id` varchar(50) NOT NULL,
  `Name` varchar(100) NOT NULL,
  PRIMARY KEY  (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
CREATE TABLE `userroles` (
  `UserId` varchar(50) NOT NULL,
  `RoleId` varchar(50) NOT NULL,
  PRIMARY KEY  (`UserId`,`RoleId`),
  KEY `FK_RoleUserId` (`UserId`),
  CONSTRAINT `FK_RoleUserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`),
  CONSTRAINT `FK_RoleRoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

INSERT INTO auth.roles(
   Id
  ,Name
) 
  SELECT UUID(),
  "User";

  INSERT INTO auth.roles(
   Id
  ,Name
) 
  SELECT UUID(),
  "Doctor";

INSERT INTO auth.roles(
   Id
  ,Name
)  SELECT UUID(),
  "Admin";
