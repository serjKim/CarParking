import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Parking, ParkingType } from '../../../models/parking';
import { ParkingsStorage } from '../../../parkings.storage';

@Component({
    selector: 'pay-button',
    templateUrl: './pay-button.component.html',
    styleUrls: ['../toolbox-button.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayButtonComponent {
    @Input()
    public parking!: Parking;

    @Input()
    public disabled!: boolean;

    constructor(
        private readonly parkingsStorage: ParkingsStorage,
    ) { }

    public async onPay(e: MouseEvent) {
        e.stopPropagation();

        if (this.disabled) {
            return;
        }

        if (this.parking.type === ParkingType.StartedFree) {
            await this.parkingsStorage.payParking(this.parking);
        }
    }
}
