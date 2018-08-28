using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Dal.Dto
{
    [Table("Identifiers")]
    public class Identifier
    {
        #region Identification
        [Key, Column("IdentifierKey")]
        public int Key { get; set; }

        [ForeignKey(nameof(IdentifierType))]
        public int IdentifierTypeKey { get; set; }
        #endregion

        #region Navigation
        public List<EmployeeIdentifier> EmployeeIdentifiers { get; set; }

        [ForeignKey(nameof(IdentifierTypeKey))]
        public IdentifierType IdentifierType { get; set; }
        #endregion

        #region Properties
        [Required, StringLength(100, MinimumLength = 1)]
        public string IdentifierValue { get; set; }
        #endregion
    }
}
