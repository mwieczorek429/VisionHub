FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test

WORKDIR /src

COPY . .

RUN dotnet restore VisionHub.Api.Tests/VisionHub.Api.Tests.csproj
RUN dotnet build VisionHub.Api.Tests/VisionHub.Api.Tests.csproj -c Release

CMD ["dotnet", "test", "VisionHub.Api.Tests/VisionHub.Api.Tests.csproj", "-c", "Release"]
