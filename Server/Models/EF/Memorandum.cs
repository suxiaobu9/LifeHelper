using System;
using System.Collections.Generic;

namespace LifeHelper.Server.Models.EF
{
    public partial class Memorandum
    {
        public int Id { get; set; }
        public string Memo { get; set; } = null!;
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
