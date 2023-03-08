using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace server.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ILogger<TasksController> _logger;
    private readonly IDistributedCache _dc;

    public TasksController(ILogger<TasksController> logger, IDistributedCache distributedCache) {
        _logger = logger;
        _dc = distributedCache;
    }

    [HttpGet]
    public IEnumerable<Task> Get() {
        // Validate data availability
        if (string.IsNullOrEmpty(_dc.GetString("Tasks"))) {
            return Enumerable.Empty<Task>().ToList();
        }

        // Deserialize available data
        List<Task>? tasks = JsonSerializer.Deserialize<List<Task>>(_dc.GetString("Tasks"));
        
        // Return data
        return tasks ?? Enumerable.Empty<Task>().ToList();
    }

    [HttpPost]
    public IActionResult Post([FromBody]Task task) {
        // Model validation
        if (!ModelState.IsValid)
            BadRequest(ModelState);

        // Init task list
        List<Task>? tasks = new List<Task>();

        // Load data from cache
        string? cache = _dc.GetString("Tasks");
        if (!string.IsNullOrEmpty(cache)) {
            // Deserialize available data
            tasks = JsonSerializer.Deserialize<List<Task>>(_dc.GetString("Tasks"));
        }

        // Validate identifier
        if (tasks?.Find(x => x.Identifier == task.Identifier) != null) {
            // Task with same name already exists
            ModelState.AddModelError("errors", "Task with same identifier already exists");
            return BadRequest(ModelState);
        }

        // Validate name
        if (tasks?.Find(x => x.Name == task.Name) != null) {
            // Task with same name already exists
            ModelState.AddModelError("errors", "Task with same name already exists");
            return BadRequest(ModelState);
        }

        // Add new task into the list
        tasks?.Add(task);

        // Save task list into the cache
        _dc.SetString("Tasks", JsonSerializer.Serialize(tasks));

        return Ok();
    }

    [HttpPut]
    [Route("{identifier}")]
    public IActionResult Put(Guid identifier, [FromBody]Task task) {
        // Model validation
        if (!ModelState.IsValid)
            BadRequest(ModelState);

        // Init task list
        List<Task>? tasks = new List<Task>();

        // Load data from cache
        string? cache = _dc.GetString("Tasks");
        if (!string.IsNullOrEmpty(cache)) {
            // Deserialize available data
            tasks = JsonSerializer.Deserialize<List<Task>>(_dc.GetString("Tasks"));
        }

        // Verify task availability
        if (tasks?.Find(x => x.Identifier == identifier) == null) {
            // Task with specific identifier does not exists
            return NotFound();
        }

        // Validate input
        if (tasks?.Find(x => x.Name == task.Name && x.Identifier != identifier) != null) {
            // Task with same name already exists
            ModelState.AddModelError("errors", "Task with same name already exists");
            return BadRequest(ModelState);
        }

        // Update original task
        Task? originalTask = tasks?.Find(x => x.Identifier == identifier);
        if (originalTask != null) {
            // Update original task
            originalTask.Name = task.Name;
            originalTask.Priority = task.Priority;
            originalTask.Status = task.Status;

            // Save task list into the cache
            _dc.SetString("Tasks", JsonSerializer.Serialize(tasks));
        }

        return Ok();
    }

    [HttpDelete]
    [Route("{identifier}")]
    public IActionResult Delete(Guid identifier) {
        // Init task list
        List<Task>? tasks = new List<Task>();

        // Load data from cache
        string? cache = _dc.GetString("Tasks");
        if (!string.IsNullOrEmpty(cache)) {
            // Deserialize available data
            tasks = JsonSerializer.Deserialize<List<Task>>(_dc.GetString("Tasks"));
        }

        // Try to find specific task in collection
        Task? task = tasks?.Find(x => x.Identifier == identifier);

        // Verify task availability
        if (task == null) {
            // Task with specific identifier does not exists
            return NotFound();
        }

        // Validate status
        if (task.Status != Status.Completed) {
            // Unclompeted task could not be deleted
            ModelState.AddModelError("errors", "Unclompeted task could not be deleted");
            return BadRequest(ModelState);
        }

        // Remove task from the list
        tasks?.Remove(task);

        // Save task list into the cache
        _dc.SetString("Tasks", JsonSerializer.Serialize(tasks));

        return Ok();
    }
}
