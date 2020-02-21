namespace CarParking.DataLayer

open System
open Mapping
open DataContext
open CarParking.DataLayer.CmdDefs
open FSharp.Control.Tasks.V2
open Dapper

module Commands =

    let insertStartedFree (cpdc, token) parking =
        let conn = getConn cpdc
        let dto = toStartedFreeParkingDto parking
        let cmd = ParkingCmdDefs.InsertStartedFree(dto, token)
        task {
            let! rows = conn.ExecuteAsync(cmd)
            return (rows > 0)
        }

    let transitionToCompletedFree (cpdc, token) freePrk =
        let conn = getConn cpdc
        let dto = toFreeParkingDto freePrk
        let cmd = ParkingCmdDefs.TransitionToCompletedFree(dto, token)
        task {
            let! rows = conn.ExecuteAsync(cmd)
            return (rows > 0)            
        }

    let transitionToCompletedFirst (cpdc, token) parking =
        let conn = getConn cpdc
        let dto = toFirstParkingDto parking
        task {
            try
                conn.Open()
                use tran = conn.BeginTransaction()
                try
                    let paymentCmd = PaymentCmdDefs.InsertPayment(dto.Payment, tran, token)
                    let! _ = conn.ExecuteAsync(paymentCmd)

                    let updateCmd = ParkingCmdDefs.TransitionToCompletedFirst(dto, tran, token)
                    let! rows = conn.ExecuteAsync(updateCmd)

                    tran.Commit()

                    return (rows > 0)
                with 
                | _ as ex -> 
                    tran.Rollback()
                    raise(new Exception(ex.Message, ex))
                    return false
            finally
                conn.Close()
        }