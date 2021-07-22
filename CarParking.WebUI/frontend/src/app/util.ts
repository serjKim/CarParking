import { Injectable, OnDestroy } from '@angular/core';
import { ReplaySubject } from 'rxjs';

export const errorUnhandledType = (obj: never) => {
    console.error('Unhandled type source: ', obj);
    return new Error('Unhandled type.');
};

@Injectable()
export class NgDestroyer extends ReplaySubject<void> implements OnDestroy {
    constructor() {
        super(1);
    }
    public ngOnDestroy(): void {
        this.next();
        this.complete();
    }
}

export const notNullOrFail = <T>(value: T | null | undefined): T => {
    if (value == null) {
        throw new Error('Expected a not null value.');
    }
    return value;
};
