FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Se o contexto for a pasta do projeto, o arquivo .csproj deve ser copiado diretamente
COPY ["WebApi1.csproj", "./"]
RUN dotnet restore "./WebApi1.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "WebApi1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi1.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi1.dll"]
