#!/bin/sh
## ---------------------------------------------------------------------------- ##
## build_with_publish.sh
## This is a version of build script using dotnet publish command
## This can be useful if you want a package based deployment  
## and build runtime images on the server based on the package.
## ---------------------------------------------------------------------------- ##

# Release Timestamp
RDATE=`date +%Y%m%d`
RVERSION=1.0.0
RENV=Release

CURRENT_DIR=`pwd`

# Remove old docker container images
docker rm $(docker ps -a -q)

## ----------------------- Build API --------------------------- ##
PUBLISH_FOLDER="$CURRENT_DIR/../build/microSERVICES-$RVERSION-$RDATE/api-customers"
mkdir -p $PUBLISH_FOLDER

echo "Publishing to $PUBLISH_FOLDER"
echo "[`date +%Y%m%d_%H:%M:%S`] Building Release files for api-customers:$RVERSION"
dotnet restore "../src/app.api.customers/app.api.customers.csproj"
dotnet publish "../src/app.api.customers/app.api.customers.csproj"  --output $PUBLISH_FOLDER --framework netcoreapp2.1 --configuration $RENV

cd $PUBLISH_FOLDER
# Use the docker file to create a build a docker image 
docker build -t microservices-api-customers:$RVERSION . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-api-customers:$RVERSION"


## ----------------------- Build Customer Management WebApp --------------------------- ##
cd $CURRENT_DIR
PUBLISH_FOLDER="$CURRENT_DIR/../build/microSERVICES-$RVERSION-$RDATE/web-customermgmt"
mkdir -p $PUBLISH_FOLDER

echo "Publishing to $PUBLISH_FOLDER"
echo "[`date +%Y%m%d_%H:%M:%S`] Building Release files for web-customermgmt:$RVERSION"
dotnet restore "../src/app.web.customerMgmt/app.web.customerMgmt.csproj"
dotnet publish "../src/app.web.customerMgmt/app.web.customerMgmt.csproj"  --output $PUBLISH_FOLDER --framework netcoreapp2.1 --configuration $RENV

cd $PUBLISH_FOLDER
# Use the docker file to create a build a docker image 
docker build -t microservices-web-customermgmt:$RVERSION . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-web-customermgmt:$RVERSION"

## ----------------------- Build Notification Service --------------------------- ##
cd $CURRENT_DIR
PUBLISH_FOLDER="$CURRENT_DIR/../build/microSERVICES-$RVERSION-$RDATE/service-notification"
mkdir -p $PUBLISH_FOLDER

echo "Publishing to $PUBLISH_FOLDER"
echo "[`date +%Y%m%d_%H:%M:%S`] Building Release files for service-notification:$RVERSION"
dotnet restore "../src/app.services.email/app.services.email.csproj"
dotnet publish "../src/app.services.email/app.services.email.csproj"  --output $PUBLISH_FOLDER --framework netcoreapp2.1 --configuration $RENV

cd $PUBLISH_FOLDER
# Use the docker file to create a build a docker image 
docker build -t microservices-service-notification:$RVERSION . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-service-notification:$RVERSION"


## ----------------------- Build Data Replication Service --------------------------- ##
cd $CURRENT_DIR
PUBLISH_FOLDER="$CURRENT_DIR/../build/microSERVICES-$RVERSION-$RDATE/service-replication"
mkdir -p $PUBLISH_FOLDER

echo "Publishing to $PUBLISH_FOLDER"
echo "[`date +%Y%m%d_%H:%M:%S`] Building Release files for service-replication:$RVERSION"
dotnet restore "../src/app.services.replication/app.services.replication.csproj"
dotnet publish "../src/app.services.replication/app.services.replication.csproj"  --output $PUBLISH_FOLDER --framework netcoreapp2.1 --configuration $RENV

cd $PUBLISH_FOLDER
# Use the docker file to create a build a docker image 
docker build -t microservices-service-replication:$RVERSION . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-service-replication:$RVERSION"

# -------------------------------------------------------------------------------------- ##
# Remove dangling images - one with <none>
docker rmi $(docker images -f "dangling=true" -q)