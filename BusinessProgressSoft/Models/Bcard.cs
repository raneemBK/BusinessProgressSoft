using System;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;

namespace BusinessProgressSoft.Models;

public partial class Bcard
{
    [Ignore]
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Gender { get; set; }

    public DateTime? Birth { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Photo { get; set; }

    public string? Address { get; set; }
}
