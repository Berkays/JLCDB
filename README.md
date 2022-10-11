# JLCDB

> Easy to use query based search engine for **JLCPCB SMT** service.

![GitHub Discussions](https://img.shields.io/github/discussions/Berkays/JLCDB?style=for-the-badge)
![GitHub Repo stars](https://img.shields.io/github/stars/berkays/JLCDB?color=brightgreen&style=for-the-badge)

![JLCDB Header](repo-header.gif)

## Demo

**Try out the live demo at
[jlcdb-web.onrender.com](https://jlcdb-web.onrender.com)**

> Live demo web service and database updater application is hosted on [Render.com](https://render.com) using Web Service and Cron Job configurations. A serverless MongoDB instance is used from [MongoDB Cloud](https://www.mongodb.com/use-cases/serverless)

## Summary

**JLCDB** is a simple query based component search engine for [JLCPCB](https://jlcpcb.com) SMT parts library. This project is aimed for ease of use by avoiding complicated tables and filters that occur in such web pages but instead using a query based search system. Repository consists of two projects including a Csv Parser and a Web Server.

_Csv Parser_ application fetches latest CSV file and updates the MongoDB instance using the parsed values from component descriptions. File is updated approximately once a week so it is suitable to use a cronjob to run this application as in the demo application. Demo application uses render.com CronJob service.

_Web server_ simply creates a websocket connection with react client for processing queries and building a MongoDB filter which then passed to a MongoDB instance. Additional table sorting and filtering can be done in the react application. Live demo deployment uses Web Service configuration from **render.com**

Serverless MongoDB instance is used from [MongoDB Cloud](https://www.mongodb.com/use-cases/serverless)

## Features

- Simple and responsive interface
- Easy to use query based search
- Autocompletion of possible categories and component features
- Downloading results as CSV file
- Table sorting and filtering
- Automatic updating of [parts database](https://jlcpcb.com/parts)

## Installation

Self contained production builds can be produced using the provided **docker** images. These images can directly be used in a cloud provider such as [Render.com](https://render.com)

- [Dockerfile.CsvParser](https://github.com/Berkays/JLCDB/blob/main/Dockerfile.CsvParser)
- [Dockerfile.Web](https://github.com/Berkays/JLCDB/blob/main/Dockerfile.Web)

Development containers also exist in the root directory for use with [Vscode Dev Containers](https://github.com/microsoft/vscode-dev-containers).
Devcontainers include a MongoDB container

> Docker images do not require any external dependencies other than Docker itself.

### CSV Parser Requirements

- Iconv (Included with Macos/Linux distributions)
- [.NET Core 6.0](https://dotnet.microsoft.com/en-us/download)

### Web Service Requirements

- [.NET Core 6.0](https://dotnet.microsoft.com/en-us/download)
- [NodeJS 16.x LTS](https://nodejs.org/en/) _(Not needed at runtime)_

### Build & Run

```
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

## Deployment

Deployment to various cloud providers such as [Render.com](https://render.com) can easily be done by using the images located in the root directory. A MongoDB instance is required.

- [Dockerfile.CsvParser](https://github.com/Berkays/JLCDB/blob/main/Dockerfile.CsvParser)
- [Dockerfile.Web](https://github.com/Berkays/JLCDB/blob/main/Dockerfile.Web)

Make sure to pass the required enviroment variables while deploying images.

Context directory must be specified root directory and Dockerfile name should be provided e.g: _Dockerfile.Web_

### Enviroment Variables

<br/>

**CSV Parser**

| Variable                     | Description                                                                                                | Type          |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------- | ------------- |
| Database\_\_ConnectionString | MongoDB connection string. Compressors are automatically added in the code.                                | string        |
| Database\_\_Name             | Name of the database to write the records.                                                                 | string        |
| UseLocalDownload (optional)  | Use locally downloaded csv file during development. CSV must be placed in the CSVParser project directory. | true \| false |

<br/>

**Web Service**

| Variable                     | Description                                                                 | Type   |
| ---------------------------- | --------------------------------------------------------------------------- | ------ |
| Database\_\_ConnectionString | MongoDB connection string. Compressors are automatically added in the code. | string |
| Database\_\_Name             | Name of the database to write the records.                                  | string |

### Publish

`dotnet publish` command can be used a create a production build.

```
dotnet publish ./Web/Web.csproj -c Release
```

Application executable and all runtime dependencies will be included in the `bin/Release/net6.0/publish` folder.

Set enviroment variables and run the application.

```
export Database__ConnectionString="" && export Database__Name="data" && ./Web/bin/Release/net6.0/publish/Web
```

### Running Docker Containers

1. Build the image

   `docker build -t JLCDB/Web -f Dockerfile.Web`

2. Create and run a container

   `docker run -it --name jlcdb-web-server -e Database__ConnectionString="mongodb://root:root@host.docker.internal:27017/admin?authSource=admin" -e Database__Name="data" -p 80:80 JLCDB/Web`

## Todo

- [ ] Query syntax highlighting and validation
- [ ] Implement more category parsers
- [ ] Full text description filtering with regex
- [ ] Parse price ranges and display an estimated price calculator
- [ ] In stock filter
- [ ] Save queries
- [ ] Filter impossible queries in server
- [ ] Individual part information request
- [ ] Price estimation of component cart

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

berkaygursoy@gmail.com
