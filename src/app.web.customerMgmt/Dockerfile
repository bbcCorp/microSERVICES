FROM microsoft/dotnet:2.1-aspnetcore-runtime

# RUN mkdir -p /app/data/logs
COPY . /app
WORKDIR /app

# We run the webapi on port 5000. Make port 5000 available to the world outside this container
EXPOSE 5000/tcp

# Run dotnet when the container launches
ENTRYPOINT ["dotnet", "app.web.customerMgmt.dll"]