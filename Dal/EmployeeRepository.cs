using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using CuyahogaHHS.Bll;

using Template.Bll;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Template.Dal
{
    public class EmployeeRepository : IEmployeeRepository, IDisposable
    {
        private readonly EmployeeContext _Context;
        private readonly ILogger _Logger;

        public EmployeeRepository(EmployeeContext context, ILogger<EmployeeRepository> logger)
        {
            _Context = context;
            _Logger = logger;
        }

        #region IEmployeeRepository
        public async Task<EmployeeEditor> FetchEditorAsync(int key)
        {
            var data = await _Context.Employees
                .Include("EmployeeIdentifiers.Identifier")
                .SingleOrDefaultAsync(e => e.Key == key);
            if (data == null)
                return null;
            return BuildEditor(data);
        }

        public async Task<IList<IdentifierType>> FetchIdentifierTypesAsync()
        {
            return await _Context.IdentifierTypes
                .Select(o => new IdentifierType
                {
                    Key = o.Key,
                    Name = o.IdentifierName
                })
                .ToListAsync();
        }

        public async Task<IList<EmployeeItem>> FetchListAsync()
        {
            return await _Context.Employees
                .Select(o => new EmployeeItem
                {
                    Key = o.Key,
                    Concurrency = o.Concurrency,
                    Name = $"{o.LastName }, {o.FirstName} {o.MiddleName}".Trim()
                })
                .ToListAsync();
        }

        public async Task<ConcurrencyResult<EmployeeEditor>> SaveAsync(EmployeeEditor data)
        {
            Validator.ValidateObject(data, new ValidationContext(data), validateAllProperties: true);
            foreach (var identifier in data.Identifiers)
                Validator.ValidateObject(identifier, new ValidationContext(identifier), validateAllProperties: true);
            await _Context.Database.BeginTransactionAsync();
            var dbData = await _Context.Employees
                .Include("EmployeeIdentifiers.Identifier")
                .SingleOrDefaultAsync(e => e.Key == data.Key);
            if (dbData == null || !Enumerable.SequenceEqual(dbData.Concurrency, data.Concurrency))
            {
                _Logger.LogWarning("Concurrency error while saving employee({0})", data.Key);
                return new ConcurrencyResult<EmployeeEditor>(BuildEditor(dbData), concurrencyError: true);
            }
            var q = CreateQuery(data.Identifiers);
            var source = await _Context.Identifiers
                .FromSql(q.sql, q.parameters)
                .Select(o => new Dto.EmployeeIdentifier
                {
                    EmployeeKey = dbData.Key,
                    Employee = dbData,
                    IdentifierKey = o.Key,
                    Identifier = o
                })
                .ToArrayAsync();
            var target = dbData.EmployeeIdentifiers.ToArray();
            dbData.EmployeeIdentifiers.RemoveAll(o => target.Except(source).Contains(o));
            dbData.EmployeeIdentifiers.AddRange(source.Except(target));
            _Context.Entry(dbData).State = EntityState.Modified;
            try
            {
                await _Context.SaveChangesAsync(true);
                _Context.Database.CommitTransaction();
            }
            catch (DbUpdateConcurrencyException)
            {
                _Logger.LogWarning("Concurrency error while saving employee({0})", data.Key);
                return new ConcurrencyResult<EmployeeEditor>(await FetchEditorAsync(data.Key), concurrencyError: true);
            }
            return new ConcurrencyResult<EmployeeEditor>(BuildEditor(dbData));
        }

        private EmployeeEditor BuildEditor(Dto.Employee data)
        {
            return new EmployeeEditor
            {
                Key = data.Key,
                Concurrency = data.Concurrency,
                Name = $"{data.LastName}, {data.FirstName} {data.MiddleName}".Trim(),
                Identifiers = new List<EmployeeEditor.Identifier>(data.EmployeeIdentifiers
                    .Select(ei => new EmployeeEditor.Identifier
                    {
                        Key = ei.IdentifierKey,
                        Value = ei.Identifier.IdentifierValue,
                        IdentifierTypeKey = ei.Identifier.IdentifierTypeKey
                    }))
            };
        }

        private SqlParameter CreateTableParam(IEnumerable<EmployeeEditor.Identifier> identifiers)
        {
            // Create a table that matches SQL Server user defined type (UDT) dbo.IdentifierType
            var data = new DataTable();
            data.Columns.Add("IdentifierKey", typeof(int));
            data.Columns.Add("IdentifierTypeKey", typeof(int));
            data.Columns.Add("IdentifierValue", typeof(string));
            // Load the table with the list of identifiers to be associated with the employee.
            foreach (var identifier in identifiers)
                data.Rows.Add(null, identifier.Key, identifier.Value);
            // Create a parameter with the new data.
            var parameter = new SqlParameter("Identifiers", SqlDbType.Udt)
            {
                UdtTypeName = "Identifier_T",
                Value = data
            };
            return parameter;
        }


        private (string sql, SqlParameter[] parameters) CreateQuery(IEnumerable<EmployeeEditor.Identifier> identifiers)
        {
            if (identifiers.Count() == 0)
                return (
                    sql: "SELECT IdentifierKey, IdentifierTypeKey, IdentifierValue FROM dbo.Identifiers WHERE 0=1",
                    parameters: new SqlParameter[] { });
            var sql = new StringBuilder();
            var parameters = new List<SqlParameter>();
            var i = 0;
            sql.AppendLine("MERGE INTO dbo.Identifiers").Append("USING(VALUES ");
            foreach (var identifier in identifiers)
            {
                parameters.Add(new SqlParameter($"@T{i}", identifier.IdentifierTypeKey));
                parameters.Add(new SqlParameter($"@V{i}", identifier.Value));
                if (i > 0)
                    sql.Append(",");
                sql.Append($"(@T{i},@V{i})");
                ++i;
            }
            sql.AppendLine(") AS src (srcIdentifierTypeKey, srcIdentifierValue)");
            sql.AppendLine("ON IdentifierTypeKey = srcIdentifierTypeKey AND IdentifierValue = srcIdentifierValue");
            sql.AppendLine("WHEN MATCHED THEN UPDATE SET IdentifierValue = srcIdentifierValue");
            sql.AppendLine("WHEN NOT MATCHED THEN INSERT(IdentifierTypeKey, IdentifierValue) VALUES(srcIdentifierTypeKey, srcIdentifierValue)");
            sql.AppendLine("OUTPUT INSERTED.IdentifierKey, INSERTED.IdentifierTypeKey, INSERTED.IdentifierValue; ");
            return (sql: sql.ToString(), parameters: parameters.ToArray());
        }
        #endregion

        #region IDisposable interface
        public void Dispose()
        {
            _Context?.Dispose();
        }
        #endregion
    }
}
