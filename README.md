# microSERVICES

Developed by: Bedabrata Chatterjee


## Introduction
This application explores a simple service based architecture involving an extremely simple Customer Entity.  

We have a simple web application that is used to manage Customers. The application does not convern itself with the business of storing and retrieving entities, or enforcing any other functionalities like notification or data replication. Instead it relies on simple WebAPIs.

Customer API exposes simple CRUD APIs. APIs commiy changes to a data repository which is responsible for generating a stream of events. In this app, we will be working with two types of events - one for CRUD and other for notification. 

The CRUD events are designed to be self contained and can be used to propagate application state. Both before and after change information is contained in the same event message. We can use this for data replication.

The Email event contains all information required to send out email as well as retry counter and failure logs.

There will be one or more services that will pick up the events and trigger the required actions. 

The idea is to have simple decoupled systems that is event driven. Each component is individually scalable.


Note: 
1. With this kind of design, the notification and data replication services are not closely coupled with online/CRUD operations. 
2. We can recover from some service disruption without any issues. We can also scale these processes and run multiple copies of the mail or data replication service if load is high. 

3. We are using a single partition for the data replication queue so message order is guaranteed. For larger systems, this may not work. You can use a combination of id and timestamp to determine what needs to be done. 

4. In these kind of architecture we gain scale but add complexity and data staleness. We are settling for eventual consistency of data with event based replication. 

### Components
The application has the following high-level components
* AspNetCore WebAPI2
* AspNetCore 2.1 Web application with Razor Pages 
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
The `setup` folder has all the scripts required to build and start the production-like environment with all the infrastructure pieces. 

Use the `build.sh` script to build the docker images for application and `startup.sh` to bring up all the containers required to test the application. You can bring down the setup by running the script `shutdown.sh`


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

## Running the application

[On Linux and Mac]
* Run the `build.sh` script to build the docker images. It will download the build and runtime images.

* Once the docker images are ready, run the `startup.sh` script.

* You can run the web application from the host machine using the url: `http://localhost:8080`

* Run the `shutdown.sh` to tear down the deployment.

NOTE: 

* The setup partitions the network so that only the web application and the notification service can access the external network. The remaining services including the API and the MongoDB servers are on a backend network and would not be accesible. If you need to access them for developmental resons, update the `docker-compose.yml` file and uncomment the relevant lines in the `networks` section of the respective services.

### Using HTTPS

* To use HTTPS version of the web application, you will need to use a .pfx files that contain the public key file (SSL certificate file) and the associted password.

* The `build.sh` script uses dotnet `dev-certs` command to generate a self-signed cert for localhost. The cert uses the default name and password and is stored in setup/certs folder. You may need to force your browser to trust the certificate if it is generated using the self-signed scheme. 

* You can provide your own certificates in the cert folder, in which case the self-signed certificate generation step is skipped.

* Update the `docker-compose.yml` with the path and password for the certificates. Update the keys containing the name - Kestrel__Certificates

* For the web application update ASPNETCORE_ENVIRONMENT=Staging to force use of HTTPS over HTTP. 

* From the host machine, you should now be able to access the application using the URL `https://localhost:8081/`