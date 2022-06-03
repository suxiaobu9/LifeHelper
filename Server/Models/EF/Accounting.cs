﻿using System;
using System.Collections.Generic;

namespace LifeHelper.Server.Models.EF
{
    public partial class Accounting
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public string Event { get; set; } = null!;
        public int UserId { get; set; }
        public DateTime AccountDate { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
