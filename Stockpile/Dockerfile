FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Stockpile/Stockpile.csproj", "Stockpile/"]
COPY ["Stockpile.DAL/Stockpile.DAL.csproj", "Stockpile.DAL/"]

RUN dotnet restore "Stockpile/Stockpile.csproj"
COPY . .
WORKDIR "/src/Stockpile"
RUN dotnet build "Stockpile.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stockpile.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stockpile.dll"]
