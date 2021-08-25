import { Injectable } from '@angular/core';
import { Params } from '@angular/router';
import { ParkingType, TRANSITION_NAMES, TransitionName } from '../models';
import { ParkingsFilter } from './parkings-filter';
import { ParkingsFilterQueryParams } from './parkings-filter-router';

@Injectable({ providedIn: 'root' })
export class ParkingsFilterSerializer {
    public serializeFilter(filter: ParkingsFilter): ParkingsFilterQueryParams {
        return {
            transitions: filter.transitionNames.size > 0
                ? this.serializeTransitionNames(filter.transitionNames)
                : null,
        };
    }

    public deserializeFilter(params: Params): ParkingsFilter {
        const keys = this.deserializeTransitionNames(params);
        return new ParkingsFilter(keys);
    }

    private deserializeTransitionNames = (params: Params): ReadonlySet<TransitionName> => {
        const transitions: string | null | undefined = (params as ParkingsFilterQueryParams).transitions;
        const transitionNames = transitions?.split(',').filter(this.isTransitionName) ?? [];

        return new Set(transitionNames);
    }

    private isTransitionName = (raw: string): raw is TransitionName => {
        return TRANSITION_NAMES.has(raw as keyof typeof ParkingType);
    }

    private serializeTransitionNames(names: ReadonlySet<TransitionName>): string {
        return Array.from(names).join(',');
    }
}
