using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Todo.Contexts;
using Todo.Entities;
using Todo.Helpers;
using Todo.Models;
using Todo.Parameters;
using Todo.Services;
using static System.Boolean;
using static System.DateTime;
using static System.Net.Mime.MediaTypeNames.Application;
using static System.String;
using static System.StringComparison;
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.Net.Http.Headers.HeaderNames;
using static Todo.Helpers.ResourceUriType;

namespace Todo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(Json, Xml, "application/vnd.usbe.hateoas+json",
        "application/vnd.usbe.todoitem.full+json", "application/vnd.usbe.todoitem.full.hateoas+json",
        "application/vnd.usbe.todoitem.friendly+json", "application/vnd.usbe.todoitem.friendly.hateoas+json")]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoDbContext _context;
        private readonly IPropertyMappingService _service;

        public TodoItemsController(TodoDbContext context, IPropertyMappingService service)
        {
            _context = context;
            _service = service;
        }

        [HttpOptions(Name = nameof(OptionsTodoItems))]
        [ProducesResponseType(Status200OK)]
        public IActionResult OptionsTodoItems()
        {
            Response.Headers.Add(Allow, $"{Options},{Head},{Get},{Post},{Put},{Patch},{Delete}");
            return Ok();
        }

        [HttpHead]
        [HttpGet(Name = nameof(GetTodoItems))]
        [ProducesResponseType(Status200OK, Type = typeof(IEnumerable<TodoItemDto>))]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<ExpandoObject>> GetTodoItems([FromQuery] TodoItemParameters parameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParseList((mediaType ?? "*/*").Split(','), out IList<MediaTypeHeaderValue> headerValues))
                return BadRequest();

            if (!_service.IsValidMapping<TodoItemDto, TodoItem>(parameters.OrderBy))
                return BadRequest();

            bool isFullRequest = headerValues.Any(static value => value.SubTypeWithoutSuffix.StartsWith("vnd.usbe.todoitem.full", OrdinalIgnoreCase));

            if (isFullRequest && !_service.HasProperties<TodoItemFullDto>(parameters.Fields) || !isFullRequest && !_service.HasProperties<TodoItemDto>(parameters.Fields))
                return BadRequest();

            IQueryable<TodoItem> queryable = _context.TodoItems;

            if (!IsNullOrWhiteSpace(parameters.IsComplete))
            {
                if (!TryParse(parameters.IsComplete.Trim(), out bool isComplete))
                    return BadRequest();

                queryable = queryable.Where(t => t.IsComplete == isComplete);
            }

            if (!IsNullOrWhiteSpace(parameters.SearchQuery))
                queryable = queryable.AsEnumerable().Where(t =>
                    (t.Name ?? Empty).Contains(parameters.SearchQuery.Trim(), OrdinalIgnoreCase) ||
                    (t.Context ?? Empty).Contains(parameters.SearchQuery.Trim(), OrdinalIgnoreCase) ||
                    (t.Project ?? Empty).Contains(parameters.SearchQuery.Trim(), OrdinalIgnoreCase)).AsQueryable();

            queryable = queryable.ApplySort(parameters.OrderBy, _service.GetPropertyMapping<TodoItemDto, TodoItem>());

            PagedList<TodoItem> pagedList = new(queryable, parameters.PageSize, parameters.PageNumber);

            IEnumerable<ExpandoObject> expandoObjects = isFullRequest ?
                pagedList.Select(ItemToFullDto).ShapeData(parameters.Fields, "id") :
                pagedList.Select(ItemToDto).ShapeData(parameters.Fields, "id");

            pagedList.CreatePaginationHeader(Response);

            if (headerValues.Any(static value => value.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase)))
            {
                foreach (ExpandoObject expandoObject in expandoObjects)
                    _ = expandoObject.TryAdd("links", CreateTodoItemLinks((Guid)((IDictionary<string, object>)expandoObject)["id"], parameters.Fields));

                return Ok(new
                {
                    values = expandoObjects,
                    links = CreateTodoItemsLinks(parameters, pagedList.HasPrevious, pagedList.HasNext)
                });
            }

            return Ok(expandoObjects);
        }

        [HttpGet("{id:guid}", Name = nameof(GetTodoItemAsync))]
        [ProducesResponseType(Status200OK, Type = typeof(TodoItemDto))]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ActionResult<ExpandoObject>> GetTodoItemAsync(Guid id, string fields, [FromHeader(Name = "Accept")] string mediaType, CancellationToken token)
        {
            if (!MediaTypeHeaderValue.TryParseList((mediaType ?? "*/*").Split(','), out IList<MediaTypeHeaderValue> headerValues))
                return BadRequest();

            bool isFullRequest = headerValues.Any(static value => value.SubTypeWithoutSuffix.StartsWith("vnd.usbe.todoitem.full", OrdinalIgnoreCase));

            if (isFullRequest && !_service.HasProperties<TodoItemFullDto>(fields) || !isFullRequest && !_service.HasProperties<TodoItemDto>(fields))
                return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(new object[] { id }, token);

            if (todoItem == null)
                return NotFound();

            ExpandoObject expandoObject = isFullRequest ?
                ItemToFullDto(todoItem).ShapeData(fields, "id") :
                ItemToDto(todoItem).ShapeData(fields, "id");

            if (headerValues.Any(static value => value.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase)))
                _ = expandoObject.TryAdd("links", CreateTodoItemLinks((Guid)((IDictionary<string, object>)expandoObject)["id"], fields));

            return expandoObject;
        }

        [HttpPost(Name = nameof(PostTodoItemAsync))]
        [ProducesResponseType(Status201Created, Type = typeof(TodoItemDto))]
        [ProducesResponseType(Status400BadRequest)]
        [Consumes(Json, Xml)]
        public async Task<ActionResult<ExpandoObject>> PostTodoItemAsync(TodoItemCreateDto dto, [FromHeader(Name = "Accept")] string mediaType, CancellationToken token)
        {
            if (!MediaTypeHeaderValue.TryParseList((mediaType ?? "*/*").Split(','), out IList<MediaTypeHeaderValue> headerValues))
                return BadRequest();

            TodoItem todoItem = DtoToItem(dto);
            _ = _context.TodoItems.Add(todoItem);
            _ = await _context.SaveChangesAsync(token);

            ExpandoObject expandoObject = headerValues.Any(static value => value.SubTypeWithoutSuffix.StartsWith("vnd.usbe.todoitem.full", OrdinalIgnoreCase)) ?
                ItemToFullDto(todoItem).ShapeData() :
                ItemToDto(todoItem).ShapeData();

            if (headerValues.Any(static value => value.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase)))
                _ = expandoObject.TryAdd("links", CreateTodoItemLinks((Guid)((IDictionary<string, object>)expandoObject)["id"]));

            return CreatedAtRoute(nameof(GetTodoItemAsync), new { id = ((IDictionary<string, object>)expandoObject)["id"] }, expandoObject);
        }

        [HttpPut("{id:guid}", Name = nameof(PutTodoItemAsync))]
        [ProducesResponseType(Status201Created, Type = typeof(TodoItemDto))]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        [Consumes(Json, Xml)]
        public async Task<ActionResult<ExpandoObject>> PutTodoItemAsync(Guid id, TodoItemUpdateDto dto, [FromHeader(Name = "Accept")] string mediaType, CancellationToken token)
        {
            if (!MediaTypeHeaderValue.TryParseList((mediaType ?? "*/*").Split(','), out IList<MediaTypeHeaderValue> headerValues))
                return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(new object[] { id }, token);

            if (todoItem == null)
            {
                todoItem = DtoToItem(dto);
                todoItem.Id = id;
                _ = _context.TodoItems.Add(todoItem);
                _ = await _context.SaveChangesAsync(token);

                ExpandoObject expandoObject = headerValues.Any(static value => value.SubTypeWithoutSuffix.StartsWith("vnd.usbe.todoitem.full", OrdinalIgnoreCase)) ?
                    ItemToFullDto(todoItem).ShapeData() :
                    ItemToDto(todoItem).ShapeData();

                if (headerValues.Any(static value => value.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase)))
                    _ = expandoObject.TryAdd("links", CreateTodoItemLinks((Guid)((IDictionary<string, object>)expandoObject)["id"]));

                return CreatedAtRoute(nameof(GetTodoItemAsync), new { id = ((IDictionary<string, object>)expandoObject)["id"] }, expandoObject);
            }

            DtoToItem(dto, todoItem);

            try
            {
                _ = await _context.SaveChangesAsync(token);
            }
            catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("{id:guid}", Name = nameof(PatchTodoItemAsync))]
        [ProducesResponseType(Status201Created, Type = typeof(TodoItemDto))]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        [Consumes("application/json-patch+json", "")]
        public async Task<ActionResult<ExpandoObject>> PatchTodoItemAsync(Guid id, JsonPatchDocument<TodoItemUpdateDto> document, [FromHeader(Name = "Accept")] string mediaType, CancellationToken token)
        {
            if (!MediaTypeHeaderValue.TryParseList((mediaType ?? "*/*").Split(','), out IList<MediaTypeHeaderValue> headerValues))
                return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(new object[] { id }, token);
            TodoItemUpdateDto dto = new();

            if (todoItem == null)
            {
                document.ApplyTo(dto, ModelState);

                if (!TryValidateModel(dto))
                    return ValidationProblem(ModelState);

                todoItem = DtoToItem(dto);
                todoItem.Id = id;
                _ = _context.TodoItems.Add(todoItem);
                _ = await _context.SaveChangesAsync(token);

                ExpandoObject expandoObject = headerValues.Any(static value => value.SubTypeWithoutSuffix.StartsWith("vnd.usbe.todoitem.full", OrdinalIgnoreCase)) ?
                    ItemToFullDto(todoItem).ShapeData() :
                    ItemToDto(todoItem).ShapeData();

                if (headerValues.Any(static value => value.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase)))
                    _ = expandoObject.TryAdd("links", CreateTodoItemLinks((Guid)((IDictionary<string, object>)expandoObject)["id"]));

                return CreatedAtRoute(nameof(GetTodoItemAsync), new { id = ((IDictionary<string, object>)expandoObject)["id"] }, expandoObject);
            }

            dto = ItemToUpdateDto(todoItem);
            document.ApplyTo(dto, ModelState);

            if (!TryValidateModel(dto))
                return ValidationProblem(ModelState);

            DtoToItem(dto, todoItem);

            try
            {
                _ = await _context.SaveChangesAsync(token);
            }
            catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = nameof(DeleteTodoItemAsync))]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> DeleteTodoItemAsync(Guid id, CancellationToken token)
        {
            TodoItem todoItem = await _context.TodoItems.FindAsync(new object[] { id }, token);

            if (todoItem == null)
                return NotFound();

            _ = _context.TodoItems.Remove(todoItem);
            _ = await _context.SaveChangesAsync(token);

            return NoContent();
        }

        private bool TodoItemExists(Guid id) => _context.TodoItems.Any(t => t.Id == id);

        private static TodoItemDto ItemToDto(TodoItem todoItem) => new()
        {
            Id = todoItem.Id,
            Name = todoItem.Name,
            Tags = $"{todoItem.Project}|{todoItem.Context}",
            Age = (Today - todoItem.Date).Days,
            IsComplete = todoItem.IsComplete
        };

        private static TodoItemFullDto ItemToFullDto(TodoItem todoItem) => new()
        {
            Id = todoItem.Id,
            Name = todoItem.Name,
            Project = todoItem.Project,
            Context = todoItem.Context,
            Date = todoItem.Date,
            IsComplete = todoItem.IsComplete
        };

        private static TodoItemUpdateDto ItemToUpdateDto(TodoItem todoItem) => new()
        {
            Name = todoItem.Name,
            Project = todoItem.Project,
            Context = todoItem.Context,
            Date = todoItem.Date,
            IsComplete = todoItem.IsComplete
        };

        private static TodoItem DtoToItem(TodoItemManipulationDto dto) => new()
        {
            Name = dto.Name,
            Project = dto.Project,
            Context = dto.Context,
            Date = dto.Date,
            IsComplete = dto.IsComplete,
            Secret = "Shhh!"
        };

        private static void DtoToItem(TodoItemManipulationDto dto, TodoItem todoItem)
        {
            todoItem.Name = dto.Name;
            todoItem.Project = dto.Project;
            todoItem.Context = dto.Context;
            todoItem.Date = dto.Date;
            todoItem.IsComplete = dto.IsComplete;
        }

        private IEnumerable<LinkDto> CreateTodoItemLinks(Guid id, string fields = null) => new List<LinkDto>
        {
            new(Url.Link(nameof(GetTodoItemAsync), new { id, fields }), "self", Get),
            new(Url.Link(nameof(PutTodoItemAsync), new { id }), "put-todoitem", Put),
            new(Url.Link(nameof(PatchTodoItemAsync), new { id }), "patch-todoitem", Patch),
            new(Url.Link(nameof(DeleteTodoItemAsync), new { id }), "delete-todoitem", Delete)
        };

        private IEnumerable<LinkDto> CreateTodoItemsLinks(TodoItemParameters parameters, bool hasPrevious, bool hasNext)
        {
            List<LinkDto> linkDtos = new() { new LinkDto(CreateTodoItemsUri(Current), "self", Get) };

            if (hasPrevious)
                linkDtos.Add(new LinkDto(CreateTodoItemsUri(PreviousPage), "previous-page", Get));

            if (hasNext)
                linkDtos.Add(new LinkDto(CreateTodoItemsUri(NextPage), "next-page", Get));

            return linkDtos;

            string CreateTodoItemsUri(ResourceUriType type)
            {
                dynamic expandoObject = new ExpandoObject();

                expandoObject.isComplete = parameters.IsComplete;
                expandoObject.searchQuery = parameters.SearchQuery;
                expandoObject.orderBy = parameters.OrderBy;
                expandoObject.pageSize = parameters.PageSize;

                expandoObject.pageNumber = parameters.PageNumber + type switch
                {
                    PreviousPage => -1,
                    NextPage => 1,
                    _ => 0
                };

                expandoObject.fields = parameters.Fields;

                return Url.Link(nameof(GetTodoItems), expandoObject);
            }
        }
    }
}
