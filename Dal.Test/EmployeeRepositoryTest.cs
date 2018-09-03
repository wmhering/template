using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using Template.Bll;

namespace Template.Dal.Test
{
    [TestClass]
    public class EmployeeRepositoryTest
    {
        private static string _ConnectionString = "Data Source=.;Initial Catalog=EmployeeInfo;Integrated Security=SSPI";
        private ILogger<EmployeeRepository> _MockLogger;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            #region var sql =
            var sql = @"
-- Delete old data
DELETE FROM dbo.EmployeeIdentifiers;
DELETE FROM dbo.Employees;
DELETE FROM dbo.Identifiers;
DELETE FROM dbo.IdentifierTypes;
-- Create new identifier types
SET IDENTITY_INSERT dbo.IdentifierTypes ON;
INSERT INTO dbo.IdentifierTypes (IdentifierTypeKey, IdentifierName, AllowMultipleEmployees, AllowMultipleIdentifiers)
VALUES (1, 'State Network ID',     0, 0),
       (2, 'State E-Mail Address', 0, 0),
       (3, 'State RACF ID',        0, 0);
SET IDENTITY_INSERT dbo.IdentifierTypes OFF;
DBCC CHECKIDENT ('dbo.IdentifierTypes', RESEED);
-- Create new identifiers
SET IDENTITY_INSERT dbo.Identifiers ON;
INSERT INTO dbo.Identifiers (IdentifierKey, IdentifierTypeKey, IdentifierValue)
VALUES (1, 1, '50018345'),
       (2, 1, '50018346'),
       (3, 1, '50018347'),
       (4, 2, 'donald.duck@jfs.ohio.gov'),
       (5, 2, 'micky.mouse@jfs.ohio.gov'),
       (6, 2, 'snow.white@jfs.ohio.gov');
SET IDENTITY_INSERT dbo.Identifiers OFF;
DBCC CHECKIDENT ('dbo.Identifiers', RESEED);
-- Create new employees
INSERT INTO dbo.Employees (EmployeeKey, FirstName, MiddleName, LastName)
VALUES (1, 'Donald', '', 'Duck'),
       (2, 'Micky', '', 'Mouse'),
       (3, 'Snow', '', 'White');
-- Create new employee identifiers
INSERT INTO dbo.EmployeeIdentifiers (EmployeeKey, IdentifierKey)
VALUES (1, 1),
       (2, 2),
       (3, 3),
       (1, 4),
       (2, 5),
       (3, 6);
";
            #endregion
            using (var connection = new SqlConnection(_ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _MockLogger = new Mock<ILogger<EmployeeRepository>>(MockBehavior.Loose).Object;
        }

        private EmployeeContext GetDbContext()
        {
            return new EmployeeContext(new DbContextOptionsBuilder<EmployeeContext>()
                .UseSqlServer(_ConnectionString).Options);
        }

        [TestMethod]
        public void FetchEditorAsync_Should_Return_Data_When_Employee_Exist()
        {
            var unit = new EmployeeRepository(GetDbContext(), _MockLogger);

            var data = unit.FetchEditorAsync(1).Result;

            Assert.IsNotNull(data);
            Assert.AreEqual<int>(1, data.Key);
        }

        [TestMethod]
        public void FetchEditorAsync_Should_Return_Null_When_Employee_Doesnt_Exist()
        {
            var unit = new EmployeeRepository(GetDbContext(), _MockLogger);

            var data = unit.FetchEditorAsync(4).Result;

            Assert.IsNull(data);
        }

        [TestMethod]
        [ExpectedException(typeof(SecurityException))]
        public void FetchEditorAsync_Should_Throw_Exception_When_User_Is_Unauthorized()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void FetchIdentifierTypesAsync_Should_Return_Data()
        {
            var unit = new EmployeeRepository(GetDbContext(), _MockLogger);

            var data = unit.FetchIdentifierTypesAsync().Result;

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Count > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(SecurityException))]
        public void FetchIdentifierTypesAsync_Should_Throw_Exception_When_User_Is_Unauthorized()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void FetchListAsync_Should_Return_Data()
        {
            var unit = new EmployeeRepository(GetDbContext(), _MockLogger);

            var data = unit.FetchListAsync().Result;

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Count > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(SecurityException))]
        public void FetchListAsync_Should_Throw_Exception_When_User_Is_Unauthorized()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void SaveAsync_Should_Log_Concurreny_Conflicts()
        {
            var repository = new EmployeeRepository(GetDbContext(), _MockLogger);
            var data = repository.FetchEditorAsync(1).Result;
            var sql = "UPDATE Employees SET MiddleName = MiddleName + 'X' WHERE EmployeeKey = 1";
            using (var connection = new SqlConnection(_ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            var mockLogger = new Mock<ILogger<EmployeeRepository>>(MockBehavior.Loose);
            mockLogger.Setup(l => l.Log<Object>(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<Object>(), null, It.IsAny<Func<object, Exception, string>>()));
            var unit = new EmployeeRepository(GetDbContext(), mockLogger.Object);

            var result = unit.SaveAsync(data).Result;

            mockLogger.Verify();
            Assert.IsTrue(result.ConcurrencyError, "There should be a concurrency error.");
        }

        [TestMethod]
        public void SaveAsync_Should_Return_Other_Users_Changes_When_There_Is_A_Conflict()
        {
            var repository = new EmployeeRepository(GetDbContext(), _MockLogger);
            var data = repository.FetchEditorAsync(1).Result;
            var sql = "UPDATE Employees SET MiddleName = MiddleName + 'X' WHERE EmployeeKey = 1";
            using (var connection = new SqlConnection(_ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            var unit = new EmployeeRepository(GetDbContext(), _MockLogger);

            var result = unit.SaveAsync(data).Result;

            Assert.IsTrue(result.ConcurrencyError, "There should be a concurrency error.");
            CollectionAssert.AreNotEqual(result.Data.Concurrency, data.Concurrency, "Concurrency field should have changed.");
            StringAssert.Contains(result.Data.Name, "X", "Name should contain X.");
        }

        [TestMethod]
        public void SaveAsync_Should_Return_Updated_Data_When_There_Is_Not_A_Conflict()
        {
            var repository = new EmployeeRepository(GetDbContext(), _MockLogger);
            var data = repository.FetchEditorAsync(1).Result;
            data.Identifiers.Add(new EmployeeEditor.Identifier { IdentifierTypeKey = 3, Value = "WXXX18" });
            var unit = new EmployeeRepository(GetDbContext(), _MockLogger);

            var result = unit.SaveAsync(data).Result;

            Assert.IsFalse(result.ConcurrencyError);
            CollectionAssert.AreNotEqual(data.Concurrency, result.Data.Concurrency);
            Assert.AreEqual(1, result.Data.Identifiers.FindAll(o => o.IdentifierTypeKey == 3).Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void SaveAsync_Should_Throw_Exception_When_Data_Isnt_Valid()
        {
            var data = new EmployeeEditor
            {
                Key = 1,
                Concurrency = new byte[8],
                Identifiers = new List<EmployeeEditor.Identifier>(new EmployeeEditor.Identifier[] {
                    new EmployeeEditor.Identifier { Key = 1, IdentifierTypeKey = 0, Value = new string('*', 101)}
                    })
            };
            var unit = new EmployeeRepository(GetDbContext(), _MockLogger);
            try
            {
                var result = unit.SaveAsync(data).Result;
                Assert.Fail(); // We should not get here
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(AggregateException))
                    throw ((AggregateException)ex).InnerExceptions[0];
                throw;
            }            
        }

        [TestMethod]
        [ExpectedException(typeof(SecurityException))]
        public void SaveAsync_Should_Throw_Exception_When_User_Is_Unauthorized()
        {
            Assert.Inconclusive();
        }
    }
}
