using System.Collections.Generic;
using System.Threading.Tasks;

using CuyahogaHHS.Bll;

namespace Template.Bll
{
    public interface IEmployeeRepository
    {
        Task<ConcurrencyResult<EmployeeEditor>> Delete(int key, byte[] concurrency);

        Task<EmployeeEditor> FetchEditor(int key);

        Task<IList<IdentifierType>> FetchIdentifierTypes();

        Task<IList<EmployeeItem>> FetchList();

        Task<ConcurrencyResult<EmployeeEditor>> Save(EmployeeEditor data);
    }
}
