![htmx-samples banner](.github/banner.png)

# htmx Samples

<!-- portfolio-badges:start -->
<!-- Identity -->
[![Atypical-Consulting - htmx-samples](https://img.shields.io/static/v1?label=Atypical-Consulting&message=htmx-samples&color=blue&logo=github)](https://github.com/Atypical-Consulting/htmx-samples)
![Top language](https://img.shields.io/github/languages/top/Atypical-Consulting/htmx-samples)
[![Stars](https://img.shields.io/github/stars/Atypical-Consulting/htmx-samples?style=social)](https://github.com/Atypical-Consulting/htmx-samples/stargazers)
[![Forks](https://img.shields.io/github/forks/Atypical-Consulting/htmx-samples?style=social)](https://github.com/Atypical-Consulting/htmx-samples/network/members)
[![License](https://img.shields.io/github/license/Atypical-Consulting/htmx-samples)](https://github.com/Atypical-Consulting/htmx-samples/blob/HEAD/LICENSE)

<!-- Activity -->
[![Issues](https://img.shields.io/github/issues/Atypical-Consulting/htmx-samples)](https://github.com/Atypical-Consulting/htmx-samples/issues)
[![Pull requests](https://img.shields.io/github/issues-pr/Atypical-Consulting/htmx-samples)](https://github.com/Atypical-Consulting/htmx-samples/pulls)
[![Last commit](https://img.shields.io/github/last-commit/Atypical-Consulting/htmx-samples)](https://github.com/Atypical-Consulting/htmx-samples/commits)
<!-- portfolio-badges:end -->

<!-- portfolio-toc:start -->

## Table of Contents

- [Samples](#samples)
- [Features](#features)
- [Getting Started](#getting-started)
- [History](#history)
- [License](#license)

<!-- portfolio-toc:end -->



> Ways to build **HTMX-powered .NET apps** — a starter template, an MVC reference,
> and a dual-server experiment, consolidated in one place (full git history preserved).

Looking for the component library? That stays on its own:
**[FastComponents](https://github.com/Atypical-Consulting/FastComponents)** — server-side
Blazor components rendered as HTMX. These samples show patterns; FastComponents is the reusable lib.

## Samples

| Path | What it is | From |
|---|---|---|
| [`minimal-template/`](minimal-template) | A **minimal `dotnet new` template** for HTMX-based Blazor apps | `phmatray/MinimalHtmx` ★ |
| [`mvc-reference/`](mvc-reference) | A **reference ASP.NET Core MVC + HTMX 2** app — hexagonal architecture | `Atypical-Consulting/aspnet-htmx-mvc` ★ |
| [`dual-server/`](dual-server) | A **dual-server** experiment combining Blazor and HTMX | `phmatray/HtmxDualServer` |

## Features

- **Three htmx approaches** side by side — template, MVC reference, dual-server
- **.NET / C#** throughout
- **One home** — shared issues, discussions and history

## Getting Started

```bash
git clone https://github.com/Atypical-Consulting/htmx-samples.git
cd htmx-samples/minimal-template   # or mvc-reference / dual-server
dotnet run
```

## History

Each folder was merged with **full git history preserved** (`git subtree`). The
original repositories are archived and redirect here.

<!-- portfolio-techstack:start -->

## Tech Stack

- **.NET 10 · .NET 8**
- TheAppManager
- Microsoft.AspNetCore.OpenApi
- Swashbuckle.AspNetCore
- Microsoft.TemplateEngine.Tasks
- Carter

<!-- portfolio-techstack:end -->

<!-- portfolio-roadmap:start -->

## Roadmap

Planned work and known limitations are tracked in the [open issues](https://github.com/Atypical-Consulting/htmx-samples/issues). Contributions toward them are welcome.

<!-- portfolio-roadmap:end -->

## License

MIT — see [`LICENSE`](LICENSE).
