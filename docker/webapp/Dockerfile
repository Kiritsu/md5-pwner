FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /src
COPY ["src/Md5Pwner/Md5Pwner.csproj", "src/Md5Pwner/"]
RUN dotnet restore "src/Md5Pwner/Md5Pwner.csproj"
COPY . /app/md5pwner
COPY . .
WORKDIR "/src/src/Md5Pwner"
RUN dotnet publish "Md5Pwner.csproj" -c Release -o /app/publish
WORKDIR /src

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS final
EXPOSE 80
RUN apk upgrade --no-cache && \
    apk add docker-cli docker-compose
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/md5pwner /app/md5pwner
ENTRYPOINT ["dotnet", "Md5Pwner.dll"]