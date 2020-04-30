namespace Todo.Parameters
{
    public class TodoItemParameters
    {
        private const int MaximumPageSize = 10;

        public string IsComplete { get; set; }
        public string SearchQuery { get; set; }
        public string OrderBy { get; set; } = "Tags";
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 4;
        public int PageSize { get => _pageSize; set => _pageSize = value > MaximumPageSize ? MaximumPageSize : value; }
    }
}
