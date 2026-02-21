FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MarketPlaceBackend/*.csproj ./MarketPlaceBackend/
RUN dotnet restore MarketPlaceBackend/MarketPlaceBackend.csproj

COPY MarketPlaceBackend/ ./MarketPlaceBackend/
RUN dotnet publish MarketPlaceBackend/MarketPlaceBackend.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MarketPlaceBackend.dll"]