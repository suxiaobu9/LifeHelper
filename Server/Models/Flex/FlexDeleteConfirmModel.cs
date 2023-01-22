namespace LifeHelper.Server.Models.Flex;

public record class FlexDeleteConfirmModel(Guid? Id, string FeatureName, Guid FeatureId)
{
    public string FeatureDisplay
    {
        get
        {
            return FeatureName switch
            {
                nameof(Models.EF.Accounting) => "記帳",
                nameof(Models.EF.Memorandum) => "備忘錄",
                _ => ""
            };
        }
    }
}
