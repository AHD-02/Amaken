FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Amaken/Amaken.csproj", "Amaken/"]

RUN dotnet restore "Amaken/Amaken.csproj"
COPY . .
WORKDIR "/src/SW.Talmaro.Web"
RUN dotnet build "Amaken.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Amaken.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "Amaken.dll"]
