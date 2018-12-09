FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Sources Sources
COPY MusicDb.ruleset .
WORKDIR /src/Sources/MusicDb
RUN dotnet restore MusicDb.csproj
RUN dotnet build MusicDb.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish MusicDb.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MusicDb.dll"]
