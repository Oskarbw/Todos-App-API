namespace api.Model
{
    public class TodoDto
    {
        public string Content { get; set; }
        public bool IsCompleted { get; set; }
        public Guid UserId { get; set; }
    }
}
