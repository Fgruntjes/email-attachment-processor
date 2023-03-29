# Opinionated CI/CD Framework using Pulumi, Github Actions and Docker

This project is an opinionated CI/CD framework that utilizes Pulumi, Github Actions, and Docker. It follows the following principles:
- Convention over configuration
- [OPTIONAL] Microservices architecture

## Features
- Provides an easy-to-use solution for building and deploying applications using modern CI/CD practices.
    - Static tests, style and quality tests
    - Unit tests [Unit Test assumtions](#unit-test-assumtions)
    - Integration tests
    - Always clean up environments
- Deployment are executed using [Pulumi](https://github.com/pulumi/pulumi).
- Default settings are used unless otherwise specified.

## Unit Test assumtions
- All projects ending with `.Test` except `App.Lib.Tests` will be concidered a test project.
- Test projects are started in parallel with seperate deployments.
- Services are started using `docker-compose up App.Lib.Tests/docker-compose.yaml`.
- Tests are run using `dotnet test`

## Install project usage: 
- Clone the project.
- Create `App.Setup/.env.local` and configure and required and overwrites from `App.Setup/.env`.
- Run `App.Setup/setup.sh` to configure all the required secrets and settings in your GH project.

## Contributing
Contributions are welcome! Open a pull request to suggest improvements or report any issues.
