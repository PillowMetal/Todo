namespace Todo.Models
{
    public class TodoItemCreationDto
    {
        public string Name { get; set; }
        public string Project { get; set; }
        public string Context { get; set; }
        public bool IsComplete { get; set; }
    }
}
