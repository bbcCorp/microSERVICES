FROM microsoft/dotnet:2.1-runtime

RUN mkdir -p /app/data/logs
COPY . /app
WORKDIR /app

# Run dotnet when the container launches
ENTRYPOINT ["dotnet", "app.services.email.dll"]