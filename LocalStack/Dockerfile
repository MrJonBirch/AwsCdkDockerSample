FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/' /etc/ssl/openssl.cnf \
    && sed -i 's/CipherString = DEFAULT@SECLEVEL=2/CipherString = DEFAULT@SECLEVEL=1/' /etc/ssl/openssl.cnf

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

WORKDIR /src
COPY ["LocalStack/LocalStack.csproj", "LocalStack/"]
COPY ["SharedCode/SharedCode.csproj", "SharedCode/"]
RUN dotnet restore "LocalStack/LocalStack.csproj"
COPY . .
WORKDIR "/src/LocalStack"
RUN dotnet build "StudioApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudioApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LocalStack.dll"]