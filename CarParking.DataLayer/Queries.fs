namespace CarParking.DataLayer

[<RequireQualifiedAccess>]
module QueryAllParkingFilter =
    open Dapper
    open CarParking.Core

    type Filter = ByTransitionNames of TransitionName list

    let apply (ByTransitionNames transitionNames) sql =
        match transitionNames with
        | [] ->
            (new DynamicParameters(), sql)
        | _ ->
            let sql =  $"{sql}
                         inner join dbo.Transition tr
                            on tr.ToTariff = p.TariffID and tr.ToStatus = p.StatusID
                         where tr.Name in @{nameof transitionNames}"  
            let parameters = DynamicParameters().AddParam(nameof transitionNames, List.distinct transitionNames)
            (parameters, sql)

module Queries =
    [<RequireQualifiedAccess>]
    module private CommandDefinitions =
        open CarParking.Core
        open CarParking.Utils.NameOf
        open Dapper
        open Dto

        let private parkingQuerySql =
            $"
            select 
                p.ParkingID    [{nameof prop<ParkingDto>.Id}],
                p.ArrivalDate  [{nameof prop<ParkingDto>.ArrivalDate}],
                ps.Name        [{nameof prop<ParkingDto>.Status}],
                p.CompleteDate [{nameof prop<ParkingDto>.CompleteDate}],
                t.Name         [{nameof prop<ParkingDto>.Tariff}],
                pt.PaymentID   [{nameof prop<ParkingDto>.Payment.PaymentId}],
                pt.CreateDate  [{nameof prop<ParkingDto>.Payment.CreateDate}]
            from dbo.Parking p 
            inner join dbo.ParkingStatus ps
                on ps.ParkingStatusID = p.StatusID
            left join dbo.Payment pt
                on pt.PaymentID = p.PaymentID
            left join dbo.Tariff t
                on t.TariffID = p.TariffID
            "    
        let queryParkingById parkingId token =
            let queryText = $"{parkingQuerySql}
                              where p.ParkingID = @{nameof parkingId}"
            let parameters = DynamicParameters().AddParam(nameof parkingId, ParkingId.toGuid parkingId)
            new CommandDefinition(queryText, parameters, cancellationToken = token)

        let queryAllPacking filter token =
            let (parameters, queryText) = QueryAllParkingFilter.apply filter parkingQuerySql
            new CommandDefinition(queryText, parameters, cancellationToken = token)

        let queryAllTransitions token =
            let queryText = $"
                    select t.[Name]  [{nameof prop<TransitionDto>.Name}],
                    	   ft.[Name] [{nameof prop<TransitionDto>.FromTariff}], 
                    	   fs.[Name] [{nameof prop<TransitionDto>.FromStatus}],
                    	   tt.[Name] [{nameof prop<TransitionDto>.ToTariff}],
                    	   ts.[Name] [{nameof prop<TransitionDto>.ToStatus}]
                    from dbo.Transition t
                    left join dbo.Tariff ft 
                        on ft.TariffID = t.FromTariff
                    left join dbo.ParkingStatus fs 
                        on fs.ParkingStatusID = t.FromStatus
                    inner join dbo.Tariff tt 
                        on tt.TariffID = t.ToTariff
                    inner join dbo.ParkingStatus ts 
                        on ts.ParkingStatusID = t.ToStatus
                    "
            new CommandDefinition(queryText, cancellationToken = token)

    open Mapping
    open DataContext
    open Dto
    open Dapper
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open System.Linq
    open CarParking.Utils.NameOf

    let private parkingMapping (parking: ParkingDto) payment =
        parking.Payment <- payment
        parking

    let queryParkingById (cpdc, token) parkingId =
        let cmd = CommandDefinitions.queryParkingById parkingId token
        let conn = getConn cpdc
        task {
            let! dto = conn.QueryAsync<ParkingDto, PaymentDto, ParkingDto> (cmd, parkingMapping, splitOn = nameof prop<PaymentDto>.PaymentId)
            return dto.FirstOrDefault() |> toParking
        }

    let queryAllPacking (cpdc, token) types =
        let cmd = CommandDefinitions.queryAllPacking types token
        let conn = getConn cpdc
        task {
            let! dtos = conn.QueryAsync<ParkingDto, PaymentDto, ParkingDto>(cmd, parkingMapping, splitOn = nameof prop<PaymentDto>.PaymentId)
            return dtos |> Seq.map toParking
        }

    let queryAllTransitions (cpdc, token) =
        let cmd = CommandDefinitions.queryAllTransitions token
        let conn = getConn cpdc
        task {
            let! dtos = conn.QueryAsync<TransitionDto> (cmd)
            return dtos |> Seq.map toTransition
        }