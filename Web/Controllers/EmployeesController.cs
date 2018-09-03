using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Template.Bll;

namespace Template.Web.Controllers
{
    [Route("[Controller]")]
    public class EmployeesController : Controller
    {
        IEmployeeRepository _Repository;

        [HttpGet]
        public async Task<IActionResult> List()
        {
            return await Task.Run<ViewResult>(() => View());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData.Add("id", id);
            return await Task.Run<ViewResult>(() => View());
        }

        [HttpGet("/api/[Controller]/{id:int}/init")]
        public async Task<IActionResult> EditInit(int id)
        {
            var editor = await _Repository.FetchEditorAsync(id);
            if (editor == null)
                return NotFound();
            return Ok(new { Employee = editor, IdentifierTypes = await _Repository.FetchIdentifierTypesAsync() });
        }
    }
}