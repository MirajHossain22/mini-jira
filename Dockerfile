FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TaskTracker.sln ./
COPY src/TaskTracker.Api/TaskTracker.Api.csproj src/TaskTracker.Api/
COPY tests/TaskTracker.Tests/TaskTracker.Tests.csproj tests/TaskTracker.Tests/
RUN dotnet restore TaskTracker.sln
COPY . .
RUN dotnet publish src/TaskTracker.Api/TaskTracker.Api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskTracker.Api.dll"]
