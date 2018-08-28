CREATE TABLE dbo.Identifiers (
  IdentifierKey INT IDENTITY(1,1) NOT NULL,
  IdentifierTypeKey INT NOT NULL,
  IdentifierValue VARCHAR(100) NOT NULL,
  CONSTRAINT Identifiers_PK
    PRIMARY KEY (IdentifierKey),
  CONSTRAINT Identifiers_IdentifierTypes_FK
    FOREIGN KEY (IdentifierTypeKey)
    REFERENCES dbo.IdentifierTypes (IdentifierTypeKey)
);

