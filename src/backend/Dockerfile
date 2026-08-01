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
  && if [ ! -f ./OuterloopLabApi.sln ] && [ -f ./OuterloopLabApi.slnx ]; then \
       cp ./OuterloopLabApi.slnx ./OuterloopLabApi.sln; \
     fi \
  && (test -f ./OuterloopLabApi.sln || test -f ./OuterloopLabApi.slnx) \
  && SOL_FILE="$(if [ -f ./OuterloopLabApi.sln ]; then echo ./OuterloopLabApi.sln; else echo ./OuterloopLabApi.slnx; fi)" \
  && dotnet sln "$SOL_FILE" add OuterloopLabApi/OuterloopLabApi.csproj \
  && dotnet sln "$SOL_FILE" add Tests/Tests.csproj \
  && dotnet restore "$SOL_FILE" \
  && if [ -f ./OuterloopLabApi.sln ]; then \
       dotnet publish ./OuterloopLabApi.sln -c Release -o /app/publish; \
     else \
       dotnet publish ./OuterloopLabApi.slnx -c Release -o /app/publish; \
     fi

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 3000
ENTRYPOINT ["dotnet", "OuterloopLabApi.dll"]
