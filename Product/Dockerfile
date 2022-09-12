#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80 443 5000

ENV ASPNETCORE_URLS=http://+:80;http://+:5000
ENV RPS_DB_Product="Data source= host.docker.internal\\SMOM,1433;Initial Catalog=SIMTech.APS.Product;User Id=sa;Password=HMLVsa201301;"
ENV RPS_UI_URL=http://localhost:4200
ENV RPS_CUSTOMER_URL=http://host.docker.internal:5108/api/customer/
ENV RPS_PICTURE_URL=http://host.docker.internal:5109/api/picture/
ENV RPS_ROUTE_URL=http://host.docker.internal:5116/api/route/
ENV RPS_SETTING_URL=http://host.docker.internal:5116/api/option/
ENV RPS_INVENTORY_URL=http://host.docker.internal:5120/api/inventory/
ENV RPS_WORKORDER_URL=http://host.docker.internal:5118/api/workorder/
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SIMTech.APS.Product.API/SIMTech.APS.Product.API.csproj", "SIMTech.APS.Product.API/"]
RUN dotnet restore "SIMTech.APS.Product.API/SIMTech.APS.Product.API.csproj"
COPY . .
WORKDIR "/src/SIMTech.APS.Product.API"
RUN dotnet build "SIMTech.APS.Product.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIMTech.APS.Product.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIMTech.APS.Product.API.dll"]