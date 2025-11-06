using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class RawData : Basemodel
    {
        public DateOnly Date { get; set; }
        public string Pages { get; set; } = string.Empty;
        public int Users { get; set; }
        public int Sessions { get; set; }
        public int Views { get; set; }
        public double PerformanceScore { get; set; }
        public double LCPms { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
