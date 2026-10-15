# Builds on the host's native platform and cross-compiles for the target (e.g. linux/arm64 for the Pi).
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY RiskyBusiness.Api.csproj ./
RUN dotnet restore -a $TARGETARCH

COPY . ./
RUN dotnet publish -c Release -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "RiskyBusiness.Api.dll"]
