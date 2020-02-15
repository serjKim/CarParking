namespace CarParking.DataLayer

open System

module Commands =
    open Mapping
    open DataContext
    open CarParking.DataLayer.CmdDefs
    open FSharp.Control.Tasks.V2
    open Dapper

    let insertFreeParking (dctx, token) parking =
        let conn = getConn dctx
        let dto = toStartedFreeParkingDto parking
        let cmd = ParkingCmdDefs.InsertStartedFreeParking(dto, token)
        task {
            let! rows = conn.ExecuteAsync(cmd)
            return (rows > 0)
        }

    let updateFreeParking (dctx, token) freePrk =
        let conn = getConn dctx
        let dto = toFreeParkingDto freePrk
        let cmd = ParkingCmdDefs.UpdateFreeParking(dto, token)
        task {
            let! rows = conn.ExecuteAsync(cmd)
            return (rows > 0)            
        }

    let updateFirstParking (dctx, token) parking =
        let conn = getConn dctx
        let dto = toFirstParkingDto parking
        task {
            try
                conn.Open()
                use tran = conn.BeginTransaction()
                try
                    let paymentCmd = PaymentCmdDefs.InsertPayment(dto.Payment, tran, token)
                    let! _ = conn.ExecuteAsync(paymentCmd)

                    let updateCmd = ParkingCmdDefs.UpdateFirstParking(dto, tran, token)
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