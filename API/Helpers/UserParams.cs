using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int pageSize = 10;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        // mora da se doda ? inace kuka kako nisu prosledjeni parametri u query stringu
        public string? CurrentUsername { get; set; }
        public string? Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 180;
        public string OrderBy { get; set; } = "lastActive";
    }
}
