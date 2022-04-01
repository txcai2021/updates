#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80 443 5000

ENV ASPNETCORE_URLS=http://+:80;http://+:5000
ENV RPS_DB_INTEGRATION="Data source= host.docker.internal\\SQLEXPRESS01,1433;Initial Catalog=SIMTech.APS.Integration;User Id=sa;Password=HMLVsa201301;"
ENV RPS_UI_URL=http://localhost:4200
ENV RPS_SALESORDER_URL=http://host.docker.internal:5112/api/salesorder/
ENV RPS_CUSTOMER_URL=http://host.docker.internal:5108/api/customer/
ENV RPS_WORKORDER_URL=http://host.docker.internal:5118/api/workorder/
ENV RPS_INVENTORY_URL=http://host.docker.internal:5108/api/inventory/
ENV RPS_RESOURCE_URL=http://host.docker.internal:5113/api/resource/
ENV RABBITMQ_HOST=host.docker.internal
ENV RABBITMQ_PORT=31672 
ENV RABBITMQ_USERNAME=guest
ENV RABBITMQ_PASSWORD=guest
ENV RABBITMQ_VHOST=/
ENV RABBITMQ_ENABLE=100
ENV RABBITMQ_EXCHANGE=cpps-rps
ENV RABBITMQ_QUEUE_SALESORDER=rps-salesorder
ENV RABBITMQ_QUEUE_CUSTOMER=rps-customer
ENV RABBITMQ_QUEUE_INVENTORY=rps-inventory
ENV RABBITMQ_QUEUE_TS_PROCESS=rps-ts-process
ENV RABBITMQ_QUEUE_TS_WORKORDER=rps-ts-workorder
ENV RABBITMQ_QUEUE_MACHINE=rps-machineblockout

      
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SIMTech.APS.Integration.API/SIMTech.APS.Integration.API.csproj", "SIMTech.APS.Integration.API/"]
RUN dotnet restore "SIMTech.APS.Integration.API/SIMTech.APS.Integration.API.csproj"
COPY . .
WORKDIR "/src/SIMTech.APS.Integration.API"
RUN dotnet build "SIMTech.APS.Integration.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIMTech.APS.Integration.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIMTech.APS.Integration.API.dll"]