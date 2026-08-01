FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

# Spec constraint: always generate the solution file via dotnet new sln + dotnet sln add.
RUN rm -f OuterloopLabApi.sln \
  && dotnet new sln -n OuterloopLabApi -o . \
  && dotnet sln ./OuterloopLabApi.sln add OuterloopLabApi/OuterloopLabApi.csproj \
  && dotnet restore \
  && dotnet publish ./OuterloopLabApi.sln -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 3000
ENTRYPOINT ["dotnet", "OuterloopLabApi.dll"]
