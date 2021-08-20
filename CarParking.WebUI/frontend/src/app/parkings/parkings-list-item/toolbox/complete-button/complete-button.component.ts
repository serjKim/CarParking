import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ParkingsStorage } from 'src/app/parkings/parkings.storage';
import { notNullOrFail } from '../../../../extensions';
import { Parking, ParkingType } from '../../../models';
import { CompleteButtonEvent, ToolboxButtonEvent } from '../toolbox-button-event';

@Component({
    selector: 'complete-button',
    templateUrl: './complete-button.component.html',
    styleUrls: ['../toolbox-button.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompleteButtonComponent {
    @Input()
    public set parking(val: Parking) {
        this.currentParking = val;
    }

    public get parking(): Parking {
        return notNullOrFail(this.currentParking);
    }

    @Input()
    public disabled = false;

    @Output()
    public complete = new EventEmitter<ToolboxButtonEvent>();

    private currentParking: Parking | null = null;

    constructor(
        private readonly parkingsStorage: ParkingsStorage,
    ) { }

    public async onComplete(e: MouseEvent) {
        e.stopPropagation();

        if (this.disabled) {
            return;
        }

        if (this.parking.type === ParkingType.StartedFree) {
            const result = await this.parkingsStorage.completeParking(this.parking);
            this.complete.emit(new CompleteButtonEvent(result.type));
        }
    }
}
