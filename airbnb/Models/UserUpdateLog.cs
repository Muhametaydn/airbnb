using System.ComponentModel.DataAnnotations;


public class UserUpdateLog
{

    [Key]
    public int LogId { get; set; }
    public int UserId { get; set; }
    public string? OldFirstName { get; set; }
    public string? NewFirstName { get; set; }
    public string? OldLastName { get; set; }
    public string? NewLastName { get; set; }
    public DateTime UpdatedAt { get; set; }
}
