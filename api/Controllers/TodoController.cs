using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Model;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodosAppContext _context;

        public TodoController(TodosAppContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) { 
                return NotFound();
            }

            var usernameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (usernameFromToken != user.Username)
            {
                return Forbid();
            }

            var todos = await _context.Todos.Where(x => x.UserId == userId).ToListAsync();

            return Ok(todos);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodo(Guid id, TodoDto todoDto)
        {
            var user = await _context.Users.FindAsync(todoDto.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var usernameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (usernameFromToken != user.Username)
            {
                return Forbid();
            }


            var todo = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id);
            if (todo == null)
            {
                return NotFound();
            }
            todo.Content = todoDto.Content;
            todo.IsCompleted = todoDto.IsCompleted;
            await _context.SaveChangesAsync();
            return Ok(todo);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Todo>> PostTodo(TodoDto todoDto)
        {
            var user = await _context.Users.FindAsync(todoDto.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var usernameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (usernameFromToken != user.Username)
            {
                return Forbid();
            }

            if (_context.Todos == null)
          {
              return Problem("Entity set 'TodosAppContext.Todos'  is null.");
          }
            var todo = new Todo
            {
                Id = Guid.NewGuid(),
                Content = todoDto.Content,
                IsCompleted = todoDto.IsCompleted,
                UserId = todoDto.UserId,
            };
            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            return Ok(todo.Id);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(Guid id)
        {
            if (_context.Todos == null)
            {
                return NotFound();
            }
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(todo.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var usernameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (usernameFromToken != user.Username)
            {
                return Forbid();
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return Ok(todo);
        }
    }
}
