import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { notNullOrFail } from '../../../../extensions';
import { Parking } from '../../../models';

@Component({
    selector: 'started-info',
    templateUrl: './started-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StartedInfoComponent {
    @Input()
    public set parking(prk: Parking) {
        this.currentParking = prk;
    }

    public get parking(): Parking {
        return notNullOrFail(this.currentParking);
    }

    private currentParking: Parking | null = null;
}
