using System.ComponentModel.DataAnnotations;

namespace server;

public enum Status {
    NotStarted, InProgress, Completed
}

public class Task {
    public Guid? Identifier { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Task name is required")]
    public string? Name { get; set; }

    [Range(0,100, ErrorMessage = "Priority should be set in range between 0 and 100")]
    public Int16 Priority { get; set; }

    [Range(0,2, ErrorMessage = "Status is required")]
    public Status Status { get; set; }
}
