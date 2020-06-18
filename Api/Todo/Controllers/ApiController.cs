using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Todo.Models;

namespace Todo.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        [HttpGet(Name = nameof(GetApi))]
        public ActionResult<IEnumerable<LinkDto>> GetApi() => new List<LinkDto>
        {
            new LinkDto(Url.Link(nameof(GetApi), new { }), "self", "GET"),
            new LinkDto(Url.Link("OptionsTodoItems", new { }), "options-todoitems", "OPTIONS"),
            new LinkDto(Url.Link("GetTodoItems", new { }), "head-todoitem", "HEAD"),
            new LinkDto(Url.Link("GetTodoItems", new { }), "get-todoitems", "GET"),
            new LinkDto(Url.Link("PostTodoItemAsync", new { }), "post-todoitem", "POST")
        };
    }
}
