import { Injectable, OnDestroy } from '@angular/core';
import { ReplaySubject } from 'rxjs';

export const assertUnhandledType = (type: never) => {
    throw new Error(`Unhandled type: ${type}`);
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
