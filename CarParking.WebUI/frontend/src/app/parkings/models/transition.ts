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

export class TransitionReadModel {
    @dataMember()
    @required()
    @typedArray(Transition)
    public transitions: Transition[];
}
