namespace api.Model
{
    public class Todo
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public bool IsCompleted { get; set; }
        public Guid UserId { get; set; }
    }
}
