export enum ParkingType {
    StartedFree = 'StartedFree',
    CompletedFree = 'CompletedFree',
    CompletedFirst = 'CompletedFirst',
}

export const enum Tariff {
    Free = 'Free',
    First = 'First',
}

export const enum ParkingStatus {
    Started = 'Started',
    Completed = 'Completed',
}

export class Payment {
    constructor(
        public readonly id: string,
        public readonly createDate: Date,
    ) { }
}

export class StartedFree {
    public readonly type = ParkingType.StartedFree;
    constructor(
        public readonly id: string,
        public readonly arrivalDate: Date,
    ) { }
}

export class CompletedFree {
    public readonly type = ParkingType.CompletedFree;
    constructor(
        public readonly id: string,
        public readonly arrivalDate: Date,
        public readonly completeDate: Date,
    ) { }
}

export class CompletedFirst {
    public readonly type = ParkingType.CompletedFirst;
    constructor(
        public readonly id: string,
        public readonly arrivalDate: Date,
        public readonly completeDate: Date,
        public readonly payment: Payment,
    ) { }
}

export type Parking =
    | StartedFree
    | CompletedFree
    | CompletedFirst
    ;
