namespace Todo.Models
{
    public class TodoItemCreateDto
    {
        public string Name { get; set; }
        public string Project { get; set; }
        public string Context { get; set; }
        public bool IsComplete { get; set; }
    }
}
