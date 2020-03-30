import { ParkingTypeKey } from '../models/parking';

export class ParkingsFilter {
    constructor(public readonly parkingTypeKeys: ReadonlySet<ParkingTypeKey>) {
    }

    public setParkingTypes(parkingTypeKeys: ReadonlySet<ParkingTypeKey>): ParkingsFilter {
        return new ParkingsFilter(parkingTypeKeys);
    }
}
