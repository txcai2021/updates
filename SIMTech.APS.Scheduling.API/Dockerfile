#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80 443 5000

ENV ASPNETCORE_URLS=http://+:80;http://+:5000;
ENV RPS_DB_DPS="Data source= host.docker.internal\\SQLEXPRESS01,1433;Initial Catalog=SIMTech.APS.DPS;User Id=sa;Password=HMLVsa201301;"
ENV RPS_UI_URL=http://localhost:4200
ENV RPS_WORKORDER_URL=http://host.docker.internal:5118/api/workorder/
ENV RPS_SETTING_URL=http://host.docker.internal:5111/api/option/
ENV RPS_CODE_URL=http://host.docker.internal:5111/api/code/
ENV RPS_RESOURCE_URL=http://host.docker.internal:5113/api/resource/
ENV RPS_PRODUCT_URL=http://host.docker.internal:5110/api/item/
ENV RPS_PRODUCTBOM_URL=http://host.docker.internal:5110/api/bom/
ENV RPS_SALESORDER_URL=http://host.docker.internal:5112/api/salesorder/
ENV RPS_CUSTOMER_URL=http://host.docker.internal:5108/api/customer/
ENV RPS_LOCATION_URL=http://host.docker.internal:5117/api/location/
ENV RPS_ROUTE_URL=http://host.docker.internal:5116/api/route/      
ENV RPS_INVENTORY_URL=http://host.docker.internal:5120/api/inventory/
ENV RPS_SCHEDULE_URL=http://host.docker.internal:5122/api/schedule/
ENV RPS_OPERATION_URL=http://host.docker.internal:5115/api/operation/
ENV RPS_CALENDAR_URL=http://host.docker.internal:5114/api/calendar/
ENV RPS_INTEGRATION_URL=http://host.docker.internal:5119/api/integration/
ENV RPS_GAP_URL=http://host.docker.internal:5125/api/cpps24/
ENV RPS_TS_URL = http://host.docker.internal/rtdgenii/api/notification/
ENV ASPNETCORE_ENVIRONMENT=Development
ENV RABBITMQ_HOST=host.docker.internal
ENV RABBITMQ_PORT=31672 
ENV RABBITMQ_USERNAME=guest
ENV RABBITMQ_PASSWORD=guest
#ENV RABBITMQ_ENABLE=100
#ENV RABBITMQ_VHOST=/
ENV RABBITMQ_EXCHANGE=cpps-rps
ENV RABBITMQ_QUEUES=rps-schedule1,rps-schedule2

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SIMTech.APS.Scheduling.API/SIMTech.APS.Scheduling.API.csproj", "SIMTech.APS.Scheduling.API/"]
RUN dotnet restore "SIMTech.APS.Scheduling.API/SIMTech.APS.Scheduling.API.csproj"
COPY . .
WORKDIR "/src/SIMTech.APS.Scheduling.API"
RUN dotnet build "SIMTech.APS.Scheduling.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIMTech.APS.Scheduling.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIMTech.APS.Scheduling.API.dll"]