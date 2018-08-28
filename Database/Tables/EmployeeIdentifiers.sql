CREATE TABLE dbo.EmployeeIdentifiers (
  EmployeeKey INT NOT NULL,
  IdentifierKey INT NOT NULL,
  CONSTRAINT EmployeeIdentifiers_PK
    PRIMARY KEY (EmployeeKey, IdentifierKey),
  CONSTRAINT EmployeeIdentifiers_Employees_FK
    FOREIGN KEY (EmployeeKey)
    REFERENCES dbo.Employees (EmployeeKey),
  CONSTRAINT EmployeeIdentifiers_Identifiers_FK
    FOREIGN KEY (IdentifierKey)
    REFERENCES dbo.Identifiers (IdentifierKey)
)
