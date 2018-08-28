using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using CuyahogaHHS.Bll;

using Template.Bll;

namespace Template.Dal
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeContext _Context;
        private readonly ILogger _Logger;

        public EmployeeRepository(EmployeeContext context, ILogger<EmployeeRepository> logger)
        {
            _Context = context;
            _Logger = logger;
        }

        public EmployeeRepository(EmployeeContext context)
        {
            _Context = context;
        }

        public async Task<ConcurrencyResult<EmployeeEditor>> Delete(int key, byte[] concurrency)
        {
            // Fetch existing employee data
            var data = await FetchEditor(key);
            // Employee has already been deleted, return success.
            if (data == null)
                return new ConcurrencyResult<EmployeeEditor>(null);
            // If the concurrency value has changed then return concurrency error.
            if (!Enumerable.SequenceEqual(data.Concurrency, concurrency))
            {
                _Logger.LogWarning("Concurrency error while deleting employee({0}).", key);
                return new ConcurrencyResult<EmployeeEditor>(data, concurrencyError: true);
            }
            // Delete the record from the database.
            try
            {
                var employee = await _Context.Employees.FindAsync(key);
                _Context.RemoveRange(employee.EmployeeIdentifiers);
                _Context.Remove(employee);
                await _Context.SaveChangesAsync();
                return new ConcurrencyResult<EmployeeEditor>(null);
            }
            // If the database detected a conflict then return concurrency error.
            catch (DbUpdateConcurrencyException ex)
            {
                _Logger.LogWarning("Concurrency error while deleting employee({0}).", key);
                return new ConcurrencyResult<EmployeeEditor>(await FetchEditor(key), concurrencyError: true);
            }
        }

        public async Task<EmployeeEditor> FetchEditor(int key)
        {
            var data = await _Context.Employees
                .Include("EmployeeIdentifiers.Identifier")
                .SingleOrDefaultAsync(e => e.Key == key);
            if (data == null)
                return null;            
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

        public async Task<IList<IdentifierType>> FetchIdentifierTypes()
        {
            return await _Context.IdentifierTypes
                .Select(o => new IdentifierType
                {
                    Key = o.Key,
                    Name = o.IdentifierName
                })
                .ToListAsync();
        }

        public async Task<IList<EmployeeItem>> FetchList()
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

        public async Task<ConcurrencyResult<EmployeeEditor>> Save(EmployeeEditor data)
        {
            return await Task.Run<ConcurrencyResult<EmployeeEditor>>(() => new ConcurrencyResult<EmployeeEditor>(data));
        }
    }
}
