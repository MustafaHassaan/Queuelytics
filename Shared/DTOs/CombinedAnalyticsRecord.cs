using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public record CombinedAnalyticsRecord
    {
        public DateTime Date { get; init; }
        public string Page { get; init; }
        public int Users { get; init; }
        public int Sessions { get; init; }
        public int Views { get; init; }
        public double PerformanceScore { get; init; }
        public int LCPms { get; init; }
        public DateTime ReceivedAt { get; init; }
    }
}
