# SOFT1Exam_Notification


## Table of Contents

- [SOFT1Exam\_Notification](#soft1exam_notification)
  - [Table of Contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Build Status](#build-status)
  - [CI/CD Pipeline](#cicd-pipeline)
    - [Pipeline Steps](#pipeline-steps)
  - [Tech stack](#tech-stack)
  - [Message broker Documentation](#message-broker-documentation)
    - [RabbitMQ](#rabbitmq)
  - [Docker Compose](#docker-compose)
    - [Overview](#overview)
    - [Dockerhub](#dockerhub)
    - [Services / Containers](#services--containers)

## Introduction

Welcome to the **SOFT1Exam_Notification** repository! This is one of 7 mircoservices for our first semester software PBA exam "MTOGO". This microservice is responsible for managing Email notifications to the customers, by implementing Sendgrid email service.




## Build Status
Check out the lastest build status here: ![CI/CD](https://github.com/LucasHemm/SOFT1Exam_Notification/actions/workflows/dotnet-tests.yml/badge.svg)

## CI/CD Pipeline

Our CI/CD pipeline utilizes GitHub Actions to automate the testing and deployment of the application. This uses our whole suite of tests. To initate the pipeline a pull request has to be made to merge the code. After the request has been made the pipeline will run the tests, and deploy the image of the application to dockerhub if all the tests pass.

The pipeline consists of the following steps:

### Pipeline Steps

1. **Checkout Repository**
2. **Setup .NET**
3. **Restore Dependencies**
4. **Build**
5. **Wait for RabbitMQ to be Ready**
6. **Test**
7. **Log in to Docker Hub**
8. **Build Docker Image**
9. **Push Docker Image** 

## Tech stack
The tech stack for this microservice is as follows:
- **C#**: The main programming language for the application.
- **ASP NET Core 8.0**: The main framework for the application.
- **Rabbitmq**: The message broker used for communication between microservices.
- **Prometheus**: The library used for metrics.
- **Grafana**: The library used for visualizing the metrics.
- **Docker**: The containerization tool used to deploy the application.
- **Docker Compose**: The tool used to deploy the application locally.
- **GitHub Actions**: The CI/CD tool used to automate the testing and deployment of the application.
- **Swagger**: The library used to document the API.
- **xunit**: The library used for unit and integration testing.
- **Testcontainers**: The library used to create testcontainers for the integration tests.
- **Coverlet**: The library used to create code coverage reports.

## Message broker Documentation
### RabbitMQ

| **Queue Name** | **Exchange** | **Routing Key** | **Description**                                                                 | **Message Format** |
|----------------|--------------|-----------------|---------------------------------------------------------------------------------|--------------------|
| `email_queue`  | `default`    | `email_queue`   | Receives messages containing information needed to send emails to customers.    | JSON               |

The message format for the `email_queue` is as follows:

```json
{
  "email": "string",
  "subject": "string",
  "content": "string"
}
```

## Docker Compose

### Overview

To run this microservice, you can use Docker Compose to deploy the services locally. 

```yaml
docker-compose up --build
```

See performance metrics at:
```
http://localhost:8086/metrics
```
Or use the prometheus UI at:
```
http://localhost:9096
```
And the grafana UI at:
```
http://localhost:3006
```

Alternatively, you can run all the services from the MTOGO project by going to this repository and following the guide there.
```
https://github.com/LucasHemm/SOFTEXAM_Deployment
```

### Dockerhub
The docker-compose file uses the local dockerfile to build the image, and run the container. The image can also be found on Docker Hub at:
```
https://hub.docker.com/u/lucashemcph
```

### Services / Containers

- **notificationbrokerservice** / **notificationbrokerservicecontainer**: Runs the main application server.
- **rabbitmq** / **rabbitmqnotification**: Runs the RabbitMQ server.
- **prometheus** / **prometheus**: Runs the prometheus server.
- **grafana** / **grafana**: Runs the grafana server.







