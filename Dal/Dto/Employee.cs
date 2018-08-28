using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Dal.Dto
{
    [Table("Employees")]
    public class Employee
    {
        #region Identification
        [Key, Column("EmployeeKey")]
        public int Key { get; set; }

        [ConcurrencyCheck]
        public byte[] Concurrency { get; set; }
        #endregion

        #region Navigation
        public List<EmployeeIdentifier> EmployeeIdentifiers { get; set; }
        #endregion

        #region Properties
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }
        #endregion
    }
}
