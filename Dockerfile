FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS build
WORKDIR /src

COPY TauManager.Core/TauManager.Core.csproj ./TauManager.Core/
COPY TauManagerBot/TauManagerBot.csproj ./TauManagerBot/

WORKDIR /src/TauManager.Core/
RUN dotnet restore

WORKDIR /src/TauManagerBot/
RUN dotnet restore

WORKDIR /src
COPY TauManager.Core/ ./TauManager.Core/
COPY TauManagerBot/ ./TauManagerBot/

WORKDIR /src/TauManagerBot/
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic AS runtime
WORKDIR /app

COPY --from=build /src/TauManagerBot/out .

ENTRYPOINT ["dotnet", "TauManagerBot.dll"]
