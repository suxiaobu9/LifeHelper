using System;
using System.Collections.Generic;

namespace LifeHelper.Server.Models.EF
{
    public partial class User
    {
        public Guid Id { get; set; }
        public string LineUserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsAdmin { get; set; }
    }
}
