using System;
using System.Collections.Generic;

namespace LifeHelper.Server.Models.EF
{
    public partial class DeleteConfirm
    {
        public int Id { get; set; }
        public string FeatureName { get; set; } = null!;
        public int FeatureId { get; set; }
        public int UserId { get; set; }
        public DateTime Deadline { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
