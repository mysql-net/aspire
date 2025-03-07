# .NET Aspire

## What is .NET Aspire?

.NET Aspire is an opinionated, cloud ready stack for building observable, production ready, distributed applications. .NET Aspire is delivered through a collection of NuGet packages that handle specific cloud-native concerns. Cloud-native apps often consist of small, interconnected pieces or microservices rather than a single, monolithic code base. Cloud-native apps generally consume a large number of services, such as databases, messaging, and caching.

.NET Aspire helps with:

[Orchestration](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview?#orchestration): .NET Aspire provides features for running and connecting multi-project applications and their dependencies.

[Components](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview?#net-aspire-components): .NET Aspire components are NuGet packages for commonly used services, such as Redis or Postgres, with standardized interfaces ensuring they connect consistently and seamlessly with your app.

[Tooling](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview?#project-templates-and-tooling): .NET Aspire comes with project templates and tooling experiences for Visual Studio and the dotnet CLI help you create and interact with .NET Aspire apps.

To learn more, read the full [.NET Aspire overview and documentation](https://learn.microsoft.com/dotnet/aspire/).

## What is in this repo?

The .NET Aspire application host, dashboard, service discovery infrastructure, and all .NET Aspire components. It also contains the project templates and a simple sample, 'eShopLite'.

You can find the full version of the eShop sample [here](https://github.com/dotnet/eshop) and the Azure version [here](https://github.com/Azure-Samples/eShopOnAzure).

## Using latest daily builds

Follow instructions in [docs/using-latest-daily.md](docs/using-latest-daily.md) to get started using .NET Aspire with the latest daily build.

## How can I contribute?

We welcome contributions! Many people all over the world have helped make .NET better.

Follow instructions in [docs/contributing.md](docs/contributing.md) for working in the code in the repository.

## Reporting security issues and security bugs

Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) <secure@microsoft.com>. You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the MSRC PGP key, can be found in the [Security TechCenter](https://www.microsoft.com/msrc/faqs-report-an-issue). You can also find these instructions in this repo's [Security doc](SECURITY.md).

Also see info about related [Microsoft .NET Core and ASP.NET Core Bug Bounty Program](https://www.microsoft.com/msrc/bounty-dot-net-core).

## .NET Foundation

.NET Aspire is a [.NET Foundation](https://www.dotnetfoundation.org/projects) project.

There are many .NET related projects on GitHub.

* [.NET home repo](https://github.com/Microsoft/dotnet) - links to 100s of .NET projects, from Microsoft and the community.
* [ASP.NET Core home](https://docs.microsoft.com/aspnet/core) - the best place to start learning about ASP.NET Core.

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org) to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](https://www.dotnetfoundation.org/code-of-conduct).

## License

The code in this repo is licensed under the [MIT](LICENSE.TXT) license.
