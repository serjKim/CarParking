import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ParkingsStorage } from 'src/app/parkings/parkings.storage';
import { Parking, ParkingType } from '../../../models/parking';
import { CompleteButtonEvent, ToolboxButtonEvent } from '../toolbox-button-event';

@Component({
    selector: 'complete-button',
    templateUrl: './complete-button.component.html',
    styleUrls: ['../toolbox-button.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompleteButtonComponent {
    @Input()
    public parking: Parking;

    @Input()
    public disabled: boolean;

    @Output()
    public complete = new EventEmitter<ToolboxButtonEvent>();

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
