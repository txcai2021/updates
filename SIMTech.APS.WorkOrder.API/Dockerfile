FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80 443 5000

ENV ASPNETCORE_URLS=http://+:80;http://+:5000
ENV RPS_DB_WORKORDER="Data source= host.docker.internal\\SQLEXPRESS01,1433;Initial Catalog=SIMTech.APS.WorkOrder;User Id=sa;Password=HMLVsa201301;"
ENV RPS_UI_URL=http://localhost:4200
ENV RPS_SETTING_URL=http://host.docker.internal:5111/api/option/
ENV RPS_PRODUCT_URL=http://host.docker.internal:5110/api/item/
ENV RPS_PRODUCTBOM_URL=http://host.docker.internal:5110/api/bom/
ENV RPS_SALESORDER_URL=http://host.docker.internal:5112/api/salesorder/
ENV RPS_ROUTE_URL=http://host.docker.internal:5116/api/route/
ENV RPS_CUSTOMER_URL=http://host.docker.internal:5108/api/customer/
ENV RPS_LOCATION_URL=http://host.docker.internal:5117/api/location/
ENV RPS_INTEGRATION_URL=http://host.docker.internal:5119/api/integration/
ENV RPS_INVENTORY_URL=http://host.docker.internal:5120/api/inventory/
ENV RABBITMQ_HOST=host.docker.internal
ENV RABBITMQ_PORT=31672 
ENV RABBITMQ_USERNAME=guest
ENV RABBITMQ_PASSWORD=guest
#ENV RABBITMQ_ENABLE=100
#ENV RABBITMQ_VHOST=/
ENV RABBITMQ_EXCHANGE=cpps-rps
ENV RABBITMQ_QUEUES_SALESORDER=rps-salesorder-status
ENV RABBITMQ_QUEUES_INVENTORY=rps-material-shortage

ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SIMTech.APS.WorkOrder.API/SIMTech.APS.WorkOrder.API.csproj", "SIMTech.APS.WorkOrder.API/"]
RUN dotnet restore "SIMTech.APS.WorkOrder.API/SIMTech.APS.WorkOrder.API.csproj"
COPY . .
WORKDIR "/src/SIMTech.APS.WorkOrder.API"
RUN dotnet build "SIMTech.APS.WorkOrder.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIMTech.APS.WorkOrder.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIMTech.APS.WorkOrder.API.dll"]