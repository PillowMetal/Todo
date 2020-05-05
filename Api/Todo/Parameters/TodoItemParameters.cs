namespace Todo.Parameters
{
    public class TodoItemParameters
    {
        private const int MaximumPageSize = 10;
        private int _pageSize = 4;

        public string IsComplete { get; set; }
        public string SearchQuery { get; set; }
        public string OrderBy { get; set; } = "Tags";
        public int PageSize { get => _pageSize; set => _pageSize = value > MaximumPageSize ? MaximumPageSize : value; }
        public int PageNumber { get; set; } = 1;
        public string Fields { get; set; }
    }
}
