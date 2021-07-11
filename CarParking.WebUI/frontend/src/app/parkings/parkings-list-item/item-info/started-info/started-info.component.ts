import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { StartedFree } from '../../../models';

@Component({
    selector: 'started-info',
    templateUrl: './started-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StartedInfoComponent {
    @Input()
    public parking!: StartedFree;
}
