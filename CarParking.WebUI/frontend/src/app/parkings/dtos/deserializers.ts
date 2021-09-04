import { deserialize } from 'santee-dcts';
import { CompletedFirst, CompletedFree, Parking, ParkingType, Payment, StartedFree } from '../models';
import { CompletedFirstDto, CompletedFreeDto, StartedFreeDto } from './parking-dto';

type DeserializerByType = {
    [prkType in ParkingType]: ((obj: object) => Parking) | undefined
};

const deserializerByType: DeserializerByType = {
    [ParkingType.StartedFree]: raw => {
        const dto = deserialize(raw, StartedFreeDto);
        return new StartedFree(dto.id, dto.arrivalDate);
    },
    [ParkingType.CompletedFree]: raw => {
        const dto = deserialize(raw, CompletedFreeDto);
        return new CompletedFree(dto.id, dto.arrivalDate, dto.completeDate);
    },
    [ParkingType.CompletedFirst]: raw => {
        const dto = deserialize(raw, CompletedFirstDto);
        const payment = new Payment(dto.payment.id, dto.payment.createDate);
        return new CompletedFirst(dto.id, dto.arrivalDate, dto.completeDate, payment);
    },
};

export interface ParkingRaw {
    readonly type: ParkingType | null | undefined;
}

export const deserializeParking = (raw: ParkingRaw | null): Parking => {
    if (!raw?.type) {
        throw new Error('Unexpected parking raw.');
    }
    const deserializer = deserializerByType[raw.type];
    if (!deserializer) {
        throw new Error(`Deserializer not found by ${raw.type}.`);
    }
    return deserializer(raw);
};

export const deserializeParkings = (array: ParkingRaw[] | null): Parking[] => {
    if (!Array.isArray(array)) {
        throw new Error('Expected an array.');
    }
    return array.map(deserializeParking);
};
