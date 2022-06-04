namespace LifeHelper.Server.Models.Flex;

/// <summary>
/// 
/// </summary>
/// <param name="DeleteConfirmId"></param>
/// <param name="FeatureName"></param>
/// <param name="Description"></param>
public record class DeleteConfirmModel(int DeleteConfirmId, string FeatureName, string Description)
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

/// <summary>
/// 
/// </summary>
/// <param name="FeatureName"></param>
/// <param name="FeatureId"></param>
public record class DeleteFeatureModel(string FeatureName, int FeatureId);
