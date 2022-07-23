FROM registry.devfksolutions.com.br/base-images/dotnet/sdk:5.0-alpine AS build
# Copiar os arquivos para a imagem do container.
COPY . .
# Rodar o `dotnet build`
RUN dotnet publish -c Release -o out ShuffleDataMasking.Worker

### Gerar a imagem de produção
FROM registry.devfksolutions.com.br/base-images/dotnet/aspnet:5.0-alpine AS runtime
# Definir o diretório da aplicação.
USER app
WORKDIR /app
# Copiar o resultado do build.
COPY --from=build /app/out .
# Usar esta porta no container se for uma API. Caso contrário, delete estas linhas.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
# Definir se aplicação deve executar no modo invariante à globalização (padrão na imagem base fksolutions)
# https://docs.microsoft.com/dotnet/core/run-time-config/globalization
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
# Definir o entrypoint.
ENTRYPOINT ["dotnet", "ShuffleDataMasking.Worker.dll"]