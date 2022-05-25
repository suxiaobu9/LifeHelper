dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer -o Models\EF\ --force --no-onconfiguring --context LifeHelperContext

pause