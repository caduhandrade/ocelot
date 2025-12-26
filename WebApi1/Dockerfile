FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar TODO o conteúdo do contexto para dentro da imagem
COPY . .

# Se estivermos na raiz do projeto (contexto = pasta do projeto), o .csproj já está aqui.
# Se estivermos na raiz do repo (contexto = raiz), precisamos entrar na pasta.
# Vamos tentar restaurar assumindo que estamos na pasta correta (Build Path configurado corretamente)
# Caso contrário, o COPY . . já trouxe tudo, então podemos procurar o arquivo.

RUN dotnet restore "WebApi1.csproj"
RUN dotnet build "WebApi1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi1.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi1.dll"]
