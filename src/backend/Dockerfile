FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

# Spec constraint: always generate the solution file via dotnet new sln + dotnet sln add.
RUN rm -f OuterloopLabApi.sln \
  && rm -f OuterloopLabApi/OuterloopLabApi.sln \
  && dotnet new sln -n OuterloopLabApi \
  && if [ ! -f ./OuterloopLabApi.sln ] && [ -f ./OuterloopLabApi/OuterloopLabApi.sln ]; then \
       mv ./OuterloopLabApi/OuterloopLabApi.sln ./OuterloopLabApi.sln; \
     fi \
  && test -f ./OuterloopLabApi.sln \
  && dotnet sln ./OuterloopLabApi.sln add OuterloopLabApi/OuterloopLabApi.csproj \
  && dotnet sln ./OuterloopLabApi.sln add Tests/Tests.csproj \
  && dotnet restore \
  && dotnet publish ./OuterloopLabApi.sln -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 3000
ENTRYPOINT ["dotnet", "OuterloopLabApi.dll"]
