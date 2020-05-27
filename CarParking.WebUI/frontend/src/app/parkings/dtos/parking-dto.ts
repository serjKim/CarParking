import { dataMember, required } from 'santee-dcts';
import { ParkingType } from '../models/parking';

export const deserializeDate = (str: string) => {
    const date = new Date(str);

    if (isNaN(date.valueOf())) {
        throw new Error('Invalid date');
    }

    return date;
};

export class PaymentDto {
    @dataMember()
    @required()
    public id: string = '';

    @dataMember({ customDeserializer: deserializeDate })
    @required()
    public createDate: Date = new Date();
}

export class StartedFreeDto {
    public readonly type = ParkingType.StartedFree;

    @dataMember()
    @required()
    public id: string = '';

    @dataMember({ customDeserializer: deserializeDate })
    @required()
    public arrivalDate: Date = new Date();
}

export class CompletedFreeDto {
    public readonly type = ParkingType.CompletedFree;

    @dataMember()
    @required()
    public id: string = '';

    @dataMember({ customDeserializer: deserializeDate })
    @required()
    public arrivalDate: Date = new Date();

    @dataMember({ customDeserializer: deserializeDate })
    @required()
    public completeDate: Date = new Date();
}

export class CompletedFirstDto {
    public readonly type = ParkingType.CompletedFirst;

    @dataMember()
    @required()
    public id: string = '';

    @dataMember({ customDeserializer: deserializeDate })
    @required()
    public arrivalDate: Date = new Date();

    @dataMember({ customDeserializer: deserializeDate })
    @required()
    public completeDate: Date = new Date();

    @dataMember()
    @required()
    public payment: PaymentDto = new PaymentDto();
}

export type ParkingDto =
    | StartedFreeDto
    | CompletedFreeDto
    | CompletedFirstDto
    ;
