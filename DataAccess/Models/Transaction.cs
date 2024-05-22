using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public class Transaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public DateTime Date { get; set; }

    public string Category { get; set; }

    public decimal Amount { get; set; }
    
    public string Type { get; set; }

}

