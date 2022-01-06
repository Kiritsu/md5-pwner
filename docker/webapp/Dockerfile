FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS base
RUN apk upgrade --no-cache && \
    apk add docker-cli
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /src
COPY ["src/Md5Pwner/Md5Pwner.csproj", "src/Md5Pwner/"]
RUN dotnet restore "src/Md5Pwner/Md5Pwner.csproj"
COPY . .
WORKDIR "/src/src/Md5Pwner"
RUN dotnet publish "Md5Pwner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Md5Pwner.dll"]