namespace MarketPlaceBackend.DTOs;

public class CommentDTO
{
    public int UserId { get; set; }
    public int PostId { get; set; }
    public string Content { get; set; }
}