pushd Repository\Goblin.Api_Base.Repository
dotnet ef migrations add %1 -v --context GoblinDbContext
dotnet ef database update -v --context GoblinDbContext
popd