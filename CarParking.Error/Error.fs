namespace CarParking.Error

type TransitionError = 
    | FreeExpired
    | AlreadyCompleted
    | PaymentNotApplicable

type CarParkingError =
    | EntityNotFound of string
    | BadInput of string
    | TransitionError of TransitionError
