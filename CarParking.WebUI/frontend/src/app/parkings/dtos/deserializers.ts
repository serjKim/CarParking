import { deserialize } from 'santee-dcts';
import { errorUnhandledType } from '../../extensions';
import { CompletedFirst, CompletedFree, Parking, ParkingType, Payment, StartedFree } from '../models';
import { CompletedFirstDto, CompletedFreeDto, ParkingDto, StartedFreeDto } from './parking-dto';

export const deserializeParking = (obj: object | null): Parking => {
    if (!obj) {
        throw new Error('Parking is required');
    }

    const raw = obj as ParkingDto;

    // exhaustive checking
    switch (raw.type) {
        case ParkingType.StartedFree: {
            const dto = deserialize(raw, StartedFreeDto);
            return new StartedFree(dto.id, dto.arrivalDate);
        }
        case ParkingType.CompletedFree: {
            const dto = deserialize(raw, CompletedFreeDto);
            return new CompletedFree(dto.id, dto.arrivalDate, dto.completeDate);
        }
        case ParkingType.CompletedFirst: {
            const dto = deserialize(raw, CompletedFirstDto);
            const payment = new Payment(dto.payment.id, dto.payment.createDate);
            return new CompletedFirst(dto.id, dto.arrivalDate, dto.completeDate, payment);
        }
        default:
            throw errorUnhandledType(raw);
    }
};

export const deserializeParkings = (array: object[] | null): Parking[] => {
    if (!Array.isArray(array)) {
        throw new Error('Expected parkings array');
    }

    return array.map(deserializeParking);
};
