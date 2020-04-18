﻿using System.Collections.Generic;
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
        public ActionResult<IEnumerable<TodoItemDto>> GetTodoItems([FromQuery] TodoItemParameters parameters)
        {
            IEnumerable<TodoItemDto> query = _context.TodoItems.Select(t => ItemToDto(t)).AsEnumerable();

            if (!IsNullOrWhiteSpace(parameters?.IsComplete))
            {
                if (!TryParse(parameters.IsComplete.Trim(), out bool flag))
                    return BadRequest();

                query = query.Where(t => t.IsComplete == flag);
            }

            if (!IsNullOrWhiteSpace(parameters?.SearchQuery))
                query = query.Where(t => t.Name.Contains(parameters.SearchQuery.Trim()) || t.Tags.Contains(parameters.SearchQuery.Trim()));

            return query.ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItemDto>> GetTodoItemAsync(long id)
        {
            TodoItem todoItem = await _context.TodoItems.FindAsync(id);
            return todoItem == null ? (ActionResult<TodoItemDto>)NotFound() : ItemToDto(todoItem);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItemDto>> PostTodoItemAsync(TodoItemCreationDto dto)
        {
            var todoItem = new TodoItem
            {
                Name = dto.Name,
                Project = dto.Project,
                Context = dto.Context,
                IsComplete = dto.IsComplete,
                Secret = "Shhh!"
            };

            _ = _context.TodoItems.Add(todoItem);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItems), new { id = todoItem.Id }, ItemToDto(todoItem));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItemAsync(long id, TodoItemCreationDto dto)
        {
            //if (id != dto.Id)
            //    return BadRequest();

            TodoItem todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
                return NotFound();

            todoItem.Name = dto.Name;
            todoItem.Project = dto.Project;
            todoItem.Context = dto.Context;
            todoItem.IsComplete = dto.IsComplete;

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
            Tags = $"{todoItem.Project}|{todoItem.Context}",
            IsComplete = todoItem.IsComplete
        };
    }
}
