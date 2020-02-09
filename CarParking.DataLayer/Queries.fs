namespace CarParking.DataLayer

open Mapping
open DataContext
open CarParking.Core
open Dto
open Dapper
open CarParking.DataLayer.CmdDefs
open FSharp.Control.Tasks.V2.ContextInsensitive

module Queries =

    let queryParkingById (dctx, token) parkingId =
        let cmd = ParkingCmdDefs.ParkingById(ParkingId.toLong parkingId, token)
        let conn = getConn dctx
        task {
            let! dto = conn.QueryFirstOrDefaultAsync<ParkingDto>(cmd)
            return toParking dto
        }

    let queryAllPacking (dctx, token) =
        let cmd = ParkingCmdDefs.AllParking(token)
        let conn = getConn dctx
        task {
            let! dtos = conn.QueryAsync<ParkingDto>(cmd)
            return dtos 
                |> Seq.map toParking
                |> Seq.choose id (*TODO: log if None*)
                |> Seq.toList
        }