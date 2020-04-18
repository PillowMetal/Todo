using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [HttpGet]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodoItemsAsync(string isComplete) => IsNullOrWhiteSpace(isComplete)
            ? await _context.TodoItems.Select(t => ItemToDto(t)).ToListAsync()
            : !TryParse(isComplete.Trim(), out bool flag)
                ? (ActionResult<IEnumerable<TodoItemDto>>)BadRequest()
                : await _context.TodoItems.Where(t => t.IsComplete == flag).Select(t => ItemToDto(t)).ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItemDto>> GetTodoItemAsync(long id)
        {
            TodoItem todoItem = await _context.TodoItems.FindAsync(id);
            return todoItem == null ? (ActionResult<TodoItemDto>)NotFound() : ItemToDto(todoItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItemAsync(long id, TodoItemDto todoItemDto)
        {
            if (id != todoItemDto.Id)
                return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
                return NotFound();

            todoItem.Name = todoItemDto.Name;
            todoItem.IsComplete = todoItemDto.IsComplete;

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

        [HttpPost]
        public async Task<ActionResult<TodoItemDto>> PostTodoItemAsync(TodoItem todoItemDto)
        {
            var todoItem = new TodoItem
            {
                IsComplete = todoItemDto.IsComplete,
                Name = todoItemDto.Name
            };

            _ = _context.TodoItems.Add(todoItem);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, ItemToDto(todoItem));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItemAsync(long id)
        {
            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
                return NotFound();

            _ = _context.TodoItems.Remove(todoItem);
            _ = await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoItemExists(long id) => _context.TodoItems.Any(e => e.Id == id);

        private static TodoItemDto ItemToDto(TodoItem todoItem) => new TodoItemDto
        {
            Id = todoItem.Id,
            Name = todoItem.Name,
            IsComplete = todoItem.IsComplete
        };
    }
}
