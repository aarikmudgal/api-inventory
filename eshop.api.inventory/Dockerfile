FROM microsoft/dotnet:2.1-sdk
RUN mkdir /app && cd /app
WORKDIR /app
COPY *.csproj /app
RUN dotnet restore
COPY . /app
CMD [ "dotnet", "run" ]
EXPOSE 8001
