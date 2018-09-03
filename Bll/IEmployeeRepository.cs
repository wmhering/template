using System.Collections.Generic;
using System.Threading.Tasks;

using CuyahogaHHS.Bll;

namespace Template.Bll
{
    /// <summary>
    /// Defines methods needed to interact with the data store when working with employees.</summary>
    public interface IEmployeeRepository
    {
        /// <summary>
        /// Fetch employee data for editing.</summary>
        /// <param name="key">
        /// The unique identifier of the employee to be edited.</param>
        /// <returns>
        /// An <see cref="EmployeeEditor"/> or null if the employee with the given identifier could not be found. </returns>
        Task<EmployeeEditor> FetchEditorAsync(int key);

        /// <summary>
        /// Fetch a list of types of employee identifiers.</summary>
        /// <returns>
        /// An IList of <see cref="IdentifierType"/>s.</returns>
        Task<IList<IdentifierType>> FetchIdentifierTypesAsync();

        /// <summary>
        /// Fetch a list of employees for displaying and selecting.</summary>
        /// <returns>
        /// An IList of <see cref="EmployeeItem"/>s.</returns>
        Task<IList<EmployeeItem>> FetchListAsync();

        /// <summary>
        /// Saves changes to an employee.</summary>
        /// <param name="data">
        /// The <see cref="EmployeeEditor"/> object containing the modified data.</param>
        /// <returns>
        /// An <see cref="ConcurrencyResult{T}"/> of <see cref="EmployeeEditor"/> indicating if there was a conflict with another user's changes or
        /// not. If there wasn't a conflict then the result also contains the saved data with any updates from the data store. If there was a
        /// conflict then the result also contains the data the other user saved.</returns>
        Task<ConcurrencyResult<EmployeeEditor>> SaveAsync(EmployeeEditor data);
    }
}
