import { Injectable, OnDestroy } from '@angular/core';
import { combineLatest, Subscription } from 'rxjs';
import { ParkingsFilter } from './parkings-filter';
import { ParkingsFilterRouter } from './parkings-filter-router';
import { TransitionButton, TransitionButtonsStorage } from './transition-buttons';

@Injectable({ providedIn: 'root' })
export class SyncButtonsWithFilter implements OnDestroy {
    private readonly subscription = new Subscription();

    constructor(
        private readonly parkingsFilterRouter: ParkingsFilterRouter,
        private readonly transitionButtonsStorage: TransitionButtonsStorage,
    ) {}

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    public register() {
        this.subscription.add(
            combineLatest([
                this.parkingsFilterRouter.filter,
                this.transitionButtonsStorage.buttons,
            ]).subscribe(this.setButtonValues),
        );
    }

    private setButtonValues = ([filter, transitionButtons]: [ParkingsFilter, readonly TransitionButton[]]) => {
        for (const button of transitionButtons) {
            const selected = filter.transitionNames.has(button.transitionName);
            button.control.setValue(selected);
        }
    }
}
