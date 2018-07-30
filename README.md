# microSERVICES

Developed by: Bedabrata Chatterjee


## Introduction
This application explores a simple service based architecture involving an extremely simple Customer Entity.  

The idea is to have simple decoupled systems that is event driven. Customer API exposes simple CRUD APIs. APIs commiy changes to a data repository which is responsible for generating a stream of events. In this app, we will be working with two types of events - one for CRUD and other for notification. 

The CRUD events are designed to be self contained and can be used to propagate application state. Both before and after change information is contained in the same event message. We can use this for data replication.

The Email event contains all information required to send out email as well as retry counter and failure logs.

Note: 
1. With this kind of design, the notification and data replication services are not closely coupled with online/CRUD operations. 
2. We can recover from some service disruption without any issues. We can also scale these processes and run multiple copies of the mail or data replication service if load is high. 

3. We are using a single partition for the data replication queue so message order is guaranteed. For larger systems, this may not work. You can use a combination of id and timestamp to determine what needs to be done. 

4. In these kind of architecture we gain scale but add complexity and data staleness. We are settling for eventual consistency of data with event based replication. 

### Components
The application has the following high-level components
* AspNetCore WebAPI
* MongoDB as data repository
* ElasticSearch as a search server
* Apache Kafka based messaging 
* Email notification service
* Data replication service 

We will use Docker containers for the infrastructure blocks.

### Message Flows
Application events will be saved in Kafka using the topic `MICROSERVICE-CUSTOMER-UPDATES`. These can be used to sync up other dependent applications/services or event to reconstruct the MongoDB collection, by simply replaying the kafka logs.

Email notification server (app.services.email) pick up notification events and sends out the email messages. We use Kafka topic `MICROSERVICE-CUSTOMER-EMAIL-NOTIFICATION` for the notifications. Retries are built into the notification system using the same queue. After 3 attempts, we write the notification attempt to Kafka topic `MICROSERVICE-CUSTOMER-EMAIL-NOTIFICATION-FAILED`

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
* ElasticSearch
* Apache Kafka
* Docker
* Confluent.Kafka 
* Wire
* MailKit
* AutoMapper
* NLog