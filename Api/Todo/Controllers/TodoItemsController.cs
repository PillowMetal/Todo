using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
using static System.String;
using static System.StringComparison;
using static Todo.Helpers.ResourceUriType;

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        #region Fields

        private readonly TodoContext _context;
        private readonly IPropertyMappingService _service;

        #endregion

        #region Constructors

        public TodoItemsController(TodoContext context, IPropertyMappingService service)
        {
            _context = context;
            _service = service;
        }

        #endregion

        #region Methods

        [HttpOptions(Name = nameof(OptionsTodoItems))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult OptionsTodoItems()
        {
            Response.Headers.Add("Allow", "OPTIONS,HEAD,GET,POST,PUT,PATCH,DELETE");
            return Ok();
        }

        [HttpHead]
        [HttpGet(Name = nameof(GetTodoItems))]
        [Produces("application/json", "application/xml", "application/vnd.usbe.hateoas+json",
            "application/vnd.usbe.todoitem.full+json", "application/vnd.usbe.todoitem.full.hateoas+json",
            "application/vnd.usbe.todoitem.friendly+json", "application/vnd.usbe.todoitem.friendly.hateoas+json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ExpandoObject>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult GetTodoItems([FromQuery] TodoItemParameters parameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue headerValue))
                return BadRequest();

            if (!_service.IsValidMapping<TodoItemDto, TodoItem>(parameters.OrderBy))
                return BadRequest();

            bool isFullRequest = headerValue.SubTypeWithoutSuffix.StartsWith("vnd.usbe.todoitem.full", OrdinalIgnoreCase);

            if (isFullRequest && !_service.HasProperties<TodoItemFullDto>(parameters.Fields) || !isFullRequest && !_service.HasProperties<TodoItemDto>(parameters.Fields))
                return BadRequest();

            IQueryable<TodoItem> queryable = _context.TodoItems.AsQueryable();

            if (!IsNullOrWhiteSpace(parameters.IsComplete))
            {
                if (!TryParse(parameters.IsComplete.Trim(), out bool flag))
                    return BadRequest();

                queryable = queryable.Where(t => t.IsComplete == flag);
            }

            if (!IsNullOrWhiteSpace(parameters.SearchQuery))
                queryable = queryable.Where(t =>
                    t.Name.Contains(parameters.SearchQuery.Trim(), OrdinalIgnoreCase) ||
                    t.Context.Contains(parameters.SearchQuery.Trim(), OrdinalIgnoreCase) ||
                    t.Project.Contains(parameters.SearchQuery.Trim(), OrdinalIgnoreCase));

            queryable = queryable.ApplySort(parameters.OrderBy, _service.GetPropertyMapping<TodoItemDto, TodoItem>());

            var pagedList = PagedList<TodoItem>.Create(queryable, parameters.PageSize, parameters.PageNumber);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(new
            {
                totalCount = pagedList.TotalCount,
                pageSize = pagedList.PageSize,
                totalPages = pagedList.TotalPages,
                currentPage = pagedList.CurrentPage
            }));

            IEnumerable<ExpandoObject> expandoObjects = isFullRequest ?
                pagedList.Select(ItemToFullDto).ShapeData(parameters.Fields).ToList() :
                pagedList.Select(ItemToDto).ShapeData(parameters.Fields).ToList();

            if (headerValue.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase))
            {
                foreach (ExpandoObject expandoObject in expandoObjects)
                    _ = expandoObject.TryAdd("links", CreateLinks((Guid)((IDictionary<string, object>)expandoObject)["id"], parameters.Fields));

                return Ok(new { value = expandoObjects, links = CreateLinks(parameters, pagedList.HasPrevious, pagedList.HasNext) });
            }

            return Ok(expandoObjects);
        }

        [HttpGet("{id}", Name = nameof(GetTodoItemAsync))]
        [Produces("application/json", "application/xml", "application/vnd.usbe.hateoas+json",
            "application/vnd.usbe.todoitem.full+json", "application/vnd.usbe.todoitem.full.hateoas+json",
            "application/vnd.usbe.todoitem.friendly+json", "application/vnd.usbe.todoitem.friendly.hateoas+json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ExpandoObject>> GetTodoItemAsync(Guid id, string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue headerValue))
                return BadRequest();

            bool isFullRequest = headerValue.SubTypeWithoutSuffix.StartsWith("vnd.usbe.todoitem.full", OrdinalIgnoreCase);

            if (isFullRequest && !_service.HasProperties<TodoItemFullDto>(fields) || !isFullRequest && !_service.HasProperties<TodoItemDto>(fields))
                return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
                return NotFound();

            ExpandoObject expandoObject = isFullRequest ? ItemToFullDto(todoItem).ShapeData(fields) : ItemToDto(todoItem).ShapeData(fields);

            if (headerValue.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase))
                _ = expandoObject.TryAdd("links", CreateLinks((Guid)((IDictionary<string, object>)expandoObject)["id"], fields));

            return expandoObject;
        }

        [HttpPost(Name = nameof(PostTodoItemAsync))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ExpandoObject>> PostTodoItemAsync(TodoItemCreateDto dto, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue headerValue))
                return BadRequest();

            TodoItem todoItem = DtoToItem(dto);
            _ = _context.TodoItems.Add(todoItem);
            _ = await _context.SaveChangesAsync();

            ExpandoObject expandoObject = ItemToDto(todoItem).ShapeData();

            if (headerValue.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase))
                _ = expandoObject.TryAdd("links", CreateLinks((Guid)((IDictionary<string, object>)expandoObject)["id"]));

            return CreatedAtRoute(nameof(GetTodoItemAsync), new { id = ((IDictionary<string, object>)expandoObject)["id"] }, expandoObject);
        }

        [HttpPut("{id}", Name = nameof(PutTodoItemAsync))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<TodoItemDto>> PutTodoItemAsync(Guid id, TodoItemUpdateDto dto, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue headerValue))
                return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                todoItem = DtoToItem(dto);
                todoItem.Id = id;
                _ = _context.TodoItems.Add(todoItem);
                _ = await _context.SaveChangesAsync();

                ExpandoObject expandoObject = ItemToDto(todoItem).ShapeData();

                if (headerValue.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase))
                    _ = expandoObject.TryAdd("links", CreateLinks((Guid)((IDictionary<string, object>)expandoObject)["id"]));

                return CreatedAtRoute(nameof(GetTodoItemAsync), new { id = ((IDictionary<string, object>)expandoObject)["id"] }, expandoObject);
            }

            DtoToItem(dto, todoItem);

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("{id}", Name = nameof(PatchTodoItemAsync))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<TodoItemDto>> PatchTodoItemAsync(Guid id, JsonPatchDocument<TodoItemUpdateDto> document, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue headerValue))
                return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(id);
            var dto = new TodoItemUpdateDto();

            if (todoItem == null)
            {
                document.ApplyTo(dto, ModelState);

                if (!TryValidateModel(dto))
                    return ValidationProblem(ModelState);

                todoItem = DtoToItem(dto);
                todoItem.Id = id;
                _ = _context.TodoItems.Add(todoItem);
                _ = await _context.SaveChangesAsync();

                ExpandoObject expandoObject = ItemToDto(todoItem).ShapeData();

                if (headerValue.SubTypeWithoutSuffix.EndsWith("hateoas", OrdinalIgnoreCase))
                    _ = expandoObject.TryAdd("links", CreateLinks((Guid)((IDictionary<string, object>)expandoObject)["id"]));

                return CreatedAtRoute(nameof(GetTodoItemAsync), new { id = ((IDictionary<string, object>)expandoObject)["id"] }, expandoObject);
            }

            dto = ItemToUpdateDto(todoItem);
            document.ApplyTo(dto, ModelState);

            if (!TryValidateModel(dto))
                return ValidationProblem(ModelState);

            DtoToItem(dto, todoItem);

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}", Name = nameof(DeleteTodoItemAsync))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DeleteTodoItemAsync(Guid id)
        {
            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
                return NotFound();

            _ = _context.TodoItems.Remove(todoItem);
            _ = await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoItemExists(Guid id) => _context.TodoItems.Any(t => t.Id == id);

        private static TodoItemDto ItemToDto(TodoItem todoItem) => new TodoItemDto
        {
            Id = todoItem.Id,
            Name = todoItem.Name,
            Tags = $"{todoItem.Project}|{todoItem.Context}",
            Age = (Today - todoItem.Date).Days,
            IsComplete = todoItem.IsComplete
        };

        private static TodoItemFullDto ItemToFullDto(TodoItem todoItem) => new TodoItemFullDto
        {
            Id = todoItem.Id,
            Name = todoItem.Name,
            Project = todoItem.Project,
            Context = todoItem.Context,
            Date = todoItem.Date,
            IsComplete = todoItem.IsComplete
        };

        private static TodoItemUpdateDto ItemToUpdateDto(TodoItem todoItem) => new TodoItemUpdateDto
        {
            Name = todoItem.Name,
            Project = todoItem.Project,
            Context = todoItem.Context,
            Date = todoItem.Date,
            IsComplete = todoItem.IsComplete
        };

        private static TodoItem DtoToItem(TodoItemManipulationDto dto) => new TodoItem
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

        private string CreateTodoItemsUri(TodoItemParameters parameters, ResourceUriType type) => type switch
        {
            PreviousPage => Url.Link(nameof(GetTodoItems), new
            {
                isComplete = parameters.IsComplete,
                searchQuery = parameters.SearchQuery,
                orderBy = parameters.OrderBy,
                pageSize = parameters.PageSize,
                pageNumber = parameters.PageNumber - 1,
                fields = parameters.Fields
            }),
            NextPage => Url.Link(nameof(GetTodoItems), new
            {
                isComplete = parameters.IsComplete,
                searchQuery = parameters.SearchQuery,
                orderBy = parameters.OrderBy,
                pageSize = parameters.PageSize,
                pageNumber = parameters.PageNumber + 1,
                fields = parameters.Fields
            }),
            _ => Url.Link(nameof(GetTodoItems), new
            {
                isComplete = parameters.IsComplete,
                searchQuery = parameters.SearchQuery,
                orderBy = parameters.OrderBy,
                pageSize = parameters.PageSize,
                pageNumber = parameters.PageNumber,
                fields = parameters.Fields
            })
        };

        private IEnumerable<LinkDto> CreateLinks(Guid id, string fields = null) => new List<LinkDto>
        {
            IsNullOrWhiteSpace(fields) ?
                new LinkDto(Url.Link(nameof(GetTodoItemAsync), new { id }), "self", "GET") :
                new LinkDto(Url.Link(nameof(GetTodoItemAsync), new { id, fields }), ")self", "GET"),
            new LinkDto(Url.Link(nameof(PutTodoItemAsync), new { id }), "put-todoitem", "PUT"),
            new LinkDto(Url.Link(nameof(PatchTodoItemAsync), new { id }), "patch-todoitem", "PATCH"),
            new LinkDto(Url.Link(nameof(DeleteTodoItemAsync), new { id }), "delete-todoitem", "DELETE")
        };

        private IEnumerable<LinkDto> CreateLinks(TodoItemParameters parameters, bool hasPrevious, bool hasNext)
        {
            var linkDtos = new List<LinkDto> { new LinkDto(CreateTodoItemsUri(parameters, Current), "self", "GET") };

            if (hasPrevious)
                linkDtos.Add(new LinkDto(CreateTodoItemsUri(parameters, PreviousPage), "previous-page", "GET"));

            if (hasNext)
                linkDtos.Add(new LinkDto(CreateTodoItemsUri(parameters, NextPage), "next-page", "GET"));

            return linkDtos;
        }
    }

    #endregion
}
