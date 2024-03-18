FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SW.Talmaro.Web/SW.Talmaro.Web.csproj", "SW.Talmaro.Web/"]
COPY ["SW.Talmaro.Legacy/SW.Talmaro.Legacy.csproj", "SW.Talmaro.Legacy/"]

RUN dotnet restore "SW.Talmaro.Web/SW.Talmaro.Web.csproj"
COPY . .
WORKDIR "/src/SW.Talmaro.Web"
RUN dotnet build "SW.Talmaro.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SW.Talmaro.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "SW.Talmaro.Web.dll"]
