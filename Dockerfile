FROM microsoft/dotnet:2.1-sdk
RUN mkdir /app && cd /app
COPY . /app
WORKDIR /app/eshop.api.inventory
RUN dotnet restore
CMD [ "dotnet", "run" ]
EXPOSE 8001
