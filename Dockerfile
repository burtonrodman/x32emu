FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 10023/udp
EXPOSE 10111

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/X32Emulator/X32Emulator.csproj", "src/X32Emulator/"]
RUN dotnet restore "src/X32Emulator/X32Emulator.csproj"
COPY . .
WORKDIR "/src/src/X32Emulator"
RUN dotnet build "X32Emulator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "X32Emulator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME /data/scenes
VOLUME /data/audio
ENTRYPOINT ["dotnet", "X32Emulator.dll"]
