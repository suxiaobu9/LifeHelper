namespace LifeHelper.Server.Repositories;

public class MemorandumRepository : Repository<Memorandum>
{
    public MemorandumRepository(LifeHelperContext db) : base(db) { }

}