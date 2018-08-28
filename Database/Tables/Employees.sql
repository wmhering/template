CREATE TABLE dbo.Employees (
  EmployeeKey INT NOT NULL,
  Concurrency ROWVERSION NOT NULL,
  FirstName VARCHAR(25) NOT NULL,
  MiddleName VARCHAR(25) NOT NULL,
  LastName VARCHAR(25) NOT NULL,
  CONSTRAINT Employees_PK
    PRIMARY KEY (EmployeeKey)
);
