using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

public class Position
{
    [Required, StringLength(50)]
    public string Id { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
