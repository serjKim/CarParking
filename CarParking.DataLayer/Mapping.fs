namespace CarParking.DataLayer

module internal Mapping =
    open CarParking.Error
    open CarParking.Core
    open CarParking.DataLayer.Dto
    open CarParking.Utils
    open FsToolkit.ErrorHandling

    let toPayment (dto: PaymentDto) =
        if (isNull (box dto)) then
            None
        else
            Some {
                Id = PaymentId dto.PaymentId
                CreateDate = dto.CreateDate }

    let toPaymentDto (payment: Payment) =
        { PaymentId = PaymentId.toGuid payment.Id
          CreateDate = payment.CreateDate }

    let toParking dto =
        if (isNull (box dto)) then
            Error <| BadInput "Dto instance is null"
        else
            result {
                let! status = ParkingStatus.parse dto.Status
                let completeDate = dto.CompleteDate.ToOption()
                let payment = toPayment dto.Payment
                let! tariff = Tariff.parse dto.Tariff
            
                match (status, completeDate, payment, tariff) with
                | Started, None, None, Free ->
                    return StartedFree {
                        Id = ParkingId dto.Id
                        ArrivalDate = dto.ArrivalDate }
                | Completed, Some cdate, None, Free ->
                    let! interval = ParkingInterval.create (dto.ArrivalDate, cdate)
                    return CompletedFree {
                        Id = ParkingId dto.Id
                        Interval = interval }
                | Completed, Some cdate, Some p, First ->
                    let! interval = ParkingInterval.create (dto.ArrivalDate, cdate)
                    return CompletedFirst {
                        Id = ParkingId dto.Id
                        Interval = interval
                        Payment = p }
                | _,_,_,_ ->
                    return! Error <| BadInput "Invalid dto"
            } |> Result.mapError (function
                | BadInput inputError ->
                    BadInput <| sprintf "Invalid ParkingDto (Id = %A): %s" dto.Id inputError
                | _ as other -> other)

    let toStartedFreeParkingDto (prk: StartedFreeParking) =
        { Id          = ParkingId.toGuid prk.Id 
          ArrivalDate = prk.ArrivalDate }

    let toCompletedFreeParkingDto (prk: CompletedFreeParking) =
        { Id           = ParkingId.toGuid prk.Id
          CompleteDate = ParkingInterval.getCompleteDate prk.Interval }

    let toCompletedFirstParkingDto (prk: CompletedFirstParking) =
        { Id           = ParkingId.toGuid prk.Id
          CompleteDate = ParkingInterval.getCompleteDate prk.Interval
          Payment      = toPaymentDto prk.Payment }

    let toTransition dto : Transition option =
        let fromTariff = dto.FromTariff |> Tariff.parse |> Result.toOption
        let fromStatus = dto.FromStatus |> ParkingStatus.parse |> Result.toOption
        let toTariff = Tariff.parse dto.ToTariff
        let toStatus = ParkingStatus.parse dto.ToStatus
        match toTariff,toStatus with
        | Ok t, Ok s -> 
            Some { Name = dto.Name
                   FromTariff = fromTariff
                   FromStatus = fromStatus
                   ToTariff = t
                   ToStatus = s }
        | Error _, Error _ -> None
        | Error _, Ok _ -> None
        | Ok _, Error _ -> None
