import { ChangeDetectionStrategy, Component, HostListener, Input } from '@angular/core';
import { NotNullProperty } from '../../extensions';
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
    @NotNullProperty()
    public set parking(prk: Parking) {
        this.selectedInfo = createSelectedInfo(prk.type);
    }

    @HostListener('click')
    public onSwitchInfo() {
        this.selectedInfo.trySwitch();
    }
}
