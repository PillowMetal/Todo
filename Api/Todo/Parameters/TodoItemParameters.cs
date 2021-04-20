namespace Todo.Parameters
{
    public class TodoItemParameters
    {
        private const int MaximumPageSize = 10;
        private readonly int _pageSize = 4;

        public string IsComplete { get; init; }
        public string SearchQuery { get; init; }
        public string OrderBy { get; init; } = "tags";
        public int PageSize { get => _pageSize; init => _pageSize = value > MaximumPageSize ? MaximumPageSize : value; }
        public int PageNumber { get; init; } = 1;
        public string Fields { get; init; }
    }
}
