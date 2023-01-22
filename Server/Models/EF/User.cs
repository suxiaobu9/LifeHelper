namespace LifeHelper.Server.Models.EF
{
    public partial class User
    {
        public User()
        {
            Accountings = new HashSet<Accounting>();
            DeleteConfirms = new HashSet<DeleteConfirm>();
            Memoranda = new HashSet<Memorandum>();
        }

        public Guid Id { get; set; }
        public string LineUserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsAdmin { get; set; }

        public virtual ICollection<Accounting> Accountings { get; set; }
        public virtual ICollection<DeleteConfirm> DeleteConfirms { get; set; }
        public virtual ICollection<Memorandum> Memoranda { get; set; }
    }
}
