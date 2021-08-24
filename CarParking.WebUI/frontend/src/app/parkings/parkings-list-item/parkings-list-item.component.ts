import { ChangeDetectionStrategy, Component, HostListener, Input } from '@angular/core';
import { notNullOrFail } from '../../extensions';
import { Parking } from '../models';
import { createSelectedInfo, InfoType, SelectedInfo } from './selected-info';

@Component({
    selector: 'parkings-list-item',
    templateUrl: './parkings-list-item.component.html',
    styleUrls: ['./parkings-list-item.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListItemComponent {
    public readonly intoType = InfoType;
    public selectedInfo = SelectedInfo.started();

    @Input()
    public set parking(prk: Parking) {
        this.selectedInfo = createSelectedInfo(prk.type);
        this.currentParking = prk;
    }

    public get parking(): Parking {
        return notNullOrFail(this.currentParking);
    }

    private currentParking: Parking | null = null;

    @HostListener('click')
    public onSwitchInfo() {
        this.selectedInfo.trySwitch();
    }
}
