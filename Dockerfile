#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80 443 5000

ENV ASPNETCORE_URLS=http://+:80;http://+:5000
ENV RPS_DB_OPERATION="Data source= host.docker.internal\\SQLEXPRESS01,1433;Initial Catalog=SIMTech.APS.Operation;User Id=sa;Password=HMLVsa201301;"
ENV RPS_UI_URL=http://localhost:4200
ENV RPS_RESOURCE_URL=http://host.docker.internal:5113/api/resource/
ENV RPS_ROUTE_URL=http://host.docker.internal:5116/api/route/
ENV RPS_SETTING_CODE_URL=http://host.docker.internal:5111/api/code/
ENV RPS_USER_URL=http://host.docker.internal:5106/api/user/
ENV RPS_SETTING_URL=http://host.docker.internal:5111/api/option/
ENV RPS_CUSTOMER_URL=http://host.docker.internal:5108/api/customer/
ENV RPS_PRODUCT_URL=http://host.docker.internal:5110/api/item/
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SIMTech.APS.Operation.API/SIMTech.APS.Operation.API.csproj", "SIMTech.APS.Operation.API/"]
RUN dotnet restore "SIMTech.APS.Operation.API/SIMTech.APS.Operation.API.csproj"
COPY . .
WORKDIR "/src/SIMTech.APS.Operation.API"
RUN dotnet build "SIMTech.APS.Operation.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIMTech.APS.Operation.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIMTech.APS.Operation.API.dll"]