using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo_MVC_API.Repository;
using TodoApi.Model;

namespace Todo_MVC_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;
        private readonly TodoDbContext _todoDbContext;

        public TodoController(IDbContextFactory<TodoDbContext> dbContextFactory, ILogger<TodoController> logger)
        {
            _logger = logger;
            _todoDbContext = dbContextFactory.CreateDbContext();
        }

        [HttpGet()]
        [Route("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            return Ok("Welcome to TODO API!");
        }

        /// <summary>
        /// Add To do 
        /// http://localhost/todo/
        /// </summary>
        /// <param name="todo">To do item to add</param>
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddTodoAsync([FromBody] Todo todo)
        {
            return await Try(async () =>
            {
                _todoDbContext.Todos.Add(todo);
                await _todoDbContext.SaveChangesAsync();

                return Created($"/todoitems/{todo.Id}", todo);
            });
        }

        /// <summary>
        /// Delete To do
        /// http://localhost/todo/123
        /// </summary>
        /// <param name="id">To do Id to delete</param>
        [HttpDelete()]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return await Try(async () =>
            {
                if (await _todoDbContext.Todos.FindAsync(id) is Todo todo)
                {
                    _todoDbContext.Todos.Remove(todo);
                    await _todoDbContext.SaveChangesAsync();
                    return NoContent();
                }

                return NotFound();
            });
        }

        /// <summary>
        /// List all to do
        /// http://localhost/todo/all
        /// </summary>
        /// <returns>List of to do Items</returns>
        [HttpGet()]
        [Route("all")]
        [ProducesResponseType(typeof(List<Todo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAllAsync()
        {
            return await Try(async () => { 
                return Ok(await _todoDbContext.Todos.ToListAsync()); 
            });
        }

        /// <summary>
        /// Mark To do complete
        /// http://localhost/todo/123/completed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="todoDb"></param>
        [HttpPatch()]
        [Route("{id}/completed")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkCompleteAsync(int id)
        {
            return await Try(async () =>
            {
                Todo? todo = await _todoDbContext.Todos.FindAsync(id);
                if (todo is null) return NotFound();

                todo.IsComplete = true;
                await _todoDbContext.SaveChangesAsync();

                return NoContent();
            });
        }

        /// <summary>
        /// Error handling helper
        /// </summary>
        /// <param name="actionAsync">async action delegate</param>
        /// <returns></returns>
        private async Task<IActionResult> Try(Func<Task<IActionResult>> actionAsync)
        {
            try
            {
                return await actionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500);
            }
        }

    }
}
