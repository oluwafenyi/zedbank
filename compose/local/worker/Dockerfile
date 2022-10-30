FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /usr/app/src

COPY ["zedbank/", "zedbank/"]
COPY ["zedbankInterestWorker/", "zedbankInterestWorker/"]

WORKDIR /usr/app/src/zedbank/

RUN dotnet restore "zedbank.csproj"

WORKDIR /usr/app/src/zedbankInterestWorker/

RUN dotnet restore "zedbankInterestWorker.csproj"

CMD ["dotnet", "run"]