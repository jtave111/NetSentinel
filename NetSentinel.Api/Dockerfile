    # ESTÁGIO 1: Build (Ambiente de Compilação)
    # Usamos o SDK 10.0 para compilar o código
    FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
    WORKDIR /app

    # Camada de cache para dependências
    COPY *.csproj ./
    RUN dotnet restore

    # Copia tudo e publica o binário
    COPY . ./
    RUN dotnet publish -c Release -o out

    # ESTÁGIO 2: Runtime (Ambiente de Execução)
    # Imagem ultra-leve apenas com o necessário para rodar .NET 10
    FROM mcr.microsoft.com/dotnet/aspnet:10.0
    WORKDIR /app

    # Trazemos apenas o resultado da compilação do estágio anterior
    COPY --from=build-env /app/out .

    # Porta que definimos para a API
    EXPOSE 5149

    ENTRYPOINT ["dotnet", "NetSentinel.Api.dll"]