using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public record GARecord
    {
        public string Date { get; init; } = "";
        public string Page { get; init; } = "";
        public int Users { get; init; }
        public int Sessions { get; init; }
        public int Views { get; init; }
    }
}
