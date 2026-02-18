# HtmxDualServer

> A dual-server architecture combining Blazor and HTMX for hybrid web applications.

## Description
HtmxDualServer demonstrates a dual-server pattern where a Blazor Shell application works alongside an HTMX-powered API server. This architecture allows teams to progressively adopt HTMX for lightweight hypermedia interactions while keeping Blazor for richer interactive components.

## Features
- Blazor Shell application for rich UI components
- HTMX API server for lightweight server-side rendering
- Docker containerized deployment
- Hybrid hypermedia + SPA architecture

## Getting Started
```bash
git clone https://github.com/phmatray/HtmxDualServer.git
cd HtmxDualServer
dotnet run --project HtmxApi
```

## License
MIT