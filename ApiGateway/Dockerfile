FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar TODO o conteúdo do contexto para dentro da imagem
COPY . .

# Para debug (opcional, mas útil se falhar): listar arquivos copiados
# RUN ls -laR /src

RUN dotnet restore "ApiGateway.csproj"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Como o ocelot.json está na raiz do projeto, copiamos ele diretamente
COPY ApiGateway/ocelot.json .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
