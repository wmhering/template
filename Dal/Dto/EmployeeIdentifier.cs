using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Template.Dal.Dto
{
    [Table("EmployeeIdentifiers")]
    public class EmployeeIdentifier
    {
        #region Identification
        [ForeignKey(nameof(Employee))]
        public int EmployeeKey { get; set; }

        [ForeignKey(nameof(Identifier))]
        public int IdentifierKey { get; set; }
        #endregion

        #region Navigation
        [ForeignKey(nameof(EmployeeKey))]
        public Employee Employee { get; set; }

        [ForeignKey(nameof(IdentifierKey))]
        public Identifier Identifier { get; set; }
        #endregion
    }
}
