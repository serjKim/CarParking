import { ParkingStatus, ParkingType, Tariff } from './parking';

export type TransitionName = keyof (typeof ParkingType);
const keys = Object.keys(ParkingType) as TransitionName[];
export const TRANSITION_NAMES = new Set(keys);

export class Transition {
    constructor(
        public readonly name: TransitionName,
        public readonly fromTariff: Tariff | null,
        public readonly fromStatus: ParkingStatus | null,
        public readonly toTariff: Tariff,
        public readonly toStatus: ParkingStatus,
    ) { }
}
