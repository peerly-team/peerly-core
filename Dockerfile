FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY .tool-versions .
RUN BUF_VERSION=$(grep '^buf ' .tool-versions | awk '{print $2}') \
 && curl -fsSL "https://github.com/bufbuild/buf/releases/download/v${BUF_VERSION}/buf-$(uname -s)-$(uname -m)" \
      -o /usr/local/bin/buf \
 && chmod +x /usr/local/bin/buf

COPY Directory.Build.props .
COPY Directory.Build.targets .
COPY Directory.Packages.props .

COPY src/Peerly.Core/Peerly.Core.csproj                                         src/Peerly.Core/
COPY src/Peerly.Core.Api/Peerly.Core.Api.csproj                                 src/Peerly.Core.Api/
COPY src/Peerly.Core.ApplicationServices/Peerly.Core.ApplicationServices.csproj src/Peerly.Core.ApplicationServices/
COPY src/Peerly.Core.FileStorage/Peerly.Core.FileStorage.csproj                 src/Peerly.Core.FileStorage/
COPY src/Peerly.Core.Hosting/Peerly.Core.Hosting.csproj                         src/Peerly.Core.Hosting/
COPY src/Peerly.Core.Persistence/Peerly.Core.Persistence.csproj                 src/Peerly.Core.Persistence/
COPY src/Peerly.Core.Tools/Peerly.Core.Tools.csproj                             src/Peerly.Core.Tools/

RUN dotnet restore src/Peerly.Core.Hosting/Peerly.Core.Hosting.csproj

COPY . .

RUN buf generate

RUN dotnet publish src/Peerly.Core.Hosting/Peerly.Core.Hosting.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Peerly.Core.Hosting.dll"]
