using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public record PSIRecord
    {
        public string Date { get; init; } = "";
        public string Page { get; init; } = "";
        public double PerformanceScore { get; init; }
        public int LCP_ms { get; init; }
    }
}
