--
-- File generated with SQLiteStudio v3.2.1 on ¶g¤T ¤G¤ë 10 09:46:52 2021
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: ExportedPost
DROP TABLE IF EXISTS ExportedPost;

CREATE TABLE ExportedPost (
  PostId       INTEGER,
  PostedDate   DATETIME,
  ExportedDate DATETIME
);


-- Table: Picture
DROP TABLE IF EXISTS Picture;

CREATE TABLE Picture (
  PostId  INTEGER,
  Name    TEXT,
  IsFirst INTEGER,
  PRIMARY KEY (
    PostId,
    Name ASC
  )
);


-- Table: Post
DROP TABLE IF EXISTS Post;

CREATE TABLE Post (
  Id       INTEGER,
  VendorId INTEGER,
  Date     DATE,
  Content  TEXT,
  KrwPrice INTEGER,
  TwdPrice INTEGER,
  Spec     TEXT,
  Color    TEXT,
  Material TEXT,
  Status   INTEGER
);


-- Table: User
DROP TABLE IF EXISTS User;

CREATE TABLE User (
  Username TEXT PRIMARY KEY,
  Password TEXT
);


-- Table: Vendor
DROP TABLE IF EXISTS Vendor;

CREATE TABLE Vendor (
  VendorId  INTEGER,
  WeChatId  TEXT,
  Name      TEXT,
  Code      TEXT,
  IsEnabled BOOLEAN,
  PRIMARY KEY (
    VendorId ASC
  )
);


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
