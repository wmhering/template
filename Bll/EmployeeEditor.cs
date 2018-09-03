using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Template.Bll
{
    /// <summary>
    /// Data used to edit an employee.</summary>
    /// <remarks>
    /// Employee data is inserted, updated and deleted based on changes to the HR system and should not be changed in this system since it will
    /// revert back to the values from HR on the next update cycle but the related data (Identifiers) can be updated.</remarks>
    public class EmployeeEditor
    {
        /// <summary>
        /// Gets the employee's unique identifier.</summary>
        public int Key { get; set; }

        /// <summary>
        /// Gets a value that changes each time data fro HR is loaded or a use edits the related data used to detect conflicts.</summary>
        public byte[] Concurrency { get; set; }

        /// <summary>
        /// Gets the employee's full name last name first.</summary>
        public string Name { get; set; }

       /// <summary>
       /// Gets a list of the employee's identifiers for various other systems.</summary>
        public List<Identifier> Identifiers { get; set; }

        /// <summary>
        /// Data about an employee's identifier into another system.</summary>
        public class Identifier
        {
            /// <summary>
            /// Gets the unique identifier of the identifier.</summary>
            public int Key { get; set; }

            /// <summary>
            /// Gets/sets the identifier's value.</summary>
            /// <remarks>This is the employee's network id, email address, phone number, etc...</remarks>
            [Display(Name = "Identifier Value")]
            [Required]
            [StringLength(100, MinimumLength = 1)]
            public string Value { get; set; }

            /// <summary>
            /// Gets/sets a value that identifies the type of this identifier.</summary>
            /// <remarks>This valu identifies the identifier as a network id, email address, phone number, etc...</remarks>
            [Display(Name = "Identifier Type")]
            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Identifier type must be a valid value.")]
            public int IdentifierTypeKey { get; set; }
        }
    }
}
