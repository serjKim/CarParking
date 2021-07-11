import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { StartedInfo } from './started-info';

@Component({
    selector: 'started-info',
    templateUrl: './started-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StartedInfoComponent {
    @Input()
    public info: StartedInfo | undefined | null = null;
}
