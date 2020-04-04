namespace CarParking.DataLayer

[<RequireQualifiedAccess>]
module QueryAllParkingFilter =
    open Dapper
    open CarParking.Utils.NameOf
    open CarParking.Core

    type Filter = ByTransitionNames of TransitionName list

    let apply (ByTransitionNames transitionNames) sql =
        match transitionNames with
        | [] ->
            (new DynamicParameters(), sql)
        | _ ->
            let transitionParamName = nameOf <@ transitionNames @>
            let sql = sprintf "%s 
                            inner join dbo.Transition tr
                                on tr.ToTariff = p.TariffID and tr.ToStatus = p.StatusID
                            where tr.Name in @%s" sql transitionParamName
            let parameters = DynamicParameters().AddParam(transitionParamName, List.distinct transitionNames)
            (parameters, sql)

module Queries =
    [<RequireQualifiedAccess>]
    module private CommandDefinitions =
        open CarParking.Core
        open Dapper
        open Dto
        open CarParking.Utils.NameOf

        let private parkingQuerySql =
            "
            select 
                p.ParkingID    [" + nameOf <@ p<ParkingDto>.Id           @> + "],
                p.ArrivalDate  [" + nameOf <@ p<ParkingDto>.ArrivalDate  @> + "],
                ps.Name        [" + nameOf <@ p<ParkingDto>.Status       @> + "],
                p.CompleteDate [" + nameOf <@ p<ParkingDto>.CompleteDate @> + "],
                t.Name         [" + nameOf <@ p<ParkingDto>.Tariff       @> + "],
                pt.PaymentID   [" + nameOf <@ p<PaymentDto>.PaymentId    @> + "],
                pt.CreateDate  [" + nameOf <@ p<PaymentDto>.CreateDate   @> + "]
            from dbo.Parking p 
            inner join dbo.ParkingStatus ps
                on ps.ParkingStatusID = p.StatusID
            left join dbo.Payment pt
                on pt.PaymentID = p.PaymentID
            left join dbo.Tariff t
                on t.TariffID = p.TariffID
            "    
        let queryParkingById parkingId token =
            let queryText = sprintf "%s
                                    where p.ParkingID = @%s" parkingQuerySql (nameOf <@ parkingId @>)
            let parameters = DynamicParameters().AddParam(nameOf <@ parkingId @>, ParkingId.toGuid parkingId)
            new CommandDefinition(queryText, parameters, cancellationToken = token)

        let queryAllPacking filter token =
            let (parameters, queryText) = QueryAllParkingFilter.apply filter parkingQuerySql
            new CommandDefinition(queryText, parameters, cancellationToken = token)

        let queryAllTransitions token =
            let queryText = "
                    select t.[Name]  [" + nameOf <@ p<TransitionDto>.Name       @> + "],
                    	   ft.[Name] [" + nameOf <@ p<TransitionDto>.FromTariff @> + "], 
                    	   fs.[Name] [" + nameOf <@ p<TransitionDto>.FromStatus @> + "],
                    	   tt.[Name] [" + nameOf <@ p<TransitionDto>.ToTariff   @> + "],
                    	   ts.[Name] [" + nameOf <@ p<TransitionDto>.ToStatus   @> + "]
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
            let! dto = conn.QueryAsync<ParkingDto, PaymentDto, ParkingDto> (cmd, parkingMapping, splitOn = nameOf <@ p<PaymentDto>.PaymentId @>)
            return dto.FirstOrDefault() |> toParking
        }

    let queryAllPacking (cpdc, token) types =
        let cmd = CommandDefinitions.queryAllPacking types token
        let conn = getConn cpdc
        task {
            let! dtos = conn.QueryAsync<ParkingDto, PaymentDto, ParkingDto>(cmd, parkingMapping, splitOn = nameOf <@ p<PaymentDto>.PaymentId @>)
            return dtos |> Seq.map toParking
        }

    let queryAllTransitions (cpdc, token) =
        let cmd = CommandDefinitions.queryAllTransitions token
        let conn = getConn cpdc
        task {
            let! dtos = conn.QueryAsync<TransitionDto> (cmd)
            return dtos |> Seq.map toTransition
        }