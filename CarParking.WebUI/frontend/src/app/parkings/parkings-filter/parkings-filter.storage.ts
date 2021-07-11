import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ParkingType } from '../models';
import { TRANSITION_NAMES, TransitionName } from '../models/transition';
import { ParkingsFilter } from './parking-filter';

export interface ParkingsFilterQueryParams extends Params {
    readonly transitions?: string | null;
}

@Injectable()
export class ParkingsFilterStorage {

    public get filter(): Observable<ParkingsFilter> {
        return this.filter$;
    }
    private readonly filter$: Observable<ParkingsFilter>;

    constructor(activatedRoute: ActivatedRoute, private readonly router: Router) {
        this.filter$ = activatedRoute.queryParams
            .pipe(
                map(params => this.deserializeFilter(params)),
            );
    }

    public applyFilter(filter: ParkingsFilter) {
        this.router.navigate([], {
            queryParams: this.serializeFilter(filter),
        });
    }

    public toHttpParams(filter: ParkingsFilter): HttpParams {
        const httpParams = new HttpParams();
        const queryParams = this.serializeFilter(filter);

        if (!!queryParams.transitions) {
            return new HttpParams({ fromObject: queryParams });
        }

        return httpParams;
    }

    private serializeFilter(filter: ParkingsFilter): ParkingsFilterQueryParams {
        return {
            transitions: filter.transitionNames.size > 0
                ? this.serializeTransitionNames(filter.transitionNames)
                : null,
        };
    }

    private deserializeFilter(params: Params): ParkingsFilter {
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
