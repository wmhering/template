CREATE TABLE dbo.IdentifierTypes (
  IdentifierTypeKey INT IDENTITY(1,1) NOT NULL,
  IdentifierName VARCHAR(100) NOT NULL,
  AllowMultipleEmployees BIT NOT NULL,
  AllowMultipleIdentifiers BIT NOT NULL,
  CONSTRAINT IdentifierTypes_PK
    PRIMARY KEY (IdentifierTypeKey)
)
