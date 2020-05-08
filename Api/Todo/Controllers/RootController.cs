using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Todo.Models;

namespace Todo.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = nameof(GetRoot))]
        public ActionResult<IEnumerable<LinkDto>> GetRoot() => new List<LinkDto>
        {
            new LinkDto(Url.Link(nameof(GetRoot), new { }), "self", "GET"),
            new LinkDto(Url.Link("OptionsTodoItems", new { }), "options-todoitems", "OPTIONS"),
            new LinkDto(Url.Link("GetTodoItems", new { }), "head-todoitem", "HEAD"),
            new LinkDto(Url.Link("GetTodoItems", new { }), "get-todoitems", "GET"),
            new LinkDto(Url.Link("PostTodoItemAsync", new { }), "post-todoitem", "POST")
        };
    }
}
