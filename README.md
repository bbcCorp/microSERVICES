# microSERVICES

Developed by: Bedabrata Chatterjee


## Introduction
This application explores a simple service based architecture involving a Customer Entity.

The application has 
* AspNetCore WebAPI
* MongoDB based repository
* Apache Kafka based messaging 
* Email notification service

The idea is to have simple decoupled systems that are event driven. Customer API performs CRUD on the customer entity and generates a stream of events. In this app, we will generate two types of streams - one for CRUD and other for notification. Note that CRUD events are expected to be self contained propagator of state, so both before and after change information is contained in the same event.

Application events will be saved in Kafka using the topic `MICROSERVICE-CUSTOMER-UPDATES`. These can be used to sync up other dependent applications/services or event to reconstruct the MongoDB collection, by simply replaying the kafka logs.

Email notification server (app.services.email) pick up notification events and sends out the email messages. We use Kafka topic `MICROSERVICE-CUSTOMER-EMAIL-NOTIFICATION` for the notifications.

We will use Docker containers for the infrastructure blocks.

## Application Setup
The `setup` folder has the scripts required to build and start the production-like environment with all the infrastructure pieces. 

Use the `build.sh` script to build the application and `startup.sh` to bring up all the containers required to test the application.


## Running Unit Tests 

The app.tests project contains the unit test for the various components used in the project.

Go to the test folder and bring up the test containers `docker-compose up -d`

Make sure that `testsettings.json` is updated with SMTP server details if you want to test the email service.

## Tools, Framework and Libraries Used

* Microsoft AspNetCore and DotNetCore
* MongoDB
* Apache Kafka
* Docker
* Confluent.Kafka 
* Wire
* MailKit
* AutoMapper
* NLog