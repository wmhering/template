using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Template.Dal.Dto
{
    [Table("IdentifierTypes")]
    public class IdentifierType
    {
        #region Identification
        [Key, Column("IdentifierTypeKey")]
        public int Key { get; set; }
        #endregion

        #region Navigation
        public List<Identifier> Identifiers { get; set; }
        #endregion

        #region Properties
        public bool AllowMultipleEmployees { get; set; }

        public bool AllowMultipleIdentifiers { get; set; }

        [Required, StringLength(100, MinimumLength = 1)]
        public string IdentifierName { get; set; }
        #endregion
    }
}
