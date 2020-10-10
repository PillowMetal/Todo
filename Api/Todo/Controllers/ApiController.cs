using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Todo.Models;
using static System.Net.Mime.MediaTypeNames.Application;
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.Net.Http.Headers.HeaderNames;

namespace Todo.Controllers
{
    [ApiController]
    [Route("api")]
    [Produces(Json)]
    public class ApiController : ControllerBase
    {
        [HttpOptions(Name = nameof(OptionsApi))]
        [ProducesResponseType(Status200OK)]
        public IActionResult OptionsApi()
        {
            Response.Headers.Add(Allow, $"{Options},{Head},{Get}");
            return Ok();
        }

        [HttpHead]
        [HttpGet(Name = nameof(GetApi))]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<LinkDto>> GetApi() => new List<LinkDto>
        {
            new LinkDto(Url.Link(nameof(GetApi), new { }), "self", Get),
            new LinkDto(Url.Link(nameof(OptionsApi), new { }), "options-api", Options),
            new LinkDto(Url.Link(nameof(GetApi), new { }), "head-api", Head),
            new LinkDto(Url.Link("OptionsTodoItems", new { }), "options-todoitems", Options),
            new LinkDto(Url.Link("GetTodoItems", new { }), "head-todoitem", Head),
            new LinkDto(Url.Link("GetTodoItems", new { }), "get-todoitems", Get),
            new LinkDto(Url.Link("PostTodoItemAsync", new { }), "post-todoitem", Post)
        };
    }
}
