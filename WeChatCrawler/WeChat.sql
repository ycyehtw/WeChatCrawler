PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: ExportedPost
DROP TABLE IF EXISTS ExportedPost;

CREATE TABLE ExportedPost (
  PostId       INTEGER,
  PostedDate   DATETIME,
  ExportedDate DATETIME,
  PRIMARY KEY (
    PostId,
    PostedDate
  )
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
  Id       INTEGER PRIMARY KEY,
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


-- Index: Idx_Post_Date
DROP INDEX IF EXISTS Idx_Post_Date;

CREATE INDEX Idx_Post_Date ON Post (
  Date
);


-- Index: Idx_Post_VendorId_Status_Date
DROP INDEX IF EXISTS Idx_Post_VendorId_Status_Date;

CREATE INDEX Idx_Post_VendorId_Status_Date ON Post (
  VendorId,
  Status,
  Date DESC
);


-- Index: Idx_Vendor_IsEnabled_Code
DROP INDEX IF EXISTS Idx_Vendor_IsEnabled_Code;

CREATE INDEX Idx_Vendor_IsEnabled_Code ON Vendor (
  IsEnabled,
  Code
);


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
