namespace CarParking.DataLayer

module Commands =
    [<RequireQualifiedAccess>]
    module private CommandDefinitions =
        open CarParking.Utils.NameOf
        open Dto
        open System.Data
        open Dapper

        let insertStartedFree (dto: StartedFreeParkingDto) token = 
            let idName = nameOf <@ p<StartedFreeParkingDto>.Id @>
            let arrivalDateName = nameOf <@ p<StartedFreeParkingDto>.ArrivalDate @>
            let queryText = "
                    insert into dbo.Parking (
                        ParkingID,
                        StatusID,
                        ArrivalDate,
                        TariffID)
                    select
                        @" + idName + ",
                        ToStatus,
                        @" + arrivalDateName + ",
                        ToTariff
                    from dbo.Transition
                    where Name = '" + DbConstants.Transitions.StartedFree + "'"
            let parameters = 
                DynamicParameters()
                    .AddParam(idName, dto.Id)
                    .AddParam(arrivalDateName, dto.ArrivalDate, DbType.DateTime2);
            new CommandDefinition (queryText, parameters, cancellationToken = token)

        let saveCompletedFree (dto: CompletedFreeParkingDto) token = 
            let completeDateName = nameOf <@ p<CompletedFreeParkingDto>.CompleteDate @>
            let idName = nameOf <@ p<CompletedFreeParkingDto>.Id @>
            let queryText = "
                    update p
                        set p.StatusID = t.ToStatus,
                            p.TariffID = t.ToTariff,
                            p.CompleteDate = @" + completeDateName + "
                    from dbo.Parking p
                    inner join dbo.Transition t
                        on t.Name = '" + DbConstants.Transitions.CompletedFree + "'
                    where p.ParkingID = @" + idName

            let parameters = 
                DynamicParameters()
                    .AddParam(idName, dto.Id)
                    .AddParam(completeDateName, dto.CompleteDate, DbType.DateTime2)
            new CommandDefinition(queryText, parameters, cancellationToken = token)

        let saveCompletedFirst (dto: CompletedFirstParkingDto) (tran: IDbTransaction) token =
            let completeDateName = nameOf <@ p<CompletedFirstParkingDto>.CompleteDate @>
            let paymentIdName = nameOf <@ p<CompletedFirstParkingDto>.PaymentId @>
            let idName = nameOf <@ p<CompletedFirstParkingDto>.Id @>
            let queryText = "
                    update p
                        set p.StatusID = t.ToStatus,
                            p.TariffID = t.ToTariff,
                            p.CompleteDate = @" + completeDateName + ",
                            p.PaymentID = @" + paymentIdName + "
                    from dbo.Parking p
                    inner join dbo.Transition t
                        on t.Name = '" + DbConstants.Transitions.CompletedFirst + "'
                    where ParkingID = @" + idName
            let parameters = 
                DynamicParameters()
                    .AddParam(idName, dto.Id)
                    .AddParam(completeDateName, dto.CompleteDate, DbType.DateTime2)
                    .AddParam(paymentIdName, dto.PaymentId)
            new CommandDefinition(queryText, parameters, transaction = tran, cancellationToken = token);

        let insertPayment (dto: PaymentDto) (tran: IDbTransaction) token  =
            let idName = nameOf <@ p<PaymentDto>.PaymentId @>
            let createDateName = nameOf <@ p<PaymentDto>.CreateDate @>
            let queryText = "
                    insert into dbo.Payment(
                        PaymentID,
                        CreateDate)
                    values(
                        @" + idName + ",
                        @" + createDateName + ")"
            let parameters =
                DynamicParameters()
                    .AddParam(idName, dto.PaymentId)
                    .AddParam(createDateName, dto.CreateDate, DbType.DateTime2)
            new CommandDefinition(queryText, parameters, cancellationToken = token, transaction = tran)

    open System
    open Mapping
    open DataContext
    open FSharp.Control.Tasks.V2
    open Dapper

    let insertStartedFree (cpdc, token) parking =
        let conn = getConn cpdc
        let dto = toStartedFreeParkingDto parking
        let cmd = CommandDefinitions.insertStartedFree dto token
        task {
            let! rows = conn.ExecuteAsync(cmd)
            return (rows > 0)
        }

    let saveCompletedFree (cpdc, token) completedFree =
        let conn = getConn cpdc
        let dto = toCompletedFreeParkingDto completedFree
        let cmd = CommandDefinitions.saveCompletedFree dto token
        task {
            let! rows = conn.ExecuteAsync(cmd)
            return (rows > 0)            
        }

    let saveCompletedFirst (cpdc, token) completedFirst =
        let dto = toCompletedFirstParkingDto completedFirst
        task {
            use conn = getConn cpdc
            conn.Open()
            use tran = conn.BeginTransaction()
            try
                let paymentCmd = CommandDefinitions.insertPayment dto.Payment tran token
                let! _ = conn.ExecuteAsync(paymentCmd)

                let updateCmd = CommandDefinitions.saveCompletedFirst dto tran token
                let! rows = conn.ExecuteAsync(updateCmd)

                tran.Commit()

                return (rows > 0)
            with 
            | _ as ex -> 
                tran.Rollback()
                raise(new Exception(ex.Message, ex))
                return false
        }