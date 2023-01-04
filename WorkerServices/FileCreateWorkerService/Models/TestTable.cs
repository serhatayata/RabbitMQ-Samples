using System;
using System.Collections.Generic;

namespace FileCreateWorkerService.Models
{
    public partial class TestTable
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string? Gender { get; set; }
        public string Tcno { get; set; } = null!;
    }
}
