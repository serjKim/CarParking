import { TransitionName } from '../models/transition';

export class ParkingsFilter {
    constructor(public readonly transitionNames: ReadonlySet<TransitionName>) {
    }

    public setTransitionNames(transitionNames: ReadonlySet<TransitionName>): ParkingsFilter {
        return new ParkingsFilter(transitionNames);
    }
}
