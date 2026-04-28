FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

EXPOSE 5000

ENV ASPNETCORE_URLS=http://0.0.0.0:5000

CMD ["dotnet", "watch", "--project", "/app/MessApp.Api.csproj", "run", "--urls", "http://0.0.0.0:5000"]