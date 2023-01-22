namespace LifeHelper.Server.Models.EF
{
    public partial class DeleteConfirm
    {
        public Guid Id { get; set; }
        public string FeatureName { get; set; } = null!;
        public Guid FeatureId { get; set; }
        public Guid UserId { get; set; }
        public DateTime Deadline { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
