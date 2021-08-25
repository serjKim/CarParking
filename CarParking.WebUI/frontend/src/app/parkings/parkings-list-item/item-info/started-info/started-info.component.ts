import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NotNullProperty } from '../../../../extensions';
import { Parking } from '../../../models';

@Component({
    selector: 'started-info',
    templateUrl: './started-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StartedInfoComponent {
    @Input()
    @NotNullProperty()
    public set parking(_: Parking) {}
}
