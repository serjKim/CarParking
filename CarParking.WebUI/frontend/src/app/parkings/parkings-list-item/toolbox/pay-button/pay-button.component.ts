import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Parking, ParkingType } from '../../../models';
import { ParkingsStorage } from '../../../parkings.storage';

@Component({
    selector: 'pay-button',
    templateUrl: './pay-button.component.html',
    styleUrls: ['../toolbox-button.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayButtonComponent {
    @Input()
    public parking: Parking | null = null;

    @Input()
    public disabled = false;

    constructor(
        private readonly parkingsStorage: ParkingsStorage,
    ) { }

    public async onPay(e: MouseEvent) {
        e.stopPropagation();

        if (this.disabled) {
            return;
        }

        if (this.parking && this.parking.type === ParkingType.StartedFree) {
            await this.parkingsStorage.payParking(this.parking);
        }
    }
}
