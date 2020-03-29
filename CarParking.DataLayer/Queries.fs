namespace CarParking.DataLayer

[<RequireQualifiedAccess>]
module QueryAllParkingFilter =
    open Dapper
    open CarParking.Core

    type Filter = ByTypes of (Tariff * ParkingStatus) list

    let apply (ByTypes types) sql =
        match types with
        | [] ->
            (new DynamicParameters(), sql)
        | _ ->
            let namedTypes = 
                types
                |> Seq.distinct
                |> Seq.indexed
                |> Seq.map (fun (i, (tariff, status)) -> 
                    {| TariffName = sprintf "Tariff%i" i
                       TariffValue = Tariff.toString tariff
                       StatusName = sprintf "Status%i" i
                       StatusValue = ParkingStatus.toString status |})
                |> Seq.toArray

            let whereClauses = 
                 namedTypes
                 |> Array.map (fun t -> sprintf "(t.Name = @%s and ps.Name = @%s)" t.TariffName t.StatusName)                 
                 |> String.concat " or "

            let sql = sprintf "%s 
                               where %s" sql whereClauses
            
            let parameters = 
                namedTypes
                |> Array.fold (fun (p: DynamicParameters) t ->
                    p.AddParam(t.TariffName, t.TariffValue)
                     .AddParam(t.StatusName, t.StatusValue)) (new DynamicParameters())

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

        let queryAllPacking types token =
            let (parameters, queryText) = QueryAllParkingFilter.apply types parkingQuerySql
            new CommandDefinition(queryText, parameters, cancellationToken = token)

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