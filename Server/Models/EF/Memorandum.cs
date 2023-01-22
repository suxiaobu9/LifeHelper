namespace LifeHelper.Server.Models.EF
{
    public partial class Memorandum
    {
        public Guid Id { get; set; }
        public string Memo { get; set; } = null!;
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
