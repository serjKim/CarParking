import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ParkingsFilter } from './parkings-filter';
import { ParkingsFilterSerializer } from './parkings-filter-serializer';

export interface ParkingsFilterQueryParams extends Params {
    readonly transitions?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ParkingsFilterRouter {
    public readonly filter: Observable<ParkingsFilter>;

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly router: Router,
        private readonly filterSerializer: ParkingsFilterSerializer,
    ) {
        this.filter = activatedRoute.queryParams
            .pipe(
                map(params => this.filterSerializer.deserializeFilter(params)),
            );
    }

    public applyFilter(filter: ParkingsFilter) {
        this.router.navigate([], {
            queryParams: this.filterSerializer.serializeFilter(filter),
        });
    }

    public toHttpParams(filter: ParkingsFilter): HttpParams {
        const httpParams = new HttpParams();
        const queryParams = this.filterSerializer.serializeFilter(filter);

        if (!!queryParams.transitions) {
            return new HttpParams({ fromObject: queryParams });
        }

        return httpParams;
    }
}
