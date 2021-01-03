namespace CarParking.DataLayer

module Commands =
    [<RequireQualifiedAccess>]
    module private CommandDefinitions =
        open CarParking.Utils.NameOf
        open Dto
        open System.Data
        open Dapper

        let insertStartedFree (dto: StartedFreeParkingDto) token = 
            let queryText = $"
                insert into dbo.Parking (
                    ParkingID,
                    StatusID,
                    ArrivalDate,
                    TariffID)
                select
                    @{nameof prop<StartedFreeParkingDto>.Id},
                    ToStatus,
                    @{nameof prop<StartedFreeParkingDto>.ArrivalDate},
                    ToTariff
                from dbo.Transition
                where Name = '{DbConstants.Transitions.StartedFree}'"
            new CommandDefinition (queryText, dto, cancellationToken = token)

        let saveCompletedFree (dto: CompletedFreeParkingDto) token = 
            let queryText = $"
                update p
                    set p.StatusID = t.ToStatus,
                        p.TariffID = t.ToTariff,
                        p.CompleteDate = @{nameof prop<CompletedFreeParkingDto>.CompleteDate}
                from dbo.Parking p
                inner join dbo.Transition t
                    on t.Name = '{DbConstants.Transitions.CompletedFree}'
                where p.ParkingID = @{nameof prop<CompletedFreeParkingDto>.Id}"
            new CommandDefinition(queryText, dto, cancellationToken = token)

        let saveCompletedFirst (dto: CompletedFirstParkingDto) (tran: IDbTransaction) token =
            let queryText = $"
                    update p
                        set p.StatusID = t.ToStatus,
                            p.TariffID = t.ToTariff,
                            p.CompleteDate = @{nameof prop<CompletedFirstParkingDto>.CompleteDate},
                            p.PaymentID = @{nameof prop<CompletedFirstParkingDto>.PaymentId}
                    from dbo.Parking p
                    inner join dbo.Transition t
                        on t.Name = '{DbConstants.Transitions.CompletedFirst}'
                    where ParkingID = @{nameof prop<CompletedFirstParkingDto>.Id}"
            new CommandDefinition(queryText, dto, transaction = tran, cancellationToken = token);

        let insertPayment (dto: PaymentDto) (tran: IDbTransaction) token  =
            let queryText = $"
                    insert into dbo.Payment(
                        PaymentID,
                        CreateDate)
                    values(
                        @{nameof prop<PaymentDto>.PaymentId},
                        @{nameof prop<PaymentDto>.CreateDate})"
            new CommandDefinition(queryText, dto, cancellationToken = token, transaction = tran)

    open System
    open Mapping
    open DataContext
    open FSharp.Control.Tasks.V2.ContextInsensitive
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