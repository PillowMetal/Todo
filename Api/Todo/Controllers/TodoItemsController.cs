using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Models;
using static System.Boolean;
using static System.String;

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
        [HttpGet]
        public ActionResult<IEnumerable<TodoItemDto>> GetTodoItems([FromQuery] TodoItemParameters parameters)
        {
            IEnumerable<TodoItemDto> enumerable = _context.TodoItems.Select(t => ItemToDto(t)).AsEnumerable();

            if (!IsNullOrWhiteSpace(parameters?.IsComplete))
            {
                if (!TryParse(parameters.IsComplete.Trim(), out bool flag))
                    return BadRequest();

                enumerable = enumerable.Where(t => t.IsComplete == flag);
            }

            if (!IsNullOrWhiteSpace(parameters?.SearchQuery))
                enumerable = enumerable.Where(t => t.Name.Contains(parameters.SearchQuery.Trim()) || t.Tags.Contains(parameters.SearchQuery.Trim()));

            return enumerable.ToList();
        }

        [HttpGet("{id}", Name = "GetTodoItems")]
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

            return CreatedAtRoute("GetTodoItems", new { id = todoItem.Id }, ItemToDto(todoItem));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TodoItem>> PutTodoItemAsync(Guid id, TodoItemUpdateDto dto)
        {
            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                todoItem = DtoToItem(dto);
                todoItem.Id = id;
                _ = _context.TodoItems.Add(todoItem);
                _ = await _context.SaveChangesAsync();

                return CreatedAtRoute("GetTodoItems", new { id = todoItem.Id }, ItemToDto(todoItem));
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
        public async Task<ActionResult<TodoItem>> PatchTodoItemAsync(Guid id, JsonPatchDocument<TodoItemUpdateDto> document)
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
        public async Task<ActionResult<TodoItem>> DeleteTodoItemAsync(Guid id)
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
            IsComplete = todoItem.IsComplete
        };

        private static TodoItemUpdateDto ItemToUpdateDto(TodoItem todoItem) => new TodoItemUpdateDto
        {
            Name = todoItem.Name,
            Project = todoItem.Project,
            Context = todoItem.Context,
            IsComplete = todoItem.IsComplete
        };

        private static TodoItem DtoToItem(TodoItemManipulationDto dto) => new TodoItem
        {
            Name = dto.Name,
            Project = dto.Project,
            Context = dto.Context,
            IsComplete = dto.IsComplete,
            Secret = "Shhh!"
        };

        public static void DtoToItem(TodoItemUpdateDto dto, TodoItem todoItem)
        {
            todoItem.Name = dto.Name;
            todoItem.Project = dto.Project;
            todoItem.Context = dto.Context;
            todoItem.IsComplete = dto.IsComplete;
        }
    }
}
