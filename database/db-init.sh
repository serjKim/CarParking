#Wait for sql server starting up
sleep 25s

echo "Running Car Parking script..."

/opt/mssql-tools/bin/sqlcmd -U sa -P '(!)123password' -d master -i cp.sql