using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class UserParams : PaginationParams
    {
        // mora da se doda ? inace kuka kako nisu prosledjeni parametri u query stringu
        public string? CurrentUsername { get; set; }
        public string? Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 180;
        public string OrderBy { get; set; } = "lastActive";
    }
}
