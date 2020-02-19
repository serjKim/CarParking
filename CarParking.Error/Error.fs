namespace CarParking.Error

type CarParkingError =
    | EntityNotFound of string
    | BadInput of string
    | TransitionError of string
