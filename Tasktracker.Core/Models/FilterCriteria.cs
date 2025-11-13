using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.Core.Models;

public class FilterCriteria
{
    public Status? WithStatus { get; set; }
    public DateTime? DueBefore { get; set; }
}
