FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /usr/app/src

COPY ["zedbank.csproj", "."]

RUN dotnet restore "zedbank.csproj"

COPY . .

EXPOSE 80
EXPOSE 443

CMD ["dotnet", "watch", "run"]