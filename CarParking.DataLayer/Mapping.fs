namespace CarParking.DataLayer

module internal Mapping =
    open CarParking.Core
    open CarParking.DataLayer.Dto
    open CarParking.Utils

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

    let toParking dto : Parking option =
        if (isNull (box dto)) then
            None
        else
            let status = ParkingStatus.parse dto.Status
            let completeDate = dto.CompleteDate.ToOption()
            let payment = toPayment dto.Payment
            let tariff = Tariff.parse dto.Tariff
            
            match (status, completeDate, payment, tariff) with
            | Ok Started, None, None, Ok Free ->
                StartedFreeParking {
                    Id = ParkingId dto.Id
                    ArrivalDate = dto.ArrivalDate }|> Some
            | Ok Completed, Some cdate, None, Ok Free ->
                CompletedFreeParking {
                    Id = ParkingId dto.Id
                    ArrivalDate = dto.ArrivalDate
                    CompleteDate = cdate } |> Some
            | Ok Completed, Some cdate, Some p, Ok First ->
                CompletedFirstParking {
                    Id = ParkingId dto.Id
                    ArrivalDate = dto.ArrivalDate
                    CompleteDate = cdate
                    Payment = p } |> Some
            | _,_,_,_ -> None

    let toStartedFreeParkingDto (prk: StartedFreeParking) =
        { Id = ParkingId.toGuid prk.Id 
          ArrivalDate = prk.ArrivalDate }

    let toFreeParkingDto (prk: CompletedFreeParking) =
        { Id           = ParkingId.toGuid prk.Id
          CompleteDate = prk.CompleteDate }

    let toFirstParkingDto (prk: CompletedFirstParking) =
        { Id           = ParkingId.toGuid prk.Id
          CompleteDate = prk.CompleteDate
          Payment      = toPaymentDto prk.Payment }