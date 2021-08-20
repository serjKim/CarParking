import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { notNullOrFail } from '../../../../extensions';
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
    public set parking(val: Parking) {
        this.currentParking = val;
    }

    public get parking(): Parking {
        return notNullOrFail(this.currentParking);
    }

    @Input()
    public disabled = false;

    private currentParking: Parking | null = null;

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
