using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Models.LineApi;

public record class LineReplyModel(LineReplyEnum LineReplyType, string Message);
