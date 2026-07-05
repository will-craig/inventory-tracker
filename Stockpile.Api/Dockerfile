FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Stockpile.Api/Stockpile.Api.csproj", "Stockpile.Api/"]
COPY ["Stockpile.Application/Stockpile.Application.csproj", "Stockpile.Application/"]
COPY ["Stockpile.DAL/Stockpile.DAL.csproj", "Stockpile.DAL/"]
COPY ["Stockpile.Domain/Stockpile.Domain.csproj", "Stockpile.Domain/"]

RUN dotnet restore "Stockpile.Api/Stockpile.Api.csproj"
COPY . .
WORKDIR "/src/Stockpile.Api"

FROM build AS publish
RUN dotnet publish "Stockpile.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stockpile.Api.dll"]
