import { dataMember, required } from 'santee-dcts';
import { ParkingStatus, Tariff } from '../models/parking';
import { TransitionName } from '../models/transition';

export class TransitionDto {
    @dataMember()
    @required()
    public name: TransitionName;

    @dataMember()
    @required({ nullable: true })
    public fromTariff: Tariff | null;

    @dataMember()
    @required({ nullable: true })
    public fromStatus: ParkingStatus | null;

    @dataMember()
    @required()
    public toTariff: Tariff;

    @dataMember()
    @required()
    public toStatus: ParkingStatus;
}
