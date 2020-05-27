import { dataMember, required, typedArray } from 'santee-dcts';
import { ParkingStatus, Tariff } from './parking';

type TransitionNames = [
    'StartedFree',
    'CompletedFree',
    'CompletedFirst',
];

export type TransitionName = TransitionNames extends Array<infer T> ? T : never;

const transitionNamesArray: TransitionNames = ['StartedFree', 'CompletedFree', 'CompletedFirst'];

export const TRANSITION_NAMES = new Set(transitionNamesArray);

export class Transition {
    constructor(
        public readonly name: TransitionName,
        public readonly fromTariff: Tariff | null,
        public readonly fromStatus: ParkingStatus | null,
        public readonly toTariff: Tariff,
        public readonly toStatus: ParkingStatus,
    ) { }
}
