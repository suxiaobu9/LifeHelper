using System;
using System.Collections.Generic;

namespace LifeHelper.Server.Models.EF
{
    public partial class DeleteAccount
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime Deadline { get; set; }

        public virtual Accounting Account { get; set; } = null!;
    }
}
