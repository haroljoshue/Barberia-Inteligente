FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .

RUN dotnet publish Barberia.Api/Barberia.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_ENVIRONMENT=Production

CMD ASPNETCORE_URLS=http://0.0.0.0:$PORT dotnet Barberia.Api.dll