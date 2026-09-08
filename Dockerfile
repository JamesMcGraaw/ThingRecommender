FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/ThingRecommender.Api/ThingRecommender.Api.csproj backend/src/ThingRecommender.Api/
COPY backend/src/ThingRecommender.Application/ThingRecommender.Application.csproj backend/src/ThingRecommender.Application/
COPY backend/src/ThingRecommender.Infrastructure/ThingRecommender.Infrastructure.csproj backend/src/ThingRecommender.Infrastructure/
COPY backend/src/ThingRecommender.Domain/ThingRecommender.Domain.csproj backend/src/ThingRecommender.Domain/
RUN dotnet restore backend/src/ThingRecommender.Api/ThingRecommender.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/ThingRecommender.Api/ThingRecommender.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ThingRecommender.Api.dll"]
