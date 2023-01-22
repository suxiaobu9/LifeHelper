using System;
using System.Collections.Generic;

namespace LifeHelper.Server.Models.EF
{
    public partial class Accounting
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public string Event { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime AccountDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
