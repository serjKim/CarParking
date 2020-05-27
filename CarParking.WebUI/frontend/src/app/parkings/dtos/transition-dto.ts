import { dataMember, required } from 'santee-dcts';
import { ParkingStatus, Tariff } from '../models/parking';
import { TransitionName } from '../models/transition';

export class TransitionDto {
    @dataMember()
    @required()
    public name: TransitionName = 'StartedFree';

    @dataMember()
    @required({ nullable: true })
    public fromTariff: Tariff | null = null;

    @dataMember()
    @required({ nullable: true })
    public fromStatus: ParkingStatus | null = null;

    @dataMember()
    @required()
    public toTariff: Tariff = Tariff.Free;

    @dataMember()
    @required()
    public toStatus: ParkingStatus = ParkingStatus.Started;
}
