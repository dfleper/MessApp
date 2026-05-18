FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY . .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://0.0.0.0:5000

CMD ["sh", "-c", "dotnet run --project MessApp.Api.csproj --urls http://0.0.0.0:${PORT:-5000}"]