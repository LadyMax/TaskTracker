using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TaskTracker.Core.Models;

public class OrderCriteria
{
    public OrderByField? OrderBy { get; set; }
    public bool Descending { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderByField
{
    Title,
    DueDate,
    Status
}
