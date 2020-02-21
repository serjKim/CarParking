FROM mcr.microsoft.com/mssql/server:2017-latest

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=(!)123password

COPY . /

RUN chmod +x /db-init.sh
CMD /bin/bash ./entrypoint.sh
