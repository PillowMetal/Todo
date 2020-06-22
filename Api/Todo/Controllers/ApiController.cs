using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Todo.Models;

namespace Todo.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        [HttpOptions(Name = nameof(OptionsApi))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult OptionsApi()
        {
            Response.Headers.Add("Allow", "OPTIONS,HEAD,GET");
            return Ok();
        }

        [HttpHead]
        [HttpGet(Name = nameof(GetApi))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<LinkDto>> GetApi() => new List<LinkDto>
        {
            new LinkDto(Url.Link(nameof(GetApi), new { }), "self", "GET"),
            new LinkDto(Url.Link(nameof(OptionsApi), new { }), "options-api", "OPTIONS"),
            new LinkDto(Url.Link(nameof(GetApi), new { }), "head-api", "HEAD"),
            new LinkDto(Url.Link("OptionsTodoItems", new { }), "options-todoitems", "OPTIONS"),
            new LinkDto(Url.Link("GetTodoItems", new { }), "head-todoitem", "HEAD"),
            new LinkDto(Url.Link("GetTodoItems", new { }), "get-todoitems", "GET"),
            new LinkDto(Url.Link("PostTodoItemAsync", new { }), "post-todoitem", "POST")
        };
    }
}
