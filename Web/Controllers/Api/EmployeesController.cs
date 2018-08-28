using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Template.Bll;

namespace Template.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _Repository;
        private readonly ILogger<EmployeesController> _Logger;

        public EmployeesController(IEmployeeRepository repository, ILogger<EmployeesController> logger)
        {
            _Repository = repository;
            _Logger = logger;
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id, byte[] concurrency)
        {
            var result = await _Repository.Delete(id, concurrency);
            if (result.ConcurrencyError)
                return Conflict(result.Data);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var result = await _Repository.FetchList();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var result = await _Repository.FetchEditor(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("{id:int}")]
        public async Task<ActionResult> Post([FromBody]EmployeeEditor data, int id = 0)
        {
            if (!ModelState.IsValid)
                return this.BadRequest(ModelState);
            var result = await _Repository.Save(data);
            if (result.ConcurrencyError)
                return Conflict(result.Data);
            return Ok(result.Data);
        }
    }
}