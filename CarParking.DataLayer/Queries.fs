namespace CarParking.DataLayer

open Mapping
open DataContext
open CarParking.Core
open Dto
open Dapper
open CarParking.DataLayer.CmdDefs
open FSharp.Control.Tasks.V2.ContextInsensitive
open System.Linq

module Queries =
    let private parkingMapping (parking: ParkingDto) payment =
        parking.Payment <- payment
        parking

    let queryParkingById (cpdc, token) parkingId =
        let struct (cmd, splitOn) = ParkingCmdDefs.ParkingById(ParkingId.toGuid parkingId, token)
        let conn = getConn cpdc
        task {
            let! dto = conn.QueryAsync<ParkingDto, PaymentDto, ParkingDto> (cmd, parkingMapping, splitOn)
            return dto.FirstOrDefault() |> toParking
        }

    let queryAllPacking (cpdc, token) =
        let struct (cmd, splitOn) = ParkingCmdDefs.AllParking(token)
        let conn = getConn cpdc
        task {
            let! dtos = conn.QueryAsync<ParkingDto, PaymentDto, ParkingDto>(cmd, parkingMapping, splitOn)
            return dtos |> Seq.map toParking
        }