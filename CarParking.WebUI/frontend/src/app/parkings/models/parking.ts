import { dataMember, deserialize, required } from 'santee-dcts';
import { assertUnhandledType } from '../../util';

const dateDeserializer = (str: string) => {
    const date = new Date(str);

    if (isNaN(date.valueOf())) {
        throw new Error('Invalid date');
    }

    return date;
};

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
    @dataMember()
    @required()
    public id: string;

    @dataMember({ customDeserializer: dateDeserializer })
    @required()
    public createDate: Date;
}

export class StartedFree {
    public readonly type = ParkingType.StartedFree;

    @dataMember()
    @required()
    public id: string;

    @dataMember({ customDeserializer: dateDeserializer })
    @required()
    public arrivalDate: Date;
}

export class CompletedFree {
    public readonly type = ParkingType.CompletedFree;

    @dataMember()
    @required()
    public id: string;

    @dataMember({ customDeserializer: dateDeserializer })
    @required()
    public arrivalDate: Date;

    @dataMember({ customDeserializer: dateDeserializer })
    @required()
    public completeDate: Date;
}

export class CompletedFirst {
    public readonly type = ParkingType.CompletedFirst;

    @dataMember()
    @required()
    public id: string;

    @dataMember({ customDeserializer: dateDeserializer })
    @required()
    public arrivalDate: Date;

    @dataMember({ customDeserializer: dateDeserializer })
    @required()
    public completeDate: Date;

    @dataMember()
    @required()
    public payment: Payment;
}

export type Parking =
    | StartedFree
    | CompletedFree
    | CompletedFirst
    ;

const parkingDeserializer = (raw: Parking) => {
    if (!raw) {
        throw new Error('Parking is required');
    }

    // exhaustive checking
    switch (raw.type) {
        case ParkingType.StartedFree:
            return deserialize(raw, StartedFree);
        case ParkingType.CompletedFree:
            return deserialize(raw, CompletedFree);
        case ParkingType.CompletedFirst:
            return deserialize(raw, CompletedFirst);
        default:
            assertUnhandledType(raw);
            return null;
    }
};

export class ParkingReadModel {
    @dataMember({ customDeserializer: parkingDeserializer })
    @required()
    public parking: Parking;
}

export class ParkingsReadModel {
    @dataMember({ customDeserializer: (array: Parking[]) => {
        if (!Array.isArray(array)) {
            throw new Error('Expected parkings array');
        }

        return array.map(parkingDeserializer);
    }})
    @required()
    public parkings: Parking[];
}
