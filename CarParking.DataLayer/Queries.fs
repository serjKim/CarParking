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

    let queryParkingById (dctx, token) parkingId =
        let struct (cmd, mapping, splitOn) = ParkingCmdDefs.ParkingById(ParkingId.toGuid parkingId, token)
        let conn = getConn dctx
        task {
            let! dto = conn.QueryAsync<ParkingDto, PaymentDto, ParkingDto> (cmd, mapping, splitOn)
            return dto.FirstOrDefault() |> toParking
        }

    let queryAllPacking (dctx, token) =
        let struct (cmd, mapping, splitOn) = ParkingCmdDefs.AllParking(token)
        let conn = getConn dctx
        task {
            let! dtos = conn.QueryAsync<ParkingDto, PaymentDto, ParkingDto>(cmd, mapping, splitOn)
            return dtos 
                |> Seq.map toParking
                |> Seq.choose id (*TODO: log if None*)
                |> Seq.toList
        }