FROM microsoft/dotnet

RUN mkdir -p /app/data/logs
COPY . /app
WORKDIR /app

# Run dotnet when the container launches
ENTRYPOINT ["dotnet", "app.services.replication.dll"]