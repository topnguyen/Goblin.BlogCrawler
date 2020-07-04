# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .

COPY Goblin.Core/Goblin.Core/*.csproj ./Goblin.Core/Goblin.Core/
COPY Goblin.Core/Goblin.Core.Web/*.csproj ./Goblin.Core/Goblin.Core.Web/

COPY src/Cross/Goblin.BlogCrawler.Core/*.csproj ./src/Cross/Goblin.BlogCrawler.Core/
COPY src/Cross/Goblin.BlogCrawler.Mapper/*.csproj ./src/Cross/Goblin.BlogCrawler.Mapper/
COPY src/Cross/Goblin.BlogCrawler.Share/*.csproj ./src/Cross/Goblin.BlogCrawler.Share/

COPY src/Repository/Goblin.BlogCrawler.Contract.Repository/*.csproj ./src/Repository/Goblin.BlogCrawler.Contract.Repository/
COPY src/Repository/Goblin.BlogCrawler.Repository/*.csproj ./src/Repository/Goblin.BlogCrawler.Repository/

COPY src/Service/Goblin.BlogCrawler.Contract.Service/*.csproj ./src/Service/Goblin.BlogCrawler.Contract.Service/
COPY src/Service/Goblin.BlogCrawler.Service/*.csproj ./src/Service/Goblin.BlogCrawler.Service/

COPY src/Web/Goblin.BlogCrawler/*.csproj ./src/Web/Goblin.BlogCrawler/

RUN dotnet restore

# copy everything else and build app

COPY Goblin.Core/Goblin.Core/. ./Goblin.Core/Goblin.Core/
COPY Goblin.Core/Goblin.Core.Web/. ./Goblin.Core/Goblin.Core.Web/

COPY src/Cross/Goblin.BlogCrawler.Core/. ./src/Cross/Goblin.BlogCrawler.Core/
COPY src/Cross/Goblin.BlogCrawler.Mapper/. ./src/Cross/Goblin.BlogCrawler.Mapper/
COPY src/Cross/Goblin.BlogCrawler.Share/. ./src/Cross/Goblin.BlogCrawler.Share/

COPY src/Repository/Goblin.BlogCrawler.Contract.Repository/. ./src/Repository/Goblin.BlogCrawler.Contract.Repository/
COPY src/Repository/Goblin.BlogCrawler.Repository/. ./src/Repository/Goblin.BlogCrawler.Repository/

COPY src/Service/Goblin.BlogCrawler.Contract.Service/. ./src/Service/Goblin.BlogCrawler.Contract.Service/
COPY src/Service/Goblin.BlogCrawler.Service/. ./src/Service/Goblin.BlogCrawler.Service/

COPY src/Web/Goblin.BlogCrawler/. ./src/Web/Goblin.BlogCrawler/

WORKDIR /source
RUN dotnet publish -c release -o /publish --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /publish
COPY --from=build /publish ./
ENTRYPOINT ["dotnet", "Goblin.BlogCrawler.dll"]