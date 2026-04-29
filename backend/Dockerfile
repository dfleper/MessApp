FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

EXPOSE 5000

ENV ASPNETCORE_URLS=http://0.0.0.0:5000

CMD ["dotnet", "run", "--project", "MessApp.Api.csproj", "--urls", "http://0.0.0.0:5000"]