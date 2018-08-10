## Dockerfile to build customer api with all dependent projects
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 5000


FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY . .
WORKDIR /src/app.api.customers

RUN dotnet restore
RUN dotnet build --no-restore -c Release -o /app

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app



FROM base AS final
WORKDIR /app
COPY --from=publish /app .

# Run dotnet when the container launches
ENTRYPOINT ["dotnet", "app.api.customers.dll"]