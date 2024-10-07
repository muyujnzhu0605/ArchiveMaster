using System.ComponentModel.DataAnnotations;

namespace ArchiveMaster.Models;

public class EntityBase
{
    [Key]
    public int Id { get; set; }
}