using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Contexts;
using Todo.Entities;
using Todo.Helpers;
using Todo.Models;
using Todo.Parameters;
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
        private readonly TodoContext _context;

        public TodoItemsController(TodoContext context) => _context = context;

        [HttpOptions]
        public IActionResult GetTodoItemsOptions()
        {
            Response.Headers.Add("Allow", "OPTIONS,HEAD,GET,POST,PUT,DELETE");
            return Ok();
        }

        [HttpHead]
        [HttpGet(Name = "GetTodoItems")]
        public ActionResult<IEnumerable<TodoItemDto>> GetTodoItems([FromQuery] TodoItemParameters parameters)
        {
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

            if (!IsNullOrWhiteSpace(parameters.OrderBy))
                if (parameters.OrderBy.Equals("tags", InvariantCultureIgnoreCase))
                    queryable = queryable.OrderBy(t => t.Project).ThenBy(t => t.Context);

            var pagedList = PagedList<TodoItem>.Create(queryable, parameters.PageNumber, parameters.PageSize);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(new
            {
                totalCount = pagedList.TotalCount,
                totalPages = pagedList.TotalPages,
                currentPage = pagedList.CurrentPage,
                pageSize = pagedList.PageSize,
                previousPageLink = pagedList.HasPrevious ? CreateTodoItemsUri(parameters, PreviousPage) : null,
                nextPageLink = pagedList.HasNext ? CreateTodoItemsUri(parameters, NextPage) : null
            }, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            return pagedList.Select(ItemToDto).ToList();
        }

        [HttpGet("{id}", Name = "GetTodoItem")]
        public async Task<ActionResult<TodoItemDto>> GetTodoItemAsync(Guid id)
        {
            TodoItem todoItem = await _context.TodoItems.FindAsync(id);
            return todoItem == null ? (ActionResult<TodoItemDto>)NotFound() : ItemToDto(todoItem);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItemDto>> PostTodoItemAsync(TodoItemCreateDto dto)
        {
            TodoItem todoItem = DtoToItem(dto);
            _ = _context.TodoItems.Add(todoItem);
            _ = await _context.SaveChangesAsync();

            return CreatedAtRoute("GetTodoItem", new { id = todoItem.Id }, ItemToDto(todoItem));
        }

        [HttpPut("{id}")]
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

        [HttpPatch("{id}")]
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

        [HttpDelete("{id}")]
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
                pageNumber = parameters.PageNumber - 1,
                pageSize = parameters.PageSize
            }),
            NextPage => Url.Link("GetTodoItems", new
            {
                isComplete = parameters.IsComplete,
                searchQuery = parameters.SearchQuery,
                pageNumber = parameters.PageNumber + 1,
                pageSize = parameters.PageSize
            }),
            _ => Url.Link("GetTodoItems", new
            {
                isComplete = parameters.IsComplete,
                searchQuery = parameters.SearchQuery,
                pageNumber = parameters.PageNumber,
                pageSize = parameters.PageSize
            })
        };
    }
}
