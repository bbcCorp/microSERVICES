#!/bin/sh
## ------------------------------------------------------------------------------------ ##
## build.sh
## This is a version of build script using multi-step docker builds
## This can be useful if you want a try out this project without any other dependecies.
## We rely on the following microsoft/dotnet images 
## 2.1-sdk, 2.1-aspnetcore-runtime and 2.1-runtime
## ------------------------------------------------------------------------------------ ##
# Release Timestamp
RDATE=`date +%Y%m%d`
RVERSION=1.0.0
RENV=Release

CURRENT_DIR=`pwd`

# Remove old docker container images
docker rm $(docker ps -a -q)

## We use a 2 step Docker build process. Intermediate build images are created from which final images are setup
cd ../src

## ----------------------- Build API --------------------------- ##
docker build -t microservices-api-customers:1.0.0 -f api-customer.Dockerfile . 
docker build -t microservices-api-customers:$RVERSION -f api-customer.Dockerfile . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-api-customers:$RVERSION"

## ----------------------- Build Notification Service --------------------------- ##
docker build -t microservices-service-notification:$RVERSION -f service-email.Dockerfile . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-service-notification:$RVERSION"

## ----------------------- Build Data Replication Service --------------------------- ##
docker build -t microservices-service-replication:$RVERSION -f service-replication.Dockerfile . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-service-replication:$RVERSION"

## ----------------------- Build Customer Management WebApp --------------------------- ##
docker build -t microservices-web-customermgmt:$RVERSION -f web-customermgmt.Dockerfile . 
echo "[`date +%Y%m%d_%H:%M:%S`] Created Docker image microservices-web-customermgmt:$RVERSION"

# -------------------------------------------------------------------------------------- ##
# Remove dangling images - one with <none>
docker rmi $(docker images -f "dangling=true" -q)