using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Encodings.Web;
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
using static Todo.Helpers.ResourceUriType;

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly IPropertyMappingService _service;

        public TodoItemsController(TodoContext context, IPropertyMappingService service)
        {
            _context = context;
            _service = service;
        }

        [HttpOptions(Name = "OptionsTodoItems")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IActionResult OptionsTodoItems()
        {
            Response.Headers.Add("Allow", "OPTIONS,HEAD,GET,POST,PUT,PATCH,DELETE");
            return Ok();
        }

        [HttpHead]
        [HttpGet(Name = "GetTodoItems")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult GetTodoItems([FromQuery] TodoItemParameters parameters)
        {
            if (!_service.IsValidMapping<TodoItemDto, TodoItem>(parameters.OrderBy))
                return BadRequest();

            if (!_service.HasProperties<TodoItemDto>(parameters.Fields))
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
                    t.Name.Contains(parameters.SearchQuery.Trim()) ||
                    t.Context.Contains(parameters.SearchQuery.Trim()) ||
                    t.Project.Contains(parameters.SearchQuery.Trim()));

            queryable = queryable.ApplySort(parameters.OrderBy, _service.GetPropertyMapping<TodoItemDto, TodoItem>());

            var pagedList = PagedList<TodoItem>.Create(queryable, parameters.PageSize, parameters.PageNumber);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(new
            {
                totalCount = pagedList.TotalCount,
                pageSize = pagedList.PageSize,
                totalPages = pagedList.TotalPages,
                currentPage = pagedList.CurrentPage
            }, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            IEnumerable<ExpandoObject> expandoObjects = pagedList.Select(ItemToDto).ShapeData(parameters.Fields).Select(expandoObject =>
            {
                _ = expandoObject.TryAdd("links", CreateLinks((Guid)((IDictionary<string, object>)expandoObject)["Id"]));
                return expandoObject;
            });

            return Ok(new
            {
                value = expandoObjects,
                links = CreateLinks(parameters, pagedList.HasPrevious, pagedList.HasNext)
            });
        }

        [HttpGet("{id}", Name = "GetTodoItem")]
        //[Produces("application/json", "application/xml", "application/vnd.usbe.hateoas+json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ExpandoObject>> GetTodoItemAsync(Guid id, string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue headerValue))
                return BadRequest();

            if (!_service.HasProperties<TodoItemDto>(fields))
                return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
                return NotFound();

            ExpandoObject expandoObject = ItemToDto(todoItem).ShapeData(fields);

            if (headerValue.MediaType == "application/vnd.usbe.hateoas+json")
                _ = expandoObject.TryAdd("links", CreateLinks((Guid)((IDictionary<string, object>)expandoObject)["Id"], fields));

            return expandoObject;
        }

        [HttpPost(Name = "PostTodoItem")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ExpandoObject>> PostTodoItemAsync(TodoItemCreateDto dto)
        {
            TodoItem todoItem = DtoToItem(dto);
            _ = _context.TodoItems.Add(todoItem);
            _ = await _context.SaveChangesAsync();

            ExpandoObject expandoObject = ItemToDto(todoItem).ShapeData();
            _ = expandoObject.TryAdd("links", CreateLinks((Guid)((IDictionary<string, object>)expandoObject)["Id"]));

            return CreatedAtRoute("GetTodoItem", new { id = ((IDictionary<string, object>)expandoObject)["Id"] }, expandoObject);
        }

        [HttpPut("{id}", Name = "PutTodoItem")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<TodoItemDto>> PutTodoItemAsync(Guid id, TodoItemUpdateDto dto)
        {
            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                todoItem = DtoToItem(dto);
                todoItem.Id = id;
                _ = _context.TodoItems.Add(todoItem);
                _ = await _context.SaveChangesAsync();

                return CreatedAtRoute("GetTodoItem", new { id = todoItem.Id }, ItemToDto(todoItem));
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

        [HttpPatch("{id}", Name = "PatchTodoItem")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<TodoItemDto>> PatchTodoItemAsync(Guid id, JsonPatchDocument<TodoItemUpdateDto> document)
        {
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

                return CreatedAtRoute("GetTodoItems", new { id = todoItem.Id }, ItemToDto(todoItem));
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

        [HttpDelete("{id}", Name = "DeleteTodoItem")]
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
            PreviousPage => Url.Link("GetTodoItems", new
            {
                isComplete = parameters.IsComplete,
                searchQuery = parameters.SearchQuery,
                orderBy = parameters.OrderBy,
                pageSize = parameters.PageSize,
                pageNumber = parameters.PageNumber - 1,
                fields = parameters.Fields
            }),
            NextPage => Url.Link("GetTodoItems", new
            {
                isComplete = parameters.IsComplete,
                searchQuery = parameters.SearchQuery,
                orderBy = parameters.OrderBy,
                pageSize = parameters.PageSize,
                pageNumber = parameters.PageNumber + 1,
                fields = parameters.Fields
            }),
            _ => Url.Link("GetTodoItems", new
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
                new LinkDto(Url.Link("GetTodoItem", new { id }), "self", "GET") :
                new LinkDto(Url.Link("GetTodoItem", new { id, fields }), "self", "GET"),
            new LinkDto(Url.Link("PutTodoItem", new { id }), "put-todoitem", "PUT"),
            new LinkDto(Url.Link("PatchTodoItem", new { id }), "patch-todoitem", "PATCH"),
            new LinkDto(Url.Link("DeleteTodoItem", new { id }), "delete-todoitem", "DELETE")
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
}
