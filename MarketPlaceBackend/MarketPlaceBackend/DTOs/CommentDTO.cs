namespace MarketPlaceBackend.DTOs
{
    public class CommentDTO
    {
        public int PostId { get; set; }
        public string Content { get; set; }
    }
    public class UpdatedCommentDTOs
    {
        public string Content { get; set; }
    }
}